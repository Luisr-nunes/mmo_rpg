import os

meta_path = r"C:\Users\luisr\OneDrive\Desktop\mmo_rpg\client_unity\Assets\Resources\Icon_Wood.png.meta"

if os.path.exists(meta_path):
    with open(meta_path, "r") as f:
        content = f.read()
    
    # Force textureType to 8 (Sprite)
    import re
    content = re.sub(r"textureType: \d", "textureType: 8", content)
    
    with open(meta_path, "w") as f:
        f.write(content)
    print("Meta file updated to Sprite!")
else:
    print("Meta file not found yet.")
