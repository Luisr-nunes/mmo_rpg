from PIL import Image

img_path = r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources\Trees.png"
img = Image.open(img_path)

# Crop the Stump perfectly (start at 191 to avoid icicle, width is 17)
stump_box = (191, 8, 208, 32)
stump_img = img.crop(stump_box)

# Create a 32x32 icon and center the stump in it
icon = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
# stump is 17x24.
# x = (32 - 17) // 2 = 7
# y = (32 - 24) // 2 = 4
icon.paste(stump_img, (7, 4))

icon.save(r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources\Icon_Wood.png")
print("Icon_Wood created!")
