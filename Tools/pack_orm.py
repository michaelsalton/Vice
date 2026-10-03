import argparse
from PIL import Image

parser = argparse.ArgumentParser(description="Pack AO, roughness and metallic maps into one ORM texture (R=AO, G=Roughness, B=Metallic).")
parser.add_argument("--roughness", required=True)
parser.add_argument("--ao")
parser.add_argument("--metallic")
parser.add_argument("--out", required=True)
arguments = parser.parse_args()

roughness = Image.open(arguments.roughness).convert("L")
size = roughness.size


def load_or_fill(path, fillValue):
    if path is None:
        return Image.new("L", size, fillValue)
    return Image.open(path).convert("L").resize(size, Image.LANCZOS)


# missing AO means nothing is occluded
ambientOcclusion = load_or_fill(arguments.ao, 255)
# missing metallic means a dielectric
metallic = load_or_fill(arguments.metallic, 0)

# PNG, because JPEG's chroma subsampling would blur G and B into each other
Image.merge("RGB", (ambientOcclusion, roughness, metallic)).save(arguments.out)
print(f"Wrote {arguments.out} ({size[0]}x{size[1]})")
