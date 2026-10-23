# MMO RPG

<!-- badges -->
![C#](https://img.shields.io/badge/C%23-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=white) ![Python](https://img.shields.io/badge/Python-3776AB?style=flat-square&logo=python&logoColor=white) ![WebSocket](https://img.shields.io/badge/WebSocket-010101?style=flat-square&logo=socketdotio&logoColor=white)

[Read in English](README.en.md)

Protótipo de um RPG multijogador em pixel art que mistura progressão de MMO, rolagens de d20 inspiradas em D&D e uma economia guiada pelos jogadores.

## Sobre

O projeto está em fase de protótipo. A base atual valida o ciclo de rede e a primeira mecânica de jogo:

- um **servidor autoritativo** em Python (WebSocket) mantém o estado do mundo: jogadores, posições, recursos e inventários;
- um **cliente Unity 2D** conecta ao servidor, envia movimento e ações de coleta e renderiza o estado recebido;
- cada coleta dispara uma **rolagem de d20** no servidor, transmitida a todos os jogadores.

A visão completa do jogo (criação de personagem, maestria por uso, masmorras instanciadas, forja, sistema de morte) está no documento de design [MD_JOGO.md](MD_JOGO.md).

## Funcionalidades implementadas

- Conexão de vários jogadores ao mesmo mundo, com estado sincronizado a cada tick.
- Movimento com WASD ou setas.
- Coleta de recursos (madeira) com Espaço ou Enter quando o jogador está próximo.
- Resultado da coleta definido por d20: 1 é falha crítica (nada), 2 a 14 rende 1, 15 a 19 rende 2 e 20 é crítico (3).
- Recursos coletados reaparecem após 10 segundos.
- Inventário por jogador exibido na interface.

## Tecnologias

| Camada | Tecnologia |
| --- | --- |
| Servidor | Python 3, `asyncio`, `websockets` |
| Cliente | Unity 2D com URP (C#), `System.Net.WebSockets`, Input System, TextMesh Pro, 2D Animation, Tilemap, importador Aseprite |
| Arte | Pacotes de pixel art de terceiros em `visual/` |
| Utilitários | Scripts Python em `client_unity/` para recorte de sprites e ajuste de metadados |

## Estrutura

```
mmo_rpg/
├── MD_JOGO.md          # Game Design Document
├── server/
│   └── server.py       # servidor WebSocket (ws://localhost:8765)
├── client_unity/       # projeto Unity
│   ├── Assets/Scripts/ # GameManager, NetworkManager, PlayerController, InventoryUI
│   └── Packages/
└── visual/             # pacotes de sprites usados no cliente
```

## Como executar

Pré-requisitos: Python 3.10+ e o Unity Hub com uma versão do Unity 6.

1. Inicie o servidor:

   ```bash
   pip install websockets
   python server/server.py
   ```

   O servidor escuta em `ws://localhost:8765`.

2. Abra a pasta `client_unity` no Unity Hub, carregue `Assets/Scenes/SampleScene` e pressione Play. Para testar o multijogador, gere um build e abra mais de uma instância.

> A pasta `ProjectSettings` do Unity não está versionada; o editor a recria com valores padrão ao abrir o projeto.

## Projeto relacionado

[mmo_rpg_godot](https://github.com/Luisr-nunes/mmo_rpg_godot) é um cliente alternativo em Godot que conversa com o mesmo servidor.

## Créditos de arte

Os sprites em `visual/` pertencem aos seus autores e seguem as licenças de cada pacote (veja os arquivos de termos dentro de cada pasta).

## Autor

Luís Nunes — [GitHub](https://github.com/Luisr-nunes) · [LinkedIn](https://www.linkedin.com/in/luis-nunes-dev/)
