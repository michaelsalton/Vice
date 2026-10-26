using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vice.Editor
{
    public class MaskBakeWindow : EditorWindow
    {
        Material sourceMaterial;
        Texture2D maskOverride;

        [MenuItem("Vice/Mask Baker")]
        static void Open()
        {
            GetWindow<MaskBakeWindow>("Mask Baker");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox("Bakes a Leartes S_GeneralORM material's mask layers into a base colour texture, written next to the mask as *_D.png.", MessageType.None);
            sourceMaterial = (Material)EditorGUILayout.ObjectField("Source Material", sourceMaterial, typeof(Material), false);
            maskOverride = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Mask Override", "Use this mask file instead of the material's _Mask; also decides where the output lands."), maskOverride, typeof(Texture2D), false);

            if (sourceMaterial != null && !sourceMaterial.HasProperty("_Mask"))
            {
                EditorGUILayout.HelpBox("This material's shader has no _Mask property; pick one still on the Leartes shader.", MessageType.Warning);
                return;
            }

            using (new EditorGUI.DisabledScope(sourceMaterial == null))
            {
                if (GUILayout.Button("Bake"))
                    Bake();
            }
        }

        void Bake()
        {
            Texture2D maskAsset = maskOverride != null ? maskOverride : sourceMaterial.GetTexture("_Mask") as Texture2D;
            if (maskAsset == null)
            {
                Debug.LogError("Mask Baker: the material has no mask texture and no override was given.");
                return;
            }

            string maskPath = AssetDatabase.GetAssetPath(maskAsset);
            string outputPath = OutputPathFor(maskPath);
            if (File.Exists(outputPath) && !EditorUtility.DisplayDialog("Mask Baker", outputPath + " already exists. Overwrite it?", "Overwrite", "Cancel"))
                return;

            Texture2D mask = LoadFullResolution(maskAsset, out bool maskIsSRGB);
            Texture2D baseAsset = sourceMaterial.GetTexture("_BaseMap") as Texture2D;
            bool baseIsSRGB = false;
            // an empty base slot samples as white in the graph, and white is 1.0 in either colour space
            Texture2D baseMap = baseAsset != null ? LoadFullResolution(baseAsset, out baseIsSRGB) : Texture2D.whiteTexture;
            if (mask == null || baseMap == null)
                return;

            Material bakeMaterial = new Material(Shader.Find("Hidden/Vice/MaskBake"));
            bakeMaterial.SetTexture("_Mask", mask);
            bakeMaterial.SetFloat("_MaskIsSRGB", maskIsSRGB ? 1f : 0f);
            bakeMaterial.SetFloat("_BaseIsSRGB", baseIsSRGB ? 1f : 0f);
            // .mat files store colours as authored (sRGB); SetVector skips Unity's own conversion, so convert explicitly
            bakeMaterial.SetVector("_BaseColorValue", sourceMaterial.GetColor("_BaseColor_Value").linear);
            bakeMaterial.SetVector("_MaskTint", sourceMaterial.GetColor("_Mask_Color").linear);
            bakeMaterial.SetVector("_DirtColor", sourceMaterial.GetColor("_DirtColor").linear);
            bakeMaterial.SetVector("_LeakColor", sourceMaterial.GetColor("_LeakColor").linear);
            bakeMaterial.SetVector("_EdgeDamageColor", sourceMaterial.GetColor("_EdgeDamageColor").linear);
            bakeMaterial.SetFloat("_MaskColorEnabled", sourceMaterial.GetFloat("_MaskColor"));
            bakeMaterial.SetFloat("_MaskDirtEnabled", sourceMaterial.GetFloat("_MaskDirt"));

            // Linear read/write means no hidden conversion on output; the shader already encoded sRGB
            RenderTexture target = RenderTexture.GetTemporary(mask.width, mask.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(baseMap, target, bakeMaterial);

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D result = new Texture2D(target.width, target.height, TextureFormat.RGB24, false, true);
            result.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            result.Apply();
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(target);

            File.WriteAllBytes(outputPath, result.EncodeToPNG());

            DestroyImmediate(result);
            DestroyImmediate(mask);
            if (baseAsset != null)
                DestroyImmediate(baseMap);
            DestroyImmediate(bakeMaterial);

            AssetDatabase.ImportAsset(outputPath);
            TextureImporter outputImporter = (TextureImporter)AssetImporter.GetAtPath(outputPath);
            TextureImporter maskImporter = (TextureImporter)AssetImporter.GetAtPath(maskPath);
            outputImporter.sRGBTexture = true;
            outputImporter.maxTextureSize = maskImporter.maxTextureSize;
            outputImporter.SaveAndReimport();

            Debug.Log("Mask Baker: wrote " + outputPath, AssetDatabase.LoadAssetAtPath<Texture2D>(outputPath));
        }

        static string OutputPathFor(string maskPath)
        {
            string directory = Path.GetDirectoryName(maskPath);
            string name = Path.GetFileNameWithoutExtension(maskPath);
            string stem = name.EndsWith("_M") ? name.Substring(0, name.Length - 2) : name;
            return Path.Combine(directory, stem + "_D.png").Replace('\\', '/');
        }

        static Texture2D LoadFullResolution(Texture2D asset, out bool isSRGB)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            isSRGB = ((TextureImporter)AssetImporter.GetAtPath(path)).sRGBTexture;

            // read the source file, skipping the importer's size cap and compression
            Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            if (!decoded.LoadImage(File.ReadAllBytes(path)))
            {
                Debug.LogError("Mask Baker: couldn't decode " + path + " (LoadImage reads PNG and JPG only).");
                DestroyImmediate(decoded);
                return null;
            }

            // copy into a texture known to be linear, so the GPU hands the shader raw bytes
            Texture2D raw = new Texture2D(decoded.width, decoded.height, TextureFormat.RGBA32, false, true);
            raw.SetPixels32(decoded.GetPixels32());
            raw.Apply();
            DestroyImmediate(decoded);
            return raw;
        }
    }
}
