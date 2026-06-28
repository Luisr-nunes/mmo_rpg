const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const connStatus = document.getElementById('connStatus');

// Elementos de UI
const inventoryUI = document.getElementById('inventoryUI');
const invWoodCount = document.getElementById('invWoodCount');
let inventoryOpen = false;

let ws;
let myId = null;
let gameState = { players: {}, resources: {} };

// Input do jogador
const keys = { w: false, a: false, s: false, d: false, i: false };

// Textos flutuantes (D&D)
let floatingTexts = []; // { text, x, y, color, life, maxLife }

// Carregar Imagens
const playerImg = new Image();
playerImg.src = 'assets/player.png'; // Pixel Crawler (Idle Down)

const objectsImg = new Image();
objectsImg.src = 'assets/objects.png'; // Pixel Crawler (Tree)

const bgImg = new Image();
bgImg.src = 'assets/bg.png'; // Pixel Crawler (Tileset)

// Constantes da Arte (Ajustadas para Pixel Crawler)
const SPRITE_WIDTH = 16;
const SPRITE_HEIGHT = 16;
const SCALE = 3; 

function addFloatingText(text, x, y, color) {
    floatingTexts.push({
        text: text,
        x: x,
        y: y,
        color: color,
        life: 60, // frames (~1 segundo a 60fps)
        maxLife: 60
    });
}

function drawBackground() {
    if (!bgImg.complete) {
        ctx.fillStyle = "#78a252";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        return;
    }
    
    // Pixel Crawler usa tiles de 32x32 na maioria dos casos
    const TILE_SRC_SIZE = 32;
    const tileSize = TILE_SRC_SIZE * 2; // Desenhar 64x64 na tela para ficar visível
    
    // Tentar pegar o tile de grama pura (geralmente em 0,0 ou 32,32)
    // Se 0,0 for transparente, 32,32 ou 0,32 costuma ser seguro. Vamos usar 32, 32
    for (let x = 0; x < canvas.width; x += tileSize) {
        for (let y = 0; y < canvas.height; y += tileSize) {
            ctx.drawImage(bgImg, 32, 32, TILE_SRC_SIZE, TILE_SRC_SIZE, x, y, tileSize, tileSize);
        }
    }
}

function drawPlayer(x, y, isMe) {
    if (!playerImg.complete || playerImg.width === 0) return;
    
    const frameWidth = playerImg.height; 
    const frameHeight = playerImg.height; 
    
    const sx = 0; 
    const sy = 0;
    
    const drawWidth = frameWidth * SCALE;
    const drawHeight = frameHeight * SCALE;
    const drawX = x - drawWidth / 2;
    const drawY = y - drawHeight + 10; 
    
    ctx.drawImage(playerImg, sx, sy, frameWidth, frameHeight, drawX, drawY, drawWidth, drawHeight);
    
    if (isMe) {
        ctx.fillStyle = '#f1c40f';
        ctx.beginPath();
        ctx.moveTo(x, drawY - 10);
        ctx.lineTo(x - 5, drawY - 15);
        ctx.lineTo(x + 5, drawY - 15);
        ctx.fill();
    }
}

function drawResource(x, y, active) {
    if (!objectsImg.complete || objectsImg.width === 0) return;
    
    // Como vimos, Size_03.png é uma folha com várias árvores (Verão, Outono, Inverno, Morta)
    // Se existem 4 colunas (vimos 4 árvores juntas na imagem), a largura de uma árvore é a largura total / 4.
    const treeW = objectsImg.width / 4;
    const treeH = objectsImg.height;
    
    // Vamos desenhar a primeira árvore (Verde) que está na coluna 0.
    const sx = 0;
    const sy = 0;
    
    const drawWidth = treeW * 2; 
    const drawHeight = treeH * 2;
    
    const drawX = x - drawWidth / 2;
    const drawY = y - drawHeight + 30; // Ajustado para a base do tronco ficar na coordenada
    
    if (active) {
        ctx.globalAlpha = 1.0;
        ctx.drawImage(objectsImg, sx, sy, treeW, treeH, drawX, drawY, drawWidth, drawHeight);
    } else {
        // Árvore cortada - desenha a mesma árvore porém transparente para o MVP
        ctx.globalAlpha = 0.3;
        ctx.drawImage(objectsImg, sx, sy, treeW, treeH, drawX, drawY, drawWidth, drawHeight);
        ctx.globalAlpha = 1.0;
    }
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
            
            // Atualizar UI do inventário
            if (gameState.players[myId]) {
                const myInv = gameState.players[myId].inventory;
                if (myInv) {
                    invWoodCount.textContent = myInv.wood || 0;
                }
            }
        } else if (data.type === 'dice_roll') {
            // Um dado foi rolado!
            let color = 'white';
            let msg = `d20: ${data.roll}`;
            
            if (data.roll === 1) { color = '#e74c3c'; msg += ' (Falha Crítica)'; }
            else if (data.roll === 20) { color = '#f1c40f'; msg += ' (Acerto Crítico!)'; }
            else if (data.amount > 0) { color = '#2ecc71'; msg += ` (+${data.amount} item)`; }
            else { color = '#95a5a6'; }
            
            addFloatingText(msg, data.x, data.y - 40, color);
        }
    };

    ws.onclose = () => {
        connStatus.textContent = 'Desconectado';
        connStatus.style.color = '#e74c3c';
        setTimeout(connect, 2000);
    };
}

