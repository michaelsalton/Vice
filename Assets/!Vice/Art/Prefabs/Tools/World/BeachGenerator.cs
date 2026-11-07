using UnityEngine;
using UnityEngine.Rendering;

namespace Vice
{
    // builds a sand mesh from a cross-shore profile; the transform's origin sits on the mean waterline at water height
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BeachGenerator : MonoBehaviour
    {
        const string MeshName = "BeachGenerator";

        [Header("Extent")]
        [Tooltip("Metres along the shoreline, on local X.")]
        [Min(1f)]
        [SerializeField] float length = 80f;

        [Tooltip("Metres of sand inland of the waterline, on local +Z. Should reach past the berm crest (Berm Height x Foreshore Run).")]
        [Min(1f)]
        [SerializeField] float inlandExtent = 35f;

        [Tooltip("Metres of seabed out from the waterline, on local -Z. Must reach deeper than the water material's Depth Max or the mesh edge shows through.")]
        [Min(1f)]
        [SerializeField] float offshoreExtent = 40f;

        [Tooltip("Metres between vertices. Keep it well under the wobble wavelength or the shoreline curves turn into straight segments.")]
        [Range(0.1f, 4f)]
        [SerializeField] float vertexSpacing = 0.5f;

        [Header("Profile")]
        [Tooltip("Metres of dry berm above the water.")]
        [Range(0.1f, 4f)]
        [SerializeField] float bermHeight = 1f;

        [Tooltip("Foreshore slope as 1:N, metres inland per metre of rise. 10 is steep coarse sand, 50 is flat fine sand.")]
        [Range(5f, 60f)]
        [SerializeField] float foreshoreRun = 25f;

        [Tooltip("Metres out from the waterline where the gentle foreshore gives way to the offshore drop.")]
        [Range(0f, 30f)]
        [SerializeField] float surfWidth = 8f;

        [Tooltip("Offshore drop slope as 1:N.")]
        [Range(1f, 20f)]
        [SerializeField] float dropRun = 6f;

        [Header("Shoreline")]
        [Tooltip("Metres of height noise along the shore. The slope stretches it sideways by Foreshore Run, so 0.04 at 1:25 moves the waterline a metre.")]
        [Range(0f, 0.3f)]
        [SerializeField] float shorelineWobble = 0.04f;

        [Tooltip("Metres between bays along the shore.")]
        [Range(2f, 100f)]
        [SerializeField] float wobbleWavelength = 25f;

        [SerializeField] int seed = 1;

        [Header("UVs")]
        [Tooltip("World metres per texture repeat on uv0.")]
        [Min(0.0001f)]
        [SerializeField] float metresPerTile = 2f;

        void OnEnable() => Generate();

        // deferred because a mesh can't be rebuilt inside OnValidate
        void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () => { if (this != null && isActiveAndEnabled) Generate(); };
#endif
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            int columns = Mathf.Max(1, Mathf.CeilToInt(length / vertexSpacing));
            int rows = Mathf.Max(1, Mathf.CeilToInt((inlandExtent + offshoreExtent) / vertexSpacing));
            int columnVertices = columns + 1;
            int rowVertices = rows + 1;

            var vertices = new Vector3[columnVertices * rowVertices];
            var uvs = new Vector2[vertices.Length];
            // a fixed offset into Perlin's plane per seed, so different seeds give different bays
            float noiseOffset = seed * 17.31f;

            for (int row = 0; row < rowVertices; row++)
            {
                float inland = Mathf.Lerp(-offshoreExtent, inlandExtent, row / (float)rows);
                for (int column = 0; column < columnVertices; column++)
                {
                    float alongShore = Mathf.Lerp(-length * 0.5f, length * 0.5f, column / (float)columns);
                    int index = row * columnVertices + column;
                    vertices[index] = new Vector3(alongShore, BeachHeight(inland, alongShore, noiseOffset), inland);
                    uvs[index] = new Vector2(alongShore, inland) / metresPerTile;
                }
            }

            var triangles = new int[columns * rows * 6];
            int triangle = 0;
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                int bottomLeft = row * columnVertices + column;
                int topLeft = bottomLeft + columnVertices;
                // clockwise seen from above, so the top face is the front face
                triangles[triangle++] = bottomLeft;
                triangles[triangle++] = topLeft;
                triangles[triangle++] = bottomLeft + 1;
                triangles[triangle++] = bottomLeft + 1;
                triangles[triangle++] = topLeft;
                triangles[triangle++] = topLeft + 1;
            }

            var filter = GetComponent<MeshFilter>();
            Mesh mesh = filter.sharedMesh;
            // the name check keeps this from ever clearing an imported mesh asset
            if (mesh == null || mesh.name != MeshName)
            {
                mesh = new Mesh { name = MeshName };
                filter.sharedMesh = mesh;
            }
            else mesh.Clear();

            mesh.indexFormat = vertices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            // the slope and the wobble only shade if the normals know about them
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            // the player walks on this, so the collider has to follow every rebuild
            if (TryGetComponent(out MeshCollider meshCollider))
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
            }
        }

        // inland: metres from the mean waterline, positive toward land; returns height above the water
        float BeachHeight(float inland, float alongShore, float noiseOffset)
        {
            // centimetres of height noise, which the gentle slope stretches into metres of shoreline curve
            float wobble = (Mathf.PerlinNoise(alongShore / wobbleWavelength + noiseOffset, noiseOffset) - 0.5f) * 2f * shorelineWobble;
            float foreshore = inland / foreshoreRun + wobble;
            // a steeper line meeting the foreshore at the bottom of the surf zone; min() takes it beyond that point
            float offshore = (inland + surfWidth) / dropRun - surfWidth / foreshoreRun;
            float seabed = SmoothMin(foreshore, offshore, 0.5f);
            return SmoothMin(seabed, bermHeight, 0.4f);
        }

        // polynomial smooth minimum: within `blend` metres of the crossing it eases between the two lines instead of creasing
        static float SmoothMin(float first, float second, float blend)
        {
            float weight = Mathf.Clamp01(0.5f + 0.5f * (second - first) / blend);
            return Mathf.Lerp(second, first, weight) - blend * weight * (1f - weight);
        }
    }
}
