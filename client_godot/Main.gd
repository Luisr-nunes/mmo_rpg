extends Node2D

var socket := WebSocketPeer.new()
var my_id = ""
var players = {}
var resources = {}

var bg_tex = preload("res://assets/bg.png")
var player_tex = preload("res://assets/player.png")
var tree_tex = preload("res://assets/objects.png")

var ui_label: Label

func _ready():
	print("Conectando ao servidor...")
	var err = socket.connect_to_url("ws://localhost:8765")
	if err != OK:
		print("Erro ao conectar!")
		
	# Setup UI
	ui_label = Label.new()
	ui_label.position = Vector2(10, 10)
	ui_label.add_theme_color_override("font_color", Color(1,1,1))
	ui_label.text = "Conectando..."
	add_child(ui_label)

func _process(delta):
	socket.poll()
	var state = socket.get_ready_state()
	if state == WebSocketPeer.STATE_OPEN:
		while socket.get_available_packet_count():
			var packet = socket.get_packet()
			var text = packet.get_string_from_utf8()
			handle_message(text)
	elif state == WebSocketPeer.STATE_CLOSED:
		ui_label.text = "Desconectado"

	handle_input()
	queue_redraw()

func handle_message(text: String):
	var data = JSON.parse_string(text)
	if data == null: return
	
	if data.has("type"):
		if data["type"] == "init":
			my_id = data["id"]
			players = data["state"]["players"]
			resources = data["state"]["resources"]
			ui_label.text = "Conectado! Inventário: 0 Madeira"
		elif data["type"] == "game_state":
			players = data["state"]["players"]
			resources = data["state"]["resources"]
			
			# Atualiza UI com o inventário do player local
			if players.has(my_id):
				var p = players[my_id]
				var wood = 0
				if p.has("inventory") and p["inventory"].has("wood"):
					wood = p["inventory"]["wood"]
				ui_label.text = "Conectado! Madeira: " + str(wood)
		elif data["type"] == "dice_roll":
			print("Alguém rolou dado: ", data["roll"], " Coletou: ", data["amount"])

func handle_input():
	if my_id == "" or not players.has(my_id): return
	
	var p = players[my_id]
	var px = p["x"]
	var py = p["y"]
	var speed = 5
	var moved = false
	
	if Input.is_action_pressed("ui_right") or Input.is_physical_key_pressed(KEY_D): 
		px += speed
		moved = true
	if Input.is_action_pressed("ui_left") or Input.is_physical_key_pressed(KEY_A): 
		px -= speed
		moved = true
	if Input.is_action_pressed("ui_down") or Input.is_physical_key_pressed(KEY_S): 
		py += speed
		moved = true
	if Input.is_action_pressed("ui_up") or Input.is_physical_key_pressed(KEY_W): 
		py -= speed
		moved = true
	
	if moved:
		send_msg({"type": "move", "x": px, "y": py})
		# Atualiza localmente para suavidade
		p["x"] = px
		p["y"] = py
		
	if Input.is_action_just_pressed("ui_accept"): # Espaço ou Enter
		# Tentar coletar o recurso mais próximo
		var closest_id = ""
		var min_dist = 9999
		for r_id in resources:
			var r = resources[r_id]
			if r.active:
				var dist = Vector2(px, py).distance_to(Vector2(r.x, r.y))
				if dist < 60 and dist < min_dist:
					min_dist = dist
					closest_id = r_id
		
		if closest_id != "":
			send_msg({"type": "collect", "resource_id": closest_id})

func send_msg(dict: Dictionary):
	if socket.get_ready_state() == WebSocketPeer.STATE_OPEN:
		socket.send_text(JSON.stringify(dict))

func _draw():
	# 1. Desenhar Fundo
	var tile_size = 48
	# Pixel Crawler grass é no 16, 16 da imagem Floors_Tiles
	var src_rect = Rect2(16, 16, 16, 16)
	for x in range(0, 800, tile_size):
		for y in range(0, 600, tile_size):
			draw_texture_rect_region(bg_tex, Rect2(x, y, tile_size, tile_size), src_rect)
			
	# 2. Desenhar Recursos (Árvores)
	if tree_tex != null:
		var tree_w = tree_tex.get_width() / 4.0 # 52
		var tree_h = tree_tex.get_height() / 3.0 # 64
		var draw_w = tree_w * 1.5
		var draw_h = tree_h * 1.5
		
		for res_id in resources:
			var res = resources[res_id]
			# No servidor o type é 'wood' (madeira)
			if res.has("type") and res["type"] == "wood":
				var rx = res["x"]
				var ry = res["y"]
				var rect = Rect2(rx - draw_w/2, ry - draw_h + 40, draw_w, draw_h)
				
				if res["active"]:
					# Árvore viva (coluna 0, linha 0)
					draw_texture_rect_region(tree_tex, rect, Rect2(0, 0, tree_w, tree_h))
				else:
					# Árvore morta (coluna 3, linha 0)
					draw_texture_rect_region(tree_tex, rect, Rect2(tree_w * 3, 0, tree_w, tree_h))
					
	# 3. Desenhar Jogadores
	if player_tex != null:
		var frame_w = player_tex.get_height() # Sprite sheet horizontal, altura = largura de 1 frame
		var frame_h = player_tex.get_height()
		var draw_w = frame_w * 2.0
		var draw_h = frame_h * 2.0
		
		for p_id in players:
			var p = players[p_id]
			var px = p["x"]
			var py = p["y"]
			var rect = Rect2(px - draw_w/2, py - draw_h + 10, draw_w, draw_h)
			
			draw_texture_rect_region(player_tex, rect, Rect2(0, 0, frame_w, frame_h))
			
			# Triângulo indicador se for o próprio jogador
			if p_id == my_id:
				var points = PackedVector2Array([
					Vector2(px, py - draw_h - 5),
					Vector2(px - 5, py - draw_h - 15),
					Vector2(px + 5, py - draw_h - 15)
				])
				draw_polygon(points, PackedColorArray([Color(1, 0.8, 0)]))
