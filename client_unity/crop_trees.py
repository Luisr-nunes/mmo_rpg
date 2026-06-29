from PIL import Image

img_path = r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources\Trees.png"
img = Image.open(img_path)

# Crop the Green Tree
tree_box = (0, 0, 48, 96)
tree_img = img.crop(tree_box)

tree_padded = Image.new("RGBA", (48, 96), (0, 0, 0, 0))
tree_padded.paste(tree_img, (0, 0))
tree_padded.putpixel((0, 0), (0, 0, 0, 1))
tree_padded.putpixel((47, 95), (0, 0, 0, 1))

# Crop the Stump perfectly (moving 1 more pixel to the right to completely dodge the icicle)
stump_box = (191, 8, 208, 32)
stump_img = img.crop(stump_box)

stump_padded = Image.new("RGBA", (48, 96), (0, 0, 0, 0))

# We paste at 15 to keep it centered
stump_padded.paste(stump_img, (15, 72))

stump_padded.putpixel((0, 0), (0, 0, 0, 1))
stump_padded.putpixel((47, 95), (0, 0, 0, 1))

tree_padded.save(r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources\Tree_Alive_Perfect.png")
stump_padded.save(r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources\Tree_Stump_Perfect.png")

print("Crop updated and icicle erased!")
