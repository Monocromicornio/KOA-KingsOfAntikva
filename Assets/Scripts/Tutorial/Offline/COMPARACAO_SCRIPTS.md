# 🔄 Comparação: Scripts Online vs Offline

## Por Que Scripts Offline?

Os scripts originais das peças dependem de:
- ❌ **NetworkManager** para sincronização
- ❌ **MatchController** para controle do jogo
- ❌ **SoundController** para feedback sonoro

No **tutorial**, não temos estes sistemas ativos, então precisamos de versões **offline** simplificadas.

---

## 📊 Comparação Lado a Lado

### Piece vs OfflinePiece

| Recurso | Piece (Online) | OfflinePiece (Offline) |
|---------|----------------|------------------------|
| **Herança** | `NetworkBehaviour` | `MonoBehaviour` |
| **Network Calls** | ✅ Sim (`NetworkExecute`) | ❌ Não |
| **MatchController** | ✅ Depende | ❌ Independente |
| **BoardController** | Via MatchController | Busca local |
| **Field Tracking** | NetworkVariable<int> | int simples |
| **Selection** | Com turn validation | Sem validação de turno |
| **Movement** | ✅ Funciona | ✅ Funciona |
| **Tutorial Events** | ✅ Dispara | ✅ Dispara |

**Código:**
```csharp
// Online
public class Piece : NetworkBehaviour
{
    private MatchController matchController => MatchController.instance;
    private BoardController board => matchController.boardController;
    private NetworkVariable<int> fieldIndex = -1;
    
    public void SetLose()
    {
        if (hasConnection) NetworkExecute(OnLose);
        else OnLose();
    }
}

// Offline
public class OfflinePiece : MonoBehaviour
{
    private BoardController board;
    private int fieldIndex = -1;
    
    public void SetLose()
    {
        OnLose();
    }
}
```

---

### AnimPiece vs OfflineAnimPiece

| Recurso | AnimPiece (Online) | OfflineAnimPiece (Offline) |
|---------|-------------------|---------------------------|
| **Herança** | `NetworkBehaviour` | `MonoBehaviour` |
| **Network Calls** | ✅ Sim (`NetworkExecute`) | ❌ Não |
| **MatchController** | ✅ Depende | ❌ Independente |
| **SoundController** | ✅ Usa | ❌ Não usa |
| **Animator** | ✅ Funciona | ✅ Funciona |
| **Animações** | Trigger/Bool | Trigger/Bool |
| **Die Effect** | Com sons do game | Com AudioSource local |

**Código:**
```csharp
// Online
public class AnimPiece : NetworkBehaviour
{
    public void SetAnimation(string animName)
    {
        if (hasConnection) NetworkExecute<string>(SetTrigger, animName);
        else SetTrigger(animName);
    }
}

// Offline
public class OfflineAnimPiece : MonoBehaviour
{
    public void SetAnimation(string animName)
    {
        SetTrigger(animName);
    }
}
```

---

### SelectablePiece vs OfflineSelectablePiece

| Recurso | SelectablePiece (Online) | OfflineSelectablePiece (Offline) |
|---------|-------------------------|--------------------------------|
| **BoardController** | Via MatchController | Busca local ou via TutorialBoardController |
| **SoundController** | ✅ Usa (Select/Cancel) | ❌ Não usa |
| **Lógica de Seleção** | ✅ Mesma | ✅ Mesma |
| **GetEmptyField** | ✅ Funciona | ✅ Funciona |
| **Distance** | ✅ Configurável | ✅ Configurável |

**Código:**
```csharp
// Online
private BoardController board => matchController.boardController;

// Offline
private BoardController board;

private void Start()
{
    TutorialBoardController tutorialBoard = FindFirstObjectByType<TutorialBoardController>();
    if (tutorialBoard != null)
    {
        board = tutorialBoard.GetBoardController();
    }
}
```

---

### MovePiece vs OfflineMovePiece

| Recurso | MovePiece (Online) | OfflineMovePiece (Offline) |
|---------|-------------------|---------------------------|
| **BoardController** | Via MatchController | Busca local |
| **AnimPiece** | `AnimPiece` | `OfflineAnimPiece` |
| **Movement** | iTween animado | iTween animado |
| **Lift/Fly/Land** | ✅ Funciona | ✅ Funciona |
| **Walk Animation** | ✅ Funciona | ✅ Funciona |

**Código:**
```csharp
// Online
[SerializeField] AnimPiece anim;
private BoardController board => matchController.boardController;

// Offline
[SerializeField] private OfflineAnimPiece anim;
private BoardController board;

private void Start()
{
    // Busca board localmente
}
```

---

### InteractivePiece vs OfflineInteractivePiece

| Recurso | InteractivePiece (Online) | OfflineInteractivePiece (Offline) |
|---------|-------------------------|----------------------------------|
| **MatchController** | ✅ Depende | ❌ Independente |
| **SoundController** | ✅ Usa | ❌ Não usa |
| **AnimPiece** | `AnimPiece` | `OfflineAnimPiece` |
| **Force System** | ✅ Funciona | ✅ Funciona |
| **Attack/Counter** | ✅ Funciona | ✅ Funciona |
| **Tutorial Events** | ✅ Dispara | ✅ Dispara |

**Código:**
```csharp
// Online
protected MatchController matchController => MatchController.instance;
protected SoundController soundController => matchController.soundController;
[SerializeField] protected AnimPiece anim;

// Offline
[SerializeField] protected OfflineAnimPiece anim;
// Sem MatchController ou SoundController
```

---

### AttackPiece vs OfflineAttackPiece

