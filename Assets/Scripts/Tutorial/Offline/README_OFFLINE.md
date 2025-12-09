# 🎮 Scripts Offline para Tutorial

## O Que São?

Versões **offline** dos scripts de peças que funcionam **sem NetworkManager** para uso exclusivo no tutorial.

---

## 📦 Scripts Criados

| Script Online | Script Offline | Mudanças Principais |
|--------------|----------------|---------------------|
| `Piece` | `OfflinePiece` | Remove NetworkBehaviour e network calls |
| `AnimPiece` | `OfflineAnimPiece` | Remove NetworkBehaviour e network calls |
| `SelectablePiece` | `OfflineSelectablePiece` | Busca BoardController localmente |
| `MovePiece` | `OfflineMovePiece` | Remove dependência do MatchController |
| `InteractivePiece` | `OfflineInteractivePiece` | Remove SoundController e MatchController |
| `AttackPiece` | `OfflineAttackPiece` | Usa versões offline dos componentes |

---

## 🔧 Como Usar

### Opção 1: Criar Prefabs Offline (Recomendado)

1. **Duplicate um prefab existente** (ex: SO Blue)
2. **Renomeie** para "SO Blue Tutorial" ou "SO Blue Offline"
3. **Remova** os scripts online:
   - AnimPiece
   - SelectablePiece
   - MovePiece
   - AttackPiece
   - InteractivePiece
   - Network Instantiate Detection

4. **Adicione** os scripts offline:
   - OfflinePiece
   - OfflineAnimPiece
   - OfflineSelectablePiece
   - OfflineMovePiece
   - OfflineAttackPiece (se for peça de ataque)

5. **Configure** as referências:
   - No `OfflineMovePiece`, arraste o `OfflineAnimPiece`
   - No `OfflineAttackPiece`, arraste o `OfflineAnimPiece`

6. **Salve o prefab** em `/Assets/Prefab/Pieces/Tutorial/`

### Opção 2: Converter Automaticamente (Script Helper)

Use o script `TutorialPieceConverter` (em breve) para converter automaticamente.

---

## 📋 Checklist de Conversão

Ao criar um prefab offline, verifique:

- [ ] Remover `Piece` → Adicionar `OfflinePiece`
- [ ] Remover `AnimPiece` → Adicionar `OfflineAnimPiece`
- [ ] Remover `SelectablePiece` → Adicionar `OfflineSelectablePiece`
- [ ] Remover `MovePiece` → Adicionar `OfflineMovePiece`
- [ ] Remover `AttackPiece` → Adicionar `OfflineAttackPiece`
- [ ] Remover `InteractivePiece` → Adicionar `OfflineInteractivePiece`
- [ ] Remover `Network Instantiate Detection`
- [ ] Configurar referências do Animator
- [ ] Configurar referências dos efeitos
- [ ] Testar na TutorialScene

---

## ⚙️ Diferenças Importantes

### 1. Sem NetworkManager
Os scripts offline **não precisam** de NetworkManager ativo.

### 2. Busca Local do BoardController
```csharp
// Busca através do TutorialBoardController primeiro
TutorialBoardController tutorialBoard = FindFirstObjectByType<TutorialBoardController>();
if (tutorialBoard != null)
{
    board = tutorialBoard.GetBoardController();
}
```

### 3. Sem Sons do MatchController
Sons de feedback (seleção, cancelar) foram removidos. Você pode adicionar AudioSource próprio se desejar.

### 4. Eventos do Tutorial Mantidos
```csharp
TutorialEvents.TriggerPieceAttacked(piece, target.piece);
```

---

## 🎯 Estrutura Recomendada de Prefabs

```
/Assets/Prefab/Pieces/
  /Online/                    # Prefabs originais (online)
    - SO Blue.prefab
    - SO Red.prefab
    - RE Blue.prefab
    - etc...
  
  /Tutorial/                  # Prefabs offline (tutorial)
    - SO Blue Tutorial.prefab
    - SO Red Tutorial.prefab
    - RE Blue Tutorial.prefab
    - etc...
```

---

## 🔍 Componentes Necessários

### Peça com Movimento
```
GameObject
├── OfflinePiece           (base offline)
├── OfflineAnimPiece
├── OfflineSelectablePiece
└── OfflineMovePiece
```

### Peça com Ataque
```
GameObject
├── OfflinePiece           (base offline)
├── OfflineAnimPiece
├── OfflineSelectablePiece
├── OfflineAttackPiece
└── OfflineInteractivePiece (base, pode ser adicionada automaticamente)
```

---

## 🐛 Troubleshooting

### Peça não se move
**Causa**: Referência do Animator não configurada  
**Solução**: No Inspector do `OfflineMovePiece`, arraste o componente `OfflineAnimPiece`

### Peça não ataca
**Causa**: Falta componente `OfflineInteractivePiece`  
**Solução**: `OfflineAttackPiece` herda de `OfflineInteractivePiece`, certifique-se que está adicionado

### BoardController não encontrado
**Causa**: TutorialBoardController não está na cena  
**Solução**: Adicione o componente `TutorialBoardController` no mesmo GameObject que tem o `BoardController`

### Animações não funcionam
**Causa**: Referência do Animator não configurada  
**Solução**: No `OfflineAnimPiece`, configure o campo `animator` no Inspector

---

## 💡 Exemplo de Uso no Tutorial

```csharp
// No TutorialStep
[SerializeField]
private TutorialSpawnData[] piecesToSpawn = new TutorialSpawnData[]
{
    new TutorialSpawnData
    {
        piecePrefab = soBlueOfflinePrefab,  // Use o prefab offline!
        fieldIndex = 44,
        isPlayerPiece = true
    }
};
```

---

## ✨ Vantagens dos Scripts Offline

1. ✅ **Independentes**: Não precisam de NetworkManager
2. ✅ **Simples**: Menos dependências e complexidade
3. ✅ **Rápidos**: Sem overhead de networking
4. ✅ **Específicos**: Feitos para tutorial
5. ✅ **Manuteníveis**: Separados do código online

---

## 🎓 Próximos Passos

1. Criar prefabs offline das suas peças principais
2. Testar movimento na TutorialScene
3. Testar ataque na TutorialScene
4. Usar nos TutorialSteps

---

**Bom tutorial offline! 🎮✨**
