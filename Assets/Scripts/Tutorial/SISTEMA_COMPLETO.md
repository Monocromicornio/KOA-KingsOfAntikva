# 🎯 Sistema de Tutorial - Kings of Antikva
## Documentação Completa do Sistema

---

## 📦 O Que Foi Criado

### Scripts Core
✅ **TutorialManager.cs** - Gerenciador principal do tutorial  
✅ **TutorialStep.cs** - ScriptableObject para cada etapa  
✅ **TutorialSequence.cs** - ScriptableObject para sequência de etapas  
✅ **TutorialEvents.cs** - Sistema de eventos globais  
✅ **TutorialModeController.cs** - Controla modo tutorial vs modo jogo  
✅ **TutorialStepType.cs** - Enum com tipos de etapas  
✅ **TutorialSpawnData.cs** - Dados de spawn de peças  
✅ **TutorialConditionChecker.cs** - Helper para condições customizadas  

### Scripts de Suporte
✅ **TutorialSceneSetup.cs** - Setup automático da cena  
✅ **TutorialBoardController.cs** - Helper para o tabuleiro  
✅ **TutorialDebugger.cs** - Debug e testes  

### Scripts de Editor
✅ **TutorialStepEditor.cs** - Editor customizado para TutorialStep  
✅ **TutorialSequenceEditor.cs** - Editor customizado para TutorialSequence  
✅ **BoardFieldIndexHelper.cs** - Mostra índices dos campos na cena  
✅ **TutorialValidator.cs** - Valida tutoriais antes de testar  
✅ **TutorialMenuItems.cs** - Menu de utilitários  

### Scripts de Exemplo
✅ **TutorialExample.cs** - Exemplos de eventos customizados  

### Documentação
✅ **README.md** - Documentação completa  
✅ **GUIA_RAPIDO.md** - Guia rápido de 5 minutos  
✅ **CreateTutorialExample.md** - Tutorial passo a passo  
✅ **SISTEMA_COMPLETO.md** - Este arquivo  

### Modificações em Scripts Existentes
✅ **Piece.cs** - Adicionados eventos e suporte a modo tutorial  
✅ **InteractivePiece.cs** - Evento de ataque  
✅ **GameField.cs** - Suporte a modo tutorial  

---

## 🎮 Como o Sistema Funciona

### Fluxo de Execução

```
1. TutorialManager inicia a TutorialSequence
   ↓
2. Carrega o primeiro TutorialStep
   ↓
3. Limpa tabuleiro (se configurado)
   ↓
4. Spawna peças definidas no step
   ↓
5. Mostra diálogo (se configurado)
   ↓
6. Aguarda condição de conclusão:
   - DialogueOnly: avança automaticamente
   - WaitForMovement: aguarda movimento
   - WaitForAttack: aguarda ataque
   - WaitForSelection: aguarda seleção
   - WaitForCustomCondition: aguarda trigger manual
   ↓
7. Executa eventos onStepComplete
   ↓
8. Aguarda delay configurado
   ↓
9. Vai para próximo step (volta ao passo 2)
   ↓
10. Quando acabam os steps: tutorial completo!
```

### Sistema de Eventos

O sistema usa eventos para detectar ações do jogador:

```csharp
TutorialEvents.OnPieceMoved    // Quando uma peça se move
TutorialEvents.OnPieceAttacked // Quando uma peça ataca
TutorialEvents.OnPieceSelected // Quando uma peça é selecionada
```

Estes eventos são disparados automaticamente pelos scripts modificados:
- `Piece.cs` dispara OnPieceMoved e OnPieceSelected
- `InteractivePiece.cs` dispara OnPieceAttacked

---

## 🛠️ Ferramentas de Editor

### Menu Window > Tutorial

1. **Open Tutorial Documentation** - Abre o README completo
2. **Open Quick Guide** - Abre o guia rápido
3. **Create Tutorial Folders** - Cria estrutura de pastas recomendada
4. **Find Tutorial Manager in Scene** - Encontra ou cria TutorialManager
5. **Tutorial Validator** - Valida sua sequência de tutorial

### Menu Assets > Create > Tutorial

1. **Tutorial Step** - Cria nova etapa
2. **Tutorial Sequence** - Cria nova sequência
3. **Complete Tutorial Example** - Cria exemplo completo

### Menu GameObject > Tutorial

1. **Create Tutorial System** - Cria GameObject com todos componentes necessários

### Board Field Index Helper

No Inspector do BoardController:
- Marque **"Show Field Indices in Scene"**
- Veja índices de todos os campos na Scene View

---

## 📋 Checklist de Implementação

### Setup Inicial
- [ ] Criar pasta `/Assets/Game/Tutorial`
- [ ] Criar subpastas `/Steps`, `/Sequences`, `/Dialogues`
- [ ] Adicionar TutorialSystem na TutorialScene

### Criar Primeiro Tutorial
- [ ] Criar DialogueBase para cada texto
- [ ] Criar TutorialSteps (mínimo 2: explicação + prática)
- [ ] Configurar peças para spawnar
- [ ] Descobrir Field Indices corretos
- [ ] Criar TutorialSequence
- [ ] Configurar TutorialManager na cena

### Testar
- [ ] Validar com Tutorial Validator
- [ ] Testar em Play Mode
- [ ] Verificar logs do debugger
- [ ] Ajustar delays e transições

