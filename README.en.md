# MMO RPG

<!-- badges -->
![C#](https://img.shields.io/badge/C%23-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![Python](https://img.shields.io/badge/Python-3776AB?style=flat-square&logo=python&logoColor=white) ![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=white) ![WebSocket](https://img.shields.io/badge/WebSocket-010101?style=flat-square&logo=socketdotio&logoColor=white)

[Ler em Português](README.md)

Prototype of a pixel-art multiplayer RPG that blends MMO progression, D&D-style d20 rolls and a player-driven economy.

## About

The project is at the prototype stage. The current code validates the network loop and the first gameplay mechanic:

- an **authoritative server** in Python (WebSocket) holds the world state: players, positions, resources and inventories;
- a **Unity 2D client** connects to the server, sends movement and gather actions, and renders the state it receives;
- every gather action triggers a **d20 roll** on the server, broadcast to all players.

The full game vision (character creation, mastery by use, instanced dungeons, crafting, death system) is in the design document [MD_JOGO.md](MD_JOGO.md) (Portuguese).

## Implemented features

- Several players connected to the same world, with state synchronized every tick.
- Movement with WASD or arrow keys.
- Resource gathering (wood) with Space or Enter when the player is close enough.
- Gather result decided by a d20: 1 is a critical failure (nothing), 2–14 yields 1, 15–19 yields 2 and 20 is a critical success (3).
- Gathered resources respawn after 10 seconds.
- Per-player inventory shown in the UI.

## Tech stack

| Layer | Technology |
| --- | --- |
| Server | Python 3, `asyncio`, `websockets` |
| Client | Unity 2D with URP (C#), `System.Net.WebSockets`, Input System, TextMesh Pro, 2D Animation, Tilemap, Aseprite importer |
| Art | Third-party pixel art packs in `visual/` |
| Tooling | Python scripts in `client_unity/` for sprite cropping and metadata fixes |

## Structure

```
mmo_rpg/
├── MD_JOGO.md          # Game Design Document
├── server/
│   └── server.py       # WebSocket server (ws://localhost:8765)
├── client_unity/       # Unity project
│   ├── Assets/Scripts/ # GameManager, NetworkManager, PlayerController, InventoryUI
│   └── Packages/
└── visual/             # sprite packs used by the client
```

## Running

Requirements: Python 3.10+ and Unity Hub with a Unity 6 editor.

1. Start the server:

   ```bash
   pip install websockets
   python server/server.py
   ```

   It listens on `ws://localhost:8765`.

2. Open the `client_unity` folder in Unity Hub, load `Assets/Scenes/SampleScene` and press Play. To test multiplayer, make a build and run more than one instance.

> Unity's `ProjectSettings` folder is not versioned; the editor recreates it with default values when the project is opened.

## Related project

[mmo_rpg_godot](https://github.com/Luisr-nunes/mmo_rpg_godot) is an alternative Godot client that talks to the same server.

## Art credits

The sprites in `visual/` belong to their authors and follow each pack's license (see the terms files inside each folder).

## Author

Luís Nunes — [GitHub](https://github.com/Luisr-nunes) · [LinkedIn](https://www.linkedin.com/in/luis-nunes-dev/)
