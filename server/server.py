import asyncio
import json
import logging
import uuid
import time
import websockets

logging.basicConfig(level=logging.INFO)

# Estado do Jogo (Memória do Servidor Autoritativo)
game_state = {
    "players": {},       # { player_id: { x, y, name, class } }
    "resources": {},     # { resource_id: { type, x, y, active, respawn_time } }
}

# Configurações do Jogo
WORLD_WIDTH = 800
WORLD_HEIGHT = 600
RESOURCE_RESPAWN_DELAY = 10  # 10 segundos para o recurso de Tier 1 reaparecer

# Gerar alguns recursos iniciais
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

# Conexões Ativas
connected_clients = set()

async def broadcast(message):
    if connected_clients:
        await asyncio.gather(*[client.send(message) for client in connected_clients])

async def game_loop():
    """Loop principal do servidor para lidar com eventos temporais (ex: respawn)"""
    while True:
        current_time = time.time()
        resources_changed = False
        
        for res_id, res_data in game_state["resources"].items():
            if not res_data["active"] and current_time >= res_data["respawn_time"]:
                res_data["active"] = True
                resources_changed = True
                logging.info(f"Recurso {res_id} renasceu.")
                
        if resources_changed:
            await broadcast(json.dumps({
                "type": "game_state",
                "state": game_state
            }))
            
        await asyncio.sleep(1) # Roda a cada segundo

async def handler(websocket):
    # Registrar novo jogador
    player_id = str(uuid.uuid4())
    connected_clients.add(websocket)
    
    # Spawn no centro do mapa
    game_state["players"][player_id] = {
        "x": WORLD_WIDTH // 2,
        "y": WORLD_HEIGHT // 2,
        "name": f"Player_{player_id[:4]}",
        "score": 0
    }
    
    logging.info(f"Novo jogador conectado: {player_id}")
    
    try:
        # Enviar estado inicial para o jogador que acabou de conectar
        await websocket.send(json.dumps({
            "type": "init",
            "id": player_id,
            "state": game_state
        }))
        
        # Avisar a todos que o estado mudou (novo jogador)
        await broadcast(json.dumps({
            "type": "game_state",
            "state": game_state
        }))

        # Escutar eventos do cliente
        async for message in websocket:
            data = json.loads(message)
            
            if data["type"] == "move":
                # Validação autoritativa muito básica
                new_x = max(0, min(WORLD_WIDTH, data["x"]))
                new_y = max(0, min(WORLD_HEIGHT, data["y"]))
                
                game_state["players"][player_id]["x"] = new_x
                game_state["players"][player_id]["y"] = new_y
                
                # Broadcast do novo estado (em um jogo real, usaríamos interpolação/ticks, não broadcast a cada movimento)
                await broadcast(json.dumps({
                    "type": "game_state",
                    "state": game_state
                }))
                
            elif data["type"] == "collect":
                # Jogador tentou coletar um recurso
                res_id = data["resource_id"]
                if res_id in game_state["resources"]:
                    res = game_state["resources"][res_id]
                    if res["active"]:
                        # Calcular distância para evitar cheats
                        px = game_state["players"][player_id]["x"]
                        py = game_state["players"][player_id]["y"]
                        dist = ((px - res["x"])**2 + (py - res["y"])**2)**0.5
                        
                        if dist < 50: # Distância máxima para coletar
                            res["active"] = False
                            res["respawn_time"] = time.time() + RESOURCE_RESPAWN_DELAY
                            game_state["players"][player_id]["score"] += 1
                            logging.info(f"Jogador {player_id} coletou {res_id}.")
                            
                            await broadcast(json.dumps({
                                "type": "game_state",
                                "state": game_state
                            }))
                            
    except websockets.exceptions.ConnectionClosed:
        pass
    finally:
        # Remover jogador ao desconectar
        logging.info(f"Jogador desconectado: {player_id}")
        connected_clients.remove(websocket)
        del game_state["players"][player_id]
        
        await broadcast(json.dumps({
            "type": "game_state",
            "state": game_state
        }))

async def main():
    # Inicia o servidor Websocket e o loop do jogo em background
    server = await websockets.serve(handler, "localhost", 8765)
    logging.info("Servidor MMO rodando em ws://localhost:8765")
    
    # Roda o servidor e o game_loop concorrentemente
    await asyncio.gather(
        server.wait_closed(),
        game_loop()
    )

if __name__ == "__main__":
    asyncio.run(main())
