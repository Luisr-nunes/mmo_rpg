const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const connStatus = document.getElementById('connStatus');
const scoreDisplay = document.getElementById('scoreDisplay');

let ws;
let myId = null;
let gameState = { players: {}, resources: {} };

// Input do jogador
const keys = { w: false, a: false, s: false, d: false };
const mouse = { x: 0, y: 0, clicked: false };

// Imagens (Placeholders simples desenhados via Canvas para o MVP)
function drawPlayer(x, y, isMe) {
    ctx.fillStyle = isMe ? '#3498db' : '#e74c3c'; // Azul para mim, Vermelho para outros
    ctx.beginPath();
    ctx.arc(x, y, 15, 0, Math.PI * 2);
    ctx.fill();
    ctx.lineWidth = 2;
    ctx.strokeStyle = '#2c3e50';
    ctx.stroke();
    
    // "Olhos" indicando a direção (simplificado)
    ctx.fillStyle = 'white';
    ctx.beginPath();
    ctx.arc(x - 5, y - 5, 4, 0, Math.PI * 2);
    ctx.arc(x + 5, y - 5, 4, 0, Math.PI * 2);
    ctx.fill();
}

function drawResource(x, y, active) {
    if (!active) {
        // Tronco cortado (Recurso aguardando respawn)
        ctx.fillStyle = '#7f8c8d';
        ctx.fillRect(x - 10, y - 10, 20, 20);
        return;
    }
    
    // Árvore (Recurso ativo)
    ctx.fillStyle = '#8e44ad'; // Tronco
    ctx.fillRect(x - 5, y - 10, 10, 20);
    ctx.fillStyle = '#27ae60'; // Folhas
    ctx.beginPath();
    ctx.arc(x, y - 20, 15, 0, Math.PI * 2);
    ctx.fill();
}

function connect() {
    ws = new WebSocket('ws://localhost:8765');

    ws.onopen = () => {
        connStatus.textContent = 'Conectado';
        connStatus.style.color = '#2ecc71';
    };

    ws.onmessage = (event) => {
        const data = JSON.parse(event.data);
        
        if (data.type === 'init') {
            myId = data.id;
            gameState = data.state;
        } else if (data.type === 'game_state') {
            gameState = data.state;
            if (gameState.players[myId]) {
                scoreDisplay.textContent = gameState.players[myId].score;
            }
        }
    };

    ws.onclose = () => {
        connStatus.textContent = 'Desconectado (Tentando reconectar...)';
        connStatus.style.color = '#e74c3c';
        setTimeout(connect, 2000);
    };
}

// Loop do Cliente
function update() {
    if (!myId || !gameState.players[myId]) return;

    let myPlayer = gameState.players[myId];
    let moved = false;
    let speed = 4; // Velocidade de movimento local

    if (keys.w) { myPlayer.y -= speed; moved = true; }
    if (keys.s) { myPlayer.y += speed; moved = true; }
    if (keys.a) { myPlayer.x -= speed; moved = true; }
    if (keys.d) { myPlayer.x += speed; moved = true; }

    // Limites da tela local
    myPlayer.x = Math.max(15, Math.min(canvas.width - 15, myPlayer.x));
    myPlayer.y = Math.max(15, Math.min(canvas.height - 15, myPlayer.y));

    // Enviar nova posição para o servidor
    if (moved && ws.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify({
            type: 'move',
            x: myPlayer.x,
            y: myPlayer.y
        }));
    }
}

function draw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Desenhar Recursos
    for (const [id, res] of Object.entries(gameState.resources)) {
        drawResource(res.x, res.y, res.active);
    }

    // Desenhar Jogadores
    for (const [id, player] of Object.entries(gameState.players)) {
        drawPlayer(player.x, player.y, id === myId);
        
        // Nome flutuante
        ctx.fillStyle = 'white';
        ctx.font = '12px Courier New';
        ctx.textAlign = 'center';
        ctx.fillText(player.name, player.x, player.y - 25);
    }
}

function gameLoop() {
    update();
    draw();
    requestAnimationFrame(gameLoop);
}

// Event Listeners
window.addEventListener('keydown', (e) => {
    if (keys.hasOwnProperty(e.key.toLowerCase())) {
        keys[e.key.toLowerCase()] = true;
    }
});

window.addEventListener('keyup', (e) => {
    if (keys.hasOwnProperty(e.key.toLowerCase())) {
        keys[e.key.toLowerCase()] = false;
    }
});

// Interação de Coleta
canvas.addEventListener('click', (e) => {
    if (!myId || !gameState.players[myId]) return;
    
    const rect = canvas.getBoundingClientRect();
    const clickX = e.clientX - rect.left;
    const clickY = e.clientY - rect.top;
    
    const myPlayer = gameState.players[myId];

    // Checar se clicou em um recurso
    for (const [id, res] of Object.entries(gameState.resources)) {
        if (!res.active) continue;
        
        // Distância do clique para o recurso
        const distToClick = Math.hypot(clickX - res.x, clickY - res.y);
        
        if (distToClick < 30) {
            // Tentar coletar enviando ao servidor
            ws.send(JSON.stringify({
                type: 'collect',
                resource_id: id
            }));
            break;
        }
    }
});

// Iniciar
connect();
gameLoop();