### Polish
- [ ] Adicionar eventos customizados
- [ ] Configurar feedback visual/sonoro
- [ ] Testar fluxo completo
- [ ] Documentar tutoriais criados

---

## 🎨 Tipos de Tutorial Steps

### 1. DialogueOnly
**Uso**: Explicações, introduções, conclusões  
**Comportamento**: Mostra texto e avança automaticamente  
**Exemplo**: "Bem-vindo ao jogo!"

### 2. WaitForMovement
**Uso**: Ensinar movimento  
**Comportamento**: Aguarda jogador mover uma peça spawnada  
**Exemplo**: "Mova seu personagem para o campo verde"

### 3. WaitForAttack
**Uso**: Ensinar combate  
**Comportamento**: Aguarda jogador atacar com uma peça spawnada  
**Exemplo**: "Ataque o inimigo à sua frente"

### 4. WaitForSelection
**Uso**: Ensinar seleção de peças  
**Comportamento**: Aguarda jogador clicar em uma peça spawnada  
**Exemplo**: "Clique no seu personagem"

### 5. WaitForCustomCondition
**Uso**: Condições especiais  
**Comportamento**: Aguarda chamada manual de `CompleteCurrentStep()`  
**Exemplo**: Completar objetivo específico do jogo

---

## 🔍 Descobrindo Field Indices

### Método Visual (Recomendado)
1. Abra a TutorialScene
2. Selecione o BoardController
3. Marque "Show Field Indices in Scene"
4. Veja os números amarelos em cada campo

### Método por Cálculo (Tabuleiro 10x10)
```
Fórmula: index = (linha * 10) + coluna

Exemplo:
- Linha 5, Coluna 5 = 54
- Linha 4, Coluna 4 = 44
- Linha 6, Coluna 3 = 63
```

### Campos Úteis para Tutorial
```
Centro do tabuleiro: 44, 45, 54, 55
Cantos:
  - Superior esquerdo: 0
  - Superior direito: 9
  - Inferior esquerdo: 90
  - Inferior direito: 99
```

---

## ⚙️ Configurações Importantes

### TutorialModeController
```
Enable Tutorial Mode: true (na TutorialScene)
```
Isso desativa verificações de turno e MatchController.

### TutorialManager
```
Current Sequence: Sua TutorialSequence
Board Controller: Referência ao BoardController
Dialogue Manager: Referência ao DialogueManager
```

### TutorialDebugger (Opcional)
```
Enable Debug Logs: true
Show Step Transitions: true
Show Event Triggers: true
Skip Step Key: N
Restart Tutorial Key: R
```

---

## 🎯 Exemplos de Uso

### Tutorial Simples (2 Steps)

**Step 1: Boas-vindas**
```
Type: DialogueOnly
Dialogue: "Bem-vindo!"
Delay: 1.0s
```

**Step 2: Prática**
```
Type: WaitForMovement
Clear Board: true
Spawn: Player no campo 40
Dialogue: "Mova seu personagem"
```

### Tutorial Completo (5 Steps)

1. **Introdução** (DialogueOnly)
2. **Explicar Seleção** (DialogueOnly)
3. **Praticar Seleção** (WaitForSelection)
4. **Explicar Movimento** (DialogueOnly)
5. **Praticar Movimento** (WaitForMovement)
6. **Explicar Ataque** (DialogueOnly)
7. **Praticar Ataque** (WaitForAttack)
8. **Conclusão** (DialogueOnly)

---

## 🐛 Troubleshooting Comum

### Tutorial não inicia
**Causa**: TutorialModeController não configurado  
**Solução**: Ativar `enableTutorialMode = true`

### Peças não aparecem
**Causa**: Field Index inválido ou prefab não configurado  
**Solução**: Verificar índices com o helper visual

### Tutorial não avança
**Causa**: StepType incorreto ou peça errada sendo usada  
**Solução**: Verificar logs do TutorialDebugger

### Diálogo não aparece
**Causa**: DialogueManager não referenciado  
**Solução**: Configurar referência no TutorialManager

### Erro de null reference no MatchController
**Causa**: MatchController ativo em modo tutorial  
**Solução**: Desabilitar MatchController na TutorialScene

---

## 📊 Estrutura de Dados

### TutorialStep
```
- dialogue: DialogueBase
- stepType: TutorialStepType
- piecesToSpawn: TutorialSpawnData[]
- clearBoardBeforeSpawn: bool
- waitForDialogueEnd: bool
- onStepStart: UnityEvent
- onStepComplete: UnityEvent
- delayBeforeNextStep: float
```

### TutorialSequence
```
- tutorialName: string
- steps: TutorialStep[]
```

### TutorialSpawnData
```
- piecePrefab: Piece
- fieldIndex: int
- isPlayerPiece: bool
```

---

## 🚀 Próximos Passos

1. Criar seus DialogueBases com textos localizados
2. Usar `Window > Tutorial > Create Tutorial Folders`
3. Criar TutorialSteps básicos
4. Validar com `Window > Tutorial > Tutorial Validator`
5. Testar em Play Mode
6. Iterar e melhorar baseado em feedback

---

## 📞 Suporte

Para dúvidas ou problemas:
1. Consulte `GUIA_RAPIDO.md` para início rápido
2. Leia `README.md` para detalhes completos
3. Use `Tutorial Validator` para verificar erros
4. Ative `TutorialDebugger` para ver logs detalhados

---

**Bom tutorial! 🎮✨**
