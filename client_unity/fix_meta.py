import re

files = [
    r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources\Tree_Alive_Perfect.png.meta",
    r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources\Tree_Stump_Perfect.png.meta"
]

for file_path in files:
    with open(file_path, "r") as f:
        content = f.read()

    # Change spriteMode to 1 (Single)
    content = re.sub(r"spriteMode: \d", "spriteMode: 1", content)
    
    # Change alignment to 7 (Bottom)
    content = re.sub(r"alignment: \d", "alignment: 7", content)
    
    # Change spritePivot to {x: 0.5, y: 0} (Bottom)
    content = re.sub(r"spritePivot: \{.*?\}", "spritePivot: {x: 0.5, y: 0}", content)
    
    # Change spriteMeshType to 0 (Full Rect) to prevent cropping
    content = re.sub(r"spriteMeshType: \d", "spriteMeshType: 0", content)

    with open(file_path, "w") as f:
        f.write(content)

print("Meta files updated successfully!")