| Recurso | AttackPiece (Online) | OfflineAttackPiece (Offline) |
|---------|---------------------|----------------------------|
| **SelectablePiece** | `SelectablePiece` | `OfflineSelectablePiece` |
| **InteractivePiece** | Herda de `InteractivePiece` | Herda de `OfflineInteractivePiece` |
| **Attack Logic** | ✅ Mesma | ✅ Mesma |
| **Position to Attack** | ✅ Funciona | ✅ Funciona |
| **Effects** | ✅ Funciona | ✅ Funciona |

---

## 🎯 Matriz de Dependências

### Scripts Online
```
Piece
├── NetworkBehaviour
├── MatchController
│   ├── hasConnection
│   ├── BoardController
│   └── NetworkManager
├── NetworkVariable<int>
└── NetworkExecute calls

AnimPiece
├── NetworkBehaviour
├── MatchController
│   ├── hasConnection
│   ├── SoundController
│   └── GameMode
└── Animator

SelectablePiece
├── MonoBehaviour
├── MatchController
│   ├── BoardController
│   └── SoundController
└── Piece

MovePiece
├── MonoBehaviour
├── MatchController
│   ├── BoardController
│   └── finished
├── AnimPiece
└── SelectablePiece

InteractivePiece
├── MonoBehaviour
├── MatchController
│   ├── SoundController
│   └── finished
├── AnimPiece
└── Piece

AttackPiece
├── InteractivePiece (base)
├── SelectablePiece
└── Effects
```

### Scripts Offline
```
OfflinePiece
├── MonoBehaviour
└── BoardController (busca local)

OfflineAnimPiece
├── MonoBehaviour
└── Animator

OfflineSelectablePiece
├── MonoBehaviour
├── BoardController (busca local)
└── Piece

OfflineMovePiece
├── MonoBehaviour
├── BoardController (busca local)
├── OfflineAnimPiece
└── OfflineSelectablePiece

OfflineInteractivePiece
├── MonoBehaviour
├── OfflineAnimPiece
└── Piece

OfflineAttackPiece
├── OfflineInteractivePiece (base)
├── OfflineSelectablePiece
└── Effects
```

---

## 📦 O Que Foi Removido?

### ❌ Dependências Removidas
1. **NetworkBehaviour** - Não precisa sincronizar rede
2. **MatchController** - Não existe no tutorial
3. **SoundController** - Sons podem ser adicionados localmente se necessário
4. **Network Calls** - `NetworkExecute`, etc
5. **GameMode checks** - Não precisa verificar modo de jogo
6. **Connection checks** - `hasConnection`, etc

### ✅ O Que Foi Mantido
1. **Toda a lógica de movimento** (iTween, lift, fly, land)
2. **Toda a lógica de seleção** (campos válidos, distância)
3. **Toda a lógica de ataque** (força, challenge, counterattack)
4. **Todas as animações** (Walk, Attack, Die, Win)
5. **Eventos do Tutorial** (`TutorialEvents`)
6. **Sistema de campos** (GetEmptyField, etc)

---

## 🔧 Como BoardController é Encontrado?

### Scripts Online
```csharp
// Acesso direto via MatchController
private BoardController board => matchController.boardController;
```

### Scripts Offline
```csharp
// Busca local em Start()
private BoardController board;

private void Start()
{
    // Prioridade 1: Via TutorialBoardController
    TutorialBoardController tutorialBoard = FindFirstObjectByType<TutorialBoardController>();
    if (tutorialBoard != null)
    {
        board = tutorialBoard.GetBoardController();
    }
    
    // Fallback: Busca direta
    if (board == null)
    {
        board = FindFirstObjectByType<BoardController>();
    }
}
```

---

## 💡 Quando Usar Cada Um?

### Use Scripts Online
- ✅ Jogo multiplayer normal
- ✅ Modo competitivo
- ✅ Quando tem NetworkManager
- ✅ Quando precisa de sincronização

### Use Scripts Offline
- ✅ Tutorial
- ✅ Modo offline/singleplayer
- ✅ Testes sem rede
- ✅ Quando não tem NetworkManager

---

## 🎮 Exemplo Prático

### Prefab Online (SO Blue)
```
SO Blue.prefab
├── Piece
├── AnimPiece              ← Online
├── SelectablePiece        ← Online
├── MovePiece              ← Online
├── AttackPiece            ← Online
└── NetworkInstantiate...  ← Online
```

**Usa em:** Jogo multiplayer normal

### Prefab Offline (SO Blue Tutorial)
```
SO Blue Tutorial.prefab
├── Piece
├── OfflineAnimPiece       ← Offline
├── OfflineSelectablePiece ← Offline
├── OfflineMovePiece       ← Offline
└── OfflineAttackPiece     ← Offline
```

**Usa em:** Tutorial (TutorialScene)

---

## 📝 Resumo

| Aspecto | Online | Offline |
|---------|--------|---------|
| **Complexidade** | Alta | Baixa |
| **Dependências** | Muitas | Poucas |
| **NetworkManager** | ✅ Necessário | ❌ Não precisa |
| **MatchController** | ✅ Necessário | ❌ Não precisa |
| **Funcionalidade** | 100% | ~95% (sem sons do sistema) |
| **Uso** | Jogo completo | Tutorial apenas |
| **Performance** | Normal | Ligeiramente melhor |

---

## 🎯 Vantagens dos Scripts Offline

1. ✅ **Independentes** - Funcionam sozinhos
2. ✅ **Simples** - Menos código, menos bugs
3. ✅ **Rápidos** - Sem overhead de rede
4. ✅ **Focados** - Feitos especificamente para tutorial
5. ✅ **Testáveis** - Fácil de testar isoladamente

---

**Agora você pode criar tutoriais offline completos! 🎮✨**