// Loop do Cliente
function update() {
    if (!myId || !gameState.players[myId]) return;

    let myPlayer = gameState.players[myId];
    let moved = false;
    let speed = 4;

    if (keys.w) { myPlayer.y -= speed; moved = true; }
    if (keys.s) { myPlayer.y += speed; moved = true; }
    if (keys.a) { myPlayer.x -= speed; moved = true; }
    if (keys.d) { myPlayer.x += speed; moved = true; }

    myPlayer.x = Math.max(20, Math.min(canvas.width - 20, myPlayer.x));
    myPlayer.y = Math.max(20, Math.min(canvas.height - 20, myPlayer.y));

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

    drawBackground();

    // Desenhar Recursos
    for (const [id, res] of Object.entries(gameState.resources)) {
        drawResource(res.x, res.y, res.active);
    }

    // Desenhar Jogadores
    for (const [id, player] of Object.entries(gameState.players)) {
        drawPlayer(player.x, player.y, id === myId);
        
        ctx.fillStyle = 'white';
        ctx.font = 'bold 12px Courier New';
        ctx.textAlign = 'center';
        // Sombra no texto
        ctx.strokeText(player.name, player.x, player.y - 30);
        ctx.fillText(player.name, player.x, player.y - 30);
    }
    
    // Desenhar Textos flutuantes (D&D)
    for (let i = floatingTexts.length - 1; i >= 0; i--) {
        let ft = floatingTexts[i];
        
        // Movimento para cima
        ft.y -= 1;
        
        // Fade out
        let alpha = ft.life / ft.maxLife;
        ctx.globalAlpha = alpha;
        
        ctx.fillStyle = ft.color;
        ctx.font = 'bold 16px Courier New';
        ctx.textAlign = 'center';
        ctx.fillText(ft.text, ft.x, ft.y);
        
        ctx.globalAlpha = 1.0; // reset
        
        ft.life--;
        if (ft.life <= 0) {
            floatingTexts.splice(i, 1);
        }
    }
}

function gameLoop() {
    update();
    draw();
    requestAnimationFrame(gameLoop);
}

// Event Listeners
window.addEventListener('keydown', (e) => {
    const k = e.key.toLowerCase();
    if (keys.hasOwnProperty(k)) {
        keys[k] = true;
    }
    
    // Toggle Inventário
    if (k === 'i') {
        inventoryOpen = !inventoryOpen;
        inventoryUI.style.display = inventoryOpen ? 'block' : 'none';
    }
});

window.addEventListener('keyup', (e) => {
    const k = e.key.toLowerCase();
    if (keys.hasOwnProperty(k)) {
        keys[k] = false;
    }
});

canvas.addEventListener('click', (e) => {
    if (!myId || !gameState.players[myId] || inventoryOpen) return;
    
    const rect = canvas.getBoundingClientRect();
    const clickX = e.clientX - rect.left;
    const clickY = e.clientY - rect.top;
    
    for (const [id, res] of Object.entries(gameState.resources)) {
        if (!res.active) continue;
        
        const dist = Math.hypot(clickX - res.x, clickY - res.y);
        
        if (dist < 40) {
            ws.send(JSON.stringify({
                type: 'collect',
                resource_id: id
            }));
            break;
        }
    }
});

connect();
gameLoop();
