import asyncio
import json
import logging
import uuid
import time
import random
import websockets

logging.basicConfig(level=logging.INFO)

# Estado do Jogo
game_state = {
    "players": {},       # { player_id: { x, y, name, inventory } }
    "resources": {},     # { resource_id: { type, x, y, active, respawn_time } }
}

WORLD_WIDTH = 800
WORLD_HEIGHT = 600
RESOURCE_RESPAWN_DELAY = 10

def init_resources():
    for i in range(5):
        res_id = str(uuid.uuid4())
        game_state["resources"][res_id] = {
            "type": "wood",
            "x": 100 + i * 150,
            "y": 300,
            "active": True,
            "respawn_time": 0
        }

init_resources()

connected_clients = set()

def get_network_state():
    players_list = []
    for k, v in game_state["players"].items():
        players_list.append({"id": k, "name": v["name"], "x": float(v["x"]), "y": float(v["y"])})
        
    resources_list = []
    for k, v in game_state["resources"].items():
        resources_list.append({"id": k, "type": v["type"], "x": float(v["x"]), "y": float(v["y"]), "active": v["active"]})
        
    return {
        "players": players_list,
        "resources": resources_list
    }

async def broadcast(message):
    if connected_clients:
        await asyncio.gather(*[client.send(message) for client in connected_clients])

async def game_loop():
    while True:
        current_time = time.time()
        resources_changed = False
        
        for res_id, res_data in game_state["resources"].items():
            if not res_data["active"] and current_time >= res_data["respawn_time"]:
                res_data["active"] = True
                resources_changed = True
                
        if resources_changed:
            await broadcast(json.dumps({"type": "game_state", "state": get_network_state()}))
            
        await asyncio.sleep(1)

async def handler(websocket):
    player_id = str(uuid.uuid4())
    connected_clients.add(websocket)
    
    game_state["players"][player_id] = {
        "x": WORLD_WIDTH // 2,
        "y": WORLD_HEIGHT // 2,
        "name": f"Player_{player_id[:4]}",
        "inventory": {"wood": 0, "stone": 0}
    }
    
    try:
        await websocket.send(json.dumps({
            "type": "init",
            "id": player_id,
            "state": get_network_state()
        }))
        
        await broadcast(json.dumps({"type": "game_state", "state": get_network_state()}))

        async for message in websocket:
            data = json.loads(message)
            
            if data["type"] == "move":
                game_state["players"][player_id]["x"] = max(0, min(WORLD_WIDTH, data["x"]))
                game_state["players"][player_id]["y"] = max(0, min(WORLD_HEIGHT, data["y"]))
                
                await broadcast(json.dumps({"type": "game_state", "state": get_network_state()}))
                
            elif data["type"] == "collect":
                res_id = data["resource_id"]
                if res_id in game_state["resources"]:
                    res = game_state["resources"][res_id]
                    if res["active"]:
                        px = game_state["players"][player_id]["x"]
                        py = game_state["players"][player_id]["y"]
                        dist = ((px - res["x"])**2 + (py - res["y"])**2)**0.5
                        
                        if dist < 60:
                            # Regra de Dados (D&D)
                            roll = random.randint(1, 20)
                            amount_collected = 0
                            
                            if roll == 1:
                                amount_collected = 0 # Falha Crítica
                            elif roll <= 14:
                                amount_collected = 1 # Sucesso Normal
                            elif roll <= 19:
                                amount_collected = 2 # Sucesso Bom
                            elif roll == 20:
                                amount_collected = 3 # Sucesso Crítico
                                
                            if amount_collected > 0:
                                res["active"] = False
                                res["respawn_time"] = time.time() + RESOURCE_RESPAWN_DELAY
                                game_state["players"][player_id]["inventory"][res["type"]] += amount_collected
                            
                            # Transmitir rolagem
                            await broadcast(json.dumps({
                                "type": "dice_roll",
                                "player_id": player_id,
                                "roll": roll,
                                "amount": amount_collected,
                                "x": px,
                                "y": py
                            }))
                            
                            await broadcast(json.dumps({"type": "game_state", "state": get_network_state()}))
                            
    except websockets.exceptions.ConnectionClosed:
        pass
    finally:
        connected_clients.remove(websocket)
        if player_id in game_state["players"]:
            del game_state["players"][player_id]
        await broadcast(json.dumps({"type": "game_state", "state": get_network_state()}))

async def main():
    server = await websockets.serve(handler, "localhost", 8765)
    await asyncio.gather(server.wait_closed(), game_loop())

if __name__ == "__main__":
    asyncio.run(main())
