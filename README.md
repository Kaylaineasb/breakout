# Breakout

Clone do clássico **Breakout** feito em **Unity 6**, com foco em código limpo, arquitetura orientada a eventos e polimento visual e sonoro.

<img width="654" height="366" alt="image" src="https://github.com/user-attachments/assets/01c04788-e7e8-49ba-825f-3e88340a4448" />

<img width="641" height="349" alt="image" src="https://github.com/user-attachments/assets/fd7b5cd7-af2b-489c-b787-7fa51251b6f9" />


---

## Controles

| Ação | Mouse | Teclado |
|---|---|---|
| Mover a paddle | mover o mouse | `A` / `D` ou `←` / `→` |
| Lançar a bola | clique esquerdo | `Espaço` |
| Pausar / continuar | — | `Esc` |

## Funcionalidades

- Paddle controlada por mouse ou teclado, limitada às bordas da arena
- Bola com velocidade constante e ângulo de rebatida definido pelo ponto de contato na paddle (centro = reto, bordas = mais inclinado), sem ângulos quase horizontais
- Grade de tijolos gerada por código, com cores por linha e tijolos que aguentam 2 ou 3 batidas
- Pontuação, 3 vidas e níveis com layouts diferentes e velocidade crescente
- Power-ups: **paddle maior**, **multiball** e **bola lenta**
- Menu, HUD, pausa, game over e vitória
- Histórico das últimas 3 partidas e high score salvos localmente
- Polimento: screen shake, partículas na cor do tijolo, trail na bola, tween no HUD

## Tecnologias

- **Unity 6** (`6000.3.13f1`)
- **URP 2D** (Universal Render Pipeline)
- **Input System** (pacote `com.unity.inputsystem`, sem o `UnityEngine.Input` legado)
- **TextMeshPro** / uGUI
- C# · build **WebGL**

## Como abrir o projeto

1. Instale o **Unity 6 `6000.3.13f1`** pelo Unity Hub (versões diferentes podem pedir upgrade do projeto).
2. Clone o repositório:
   ```bash
   git clone https://github.com/kaylaineasb/breakout.git
   ```
3. No Unity Hub: **Add > Add project from disk** e selecione a pasta clonada.
4. Abra a cena `Assets/_Project/Scenes/Game.unity` e aperte **Play**.

> A primeira abertura leva alguns minutos, porque o Unity recria a pasta `Library/` (ela não é versionada).

## Arquitetura

Os scripts se comunicam por **eventos C#** (`event Action`), sem `FindObjectOfType` nem singletons. O `GameManager` é a única fonte de verdade do estado do jogo; UI, áudio e efeitos apenas reagem aos eventos.

```
Ball ──── Lost / PaddleHit / WallHit ───┐
Brick ─── Hit / Destroyed ──────────────┼──► GameManager ── StateChanged / ScoreChanged / LivesChanged
PaddleController ── LaunchPressed ──► Ball                    LevelStarted / LevelCleared / LifeLost
                                                                     │
                                                 ┌───────────────────┼───────────────────┐
                                                 ▼                   ▼                   ▼
                                             UIManager          AudioManager        CameraShake
```

| Script | Responsabilidade |
|---|---|
| `GameManager` | Estados do jogo (enum `GameState`), pontuação, vidas, níveis e fluxo de partida |
| `PaddleController` | Lê o input (mouse e teclado) e move a paddle via `Rigidbody2D.MovePosition` |
| `Ball` | Lançamento, velocidade constante, ângulo de rebatida e detecção de perda |
| `Brick` | Vida do tijolo, feedback visual de dano e evento de destruição |
| `BrickSpawner` | Gera a grade a partir de um `LevelConfig` |
| `LevelConfig` | ScriptableObject com linhas, colunas, cores, resistência e velocidade do nível |
| `PowerUp` | Item que cai de tijolos e aplica o efeito ao tocar a paddle |
| `UIManager` | Troca de painéis por estado e atualização do HUD |
| `UIPunch` | Tween de escala e cor sem pacotes externos |
| `AudioManager` | Música com fade e efeitos em várias fontes com variação de pitch |
| `CameraShake` | Tremida leve da câmera ao quebrar tijolos |
| `ScoreHistory` | Persistência das últimas partidas via `PlayerPrefs` |

Detalhes que valem destaque:

- **Física estável:** a velocidade da bola é normalizada em todo `FixedUpdate`, com componente vertical mínima, para que ela nunca ganhe ou perca velocidade nem fique presa quicando na horizontal. A detecção de colisão é contínua.
- **Configuração por dados:** cada nível é um asset `LevelConfig`. Criar um nível novo não exige mudar código.
- **Pausa sem efeitos colaterais:** `Time.timeScale = 0` só na pausa; tweens e fades usam tempo não escalado.

## Estrutura de pastas

```
Assets/_Project/
├── Art/            sprites e materiais
├── Audio/          música e efeitos
├── Input/          BreakoutControls.inputactions (+ classe C# gerada)
├── Physics/        PhysicsMaterial2D da bola
├── Prefabs/
├── Scenes/         Game.unity (cena única; menus são painéis)
├── ScriptableObjects/  configs de nível
└── Scripts/
    ├── Core/       GameManager, AudioManager, CameraShake, ScoreHistory
    ├── Gameplay/   PaddleController, Ball, Brick, BrickSpawner, PowerUp
    ├── Data/       LevelConfig
    └── UI/         UIManager, UIPunch
```

## Build WebGL

1. **File > Build Profiles > Web > Switch Platform**
2. **Player Settings**: resolução `960 × 540`, template `Minimal`
3. **Build**, depois compacte o conteúdo da pasta gerada (o `index.html` precisa ficar na raiz do `.zip`) e envie ao itch.io como projeto **HTML**.

## Créditos

- Desenvolvimento: **Kaylaine Assunção** ([GitHub](https://github.com/kaylaineasb) · [LinkedIn](https://linkedin.com/in/kaylaineasb))
- Efeitos sonoros: gerados com [jsfxr](https://sfxr.me)
 <!-- Música: *NOME DA FAIXA* por *AUTOR* ([link](https://...)), licença *XXXX* <!-- preencha -->
 <!-- Fonte: *NOME DA FONTE* ([Google Fonts](https://fonts.google.com)), licença OFL <!-- se usar fonte própria -->

 <!--## Licença

Código sob licença [MIT](LICENSE). Assets de terceiros seguem as licenças indicadas em **Créditos**.-->
