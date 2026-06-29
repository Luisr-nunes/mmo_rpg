from PIL import Image

# The fully clothed character sheet from Mystic Woods
sheet_path = r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\visual\pack_02_Mystic_Woods\characters\player.png"
img = Image.open(sheet_path)

# Frame size is 48x48. Each animation has 6 frames (288 width).
# Row 0: Idle Down
# Row 1: Idle Side (Right)
# Row 2: Idle Up
# Row 3: Walk Down
# Row 4: Walk Side (Right)
# Row 5: Walk Up

# Define the rows for each animation
animations = {
    "Idle_Down": 0,
    "Idle_Side": 1,
    "Idle_Up": 2,
    "Walk_Down": 3,
    "Walk_Side": 4,
    "Walk_Up": 5
}

output_dir = r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources"

for name, row in animations.items():
    # Crop the row (288x48)
    box = (0, row * 48, 288, (row + 1) * 48)
    row_img = img.crop(box)
    
    # Save as -Sheet.png to seamlessly replace the Pixel Crawler files
    row_img.save(f"{output_dir}\\{name}-Sheet.png")

print("Character swapped successfully! Naked character is gone.")
