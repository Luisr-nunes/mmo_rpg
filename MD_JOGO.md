# Game Design Document (GDD) - RPG Híbrido

## 1. Visão Geral do Projeto
Um RPG multijogador que funde mecânicas clássicas de MMOs, a liberdade dos RPGs de mesa e uma estética nostálgica. O jogo oferece progressão profunda focada tanto em combate (PVE/Masmorras) quanto na economia gerenciada pelos jogadores (Coleta/Crafting).

### Principais Referências
*   **Dungeons & Dragons (D&D):** Rolagem de d20 para status iniciais, combates guiados pela sorte visível dos dados e sistema de salvamento contra a morte.
*   **Albion Online:** Teia de evolução de maestria baseada no uso de equipamentos e coleta, sem restrição de classes.
*   **Ragnarok Online:** Distribuição manual de pontos de atributos (Força, Inteligência, etc.) ao subir de nível geral.
*   **Grand Chase:** Sistema de *Transmog*, permitindo total personalização visual sobrepondo itens cosméticos aos equipamentos de status.
*   **Bit Heroes:** Estética visual em Pixel Art.

---

## 2. Criação de Personagem e Progressão

### 2.1. O Início
*   **Status Base:** Definidos através da rolagem clássica de um dado d20, gerando atributos únicos para cada jogador.
*   **Escolha de Origem:** O jogador escolhe sua Espécie e Classe Inicial, mas isso serve apenas como um direcionamento inicial (equipamento básico), não limitando o futuro do personagem.

### 2.2. A Dupla Camada de Evolução
*   **Nível de Personagem:** Ao ganhar XP geral, o jogador sobe de nível e recebe pontos brutos para alocar livremente em seus atributos (Força, Destreza, Inteligência, Sorte, etc.).
*   **Nível de Maestria:** A evolução prática. O que o jogador usa, evolui. Usar espadas melhora a proficiência com espadas; coletar minério melhora a habilidade de mineração.

---

## 3. O Mundo de Jogo

### 3.1. Exploração e Economia de Mundo Aberto
*   O mapa é dividido em biomas com recursos regionais exclusivos (ex: uma cidade no deserto possui minérios vitais que não existem nas florestas).
*   Incentiva o comércio, rotas de transporte e a exploração de áreas perigosas em busca de matérias-primas raras.

### 3.2. Masmorras Instanciadas (PVE)
*   Cavernas e castelos que criam sessões isoladas para o grupo que entrou.
*   Foco em desafios táticos, quebra-cabeças e chefões.
*   Principal fonte de *Loot* avançado: projetos de forja raros, grimórios e itens mágicos que não são encontrados no mundo aberto.

---

## 4. Sistema de Combate e Acessibilidade

### 4.1. Combate em Tempo Real (Action-RNG)
*   **Movimentação:** Livre, com sistema de *Targeting* (travar a mira no inimigo) para uso de habilidades.
*   **Rolagem Visível:** Toda ação de combate aciona uma rolagem de d20. O dado aparece na interface de forma não intrusiva (ex: uma bandeja de dados no canto da tela ou dados flutuantes), decidindo acertos, erros críticos ou efeitos colaterais.

### 4.2. Acessibilidade Visual (A11y)
*   Efeitos intensos atrelados a acertos críticos (como tremores de tela - *Screen Shake* - e clarões) são opcionais.
*   O menu de configurações permite ao jogador desativar esses estímulos visuais, substituindo o feedback por efeitos sonoros e indicações claras na UI.

---

## 5. O Sistema de Forja e Conhecimento (Crafting)

### 5.1. Evolução do Artesão
*   **Atributo de Forja:** Pontos gerais que definem o nível tecnológico que o jogador pode acessar (ex: 5 pontos libera Pedra, 10 pontos libera Bronze).
*   **Especialização (Prática):** Criar o mesmo item repetidas vezes concede pontos de maestria específicos para aquele item, alterando as probabilidades de raridade (Comum, Incomum, Raro, Épico, Lendário) a favor do jogador.

### 5.2. Grimórios e Moldes Físicos (Economia de Receitas)
*   **Sobrevivência Básica:** Receitas primárias (ferramentas e armas iniciais) são aprendidas automaticamente ao atingir o nível necessário.
*   **Moldes Físicos:** Receitas avançadas e feitiços exigem que o jogador carregue um Grimório ou Pergaminho físico no inventário.
*   **Memória Muscular:** Ao criar o item ou conjurar a magia um determinado número de vezes (ex: 20 vezes), o jogador adquire "Memória Muscular", não precisando mais do item físico e podendo revendê-lo no mercado.
*   **A Regra dos Lendários:** Itens e feitiços *Lendários* nunca podem ser memorizados. Eles sempre exigirão o grimório físico, garantindo escassez e monopolização estratégica do mercado.

---

## 6. Sistema de Morte e Punição

### 6.1. O Estado Caído e Testes de Morte (Regras do D&D)
*   Quando o HP chega a zero, o jogador cai inconsciente no campo de batalha, mas não morre imediatamente.
*   A cada intervalo de tempo, o sistema rola automaticamente um d20 (Teste contra a Morte).
    *   **≥ 10:** Sucesso.
    *   **≤ 9:** Falha.
*   Acumular 3 Sucessos estabiliza o jogador. Acumular 3 Falhas resulta em **Morte Definitiva**.
*   Aliados têm uma janela de tempo para curar o jogador caído e trazê-lo de volta ao combate.

### 6.2. Consequências da Morte
*   Se a Morte Definitiva ocorrer, o espírito do personagem retorna à cidade.
*   **Loot Físico:** O corpo e *todo* o inventário (incluindo moldes físicos e equipamentos valiosos) ficam caídos na masmorra.
*   Aliados sobreviventes podem saquear o corpo do amigo para resgatar os itens. Caso o grupo inteiro seja derrotado (Wipe/TPK), os itens ficam na masmorra, gerando uma potencial missão de resgate para o próprio grupo recuperar seu equipamento.