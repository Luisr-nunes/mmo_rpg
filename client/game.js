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

// Carregar Imagens
const playerImg = new Image();
playerImg.src = 'assets/player.png';

const objectsImg = new Image();
objectsImg.src = 'assets/objects.png';

// Constantes da Arte
const SPRITE_SIZE = 16;
const SCALE = 3; // Desenhar 3x maior (48x48)

// Imagens
function drawPlayer(x, y, isMe) {
    if (!playerImg.complete) return;
    
    // Para simplificar no MVP, vamos pegar apenas o primeiro frame (parado de frente)
    // Coordenadas aproximadas (frame 0, linha 0)
    const sx = 0;
    const sy = 0;
    
    // Ajustar o ponto de desenho para que (x,y) seja o centro/pé do personagem
    const drawX = x - (SPRITE_SIZE * SCALE) / 2;
    const drawY = y - (SPRITE_SIZE * SCALE) + 5; 
    
    ctx.drawImage(playerImg, sx, sy, SPRITE_SIZE, SPRITE_SIZE, drawX, drawY, SPRITE_SIZE * SCALE, SPRITE_SIZE * SCALE);
    
    if (isMe) {
        // Indicador simples de quem sou eu (triângulo pequeno em cima)
        ctx.fillStyle = '#f1c40f';
        ctx.beginPath();
        ctx.moveTo(x, drawY - 10);
        ctx.lineTo(x - 5, drawY - 15);
        ctx.lineTo(x + 5, drawY - 15);
        ctx.fill();
    }
}

function drawResource(x, y, active) {
    if (!objectsImg.complete) return;
    
    // Coordenadas aproximadas da árvore e do toco (vamos assumir que a árvore inteira tem 16x16 ou 16x32)
    // Baseado em pacotes de pixel art comuns (ajustaremos se ficar torto)
    let sx = active ? 0 : 16; 
    let sy = 0; 
    
    const drawX = x - (SPRITE_SIZE * SCALE) / 2;
    const drawY = y - (SPRITE_SIZE * SCALE);
    
    ctx.drawImage(objectsImg, sx, sy, SPRITE_SIZE, SPRITE_SIZE, drawX, drawY, SPRITE_SIZE * SCALE, SPRITE_SIZE * SCALE);
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
