# 📋 Template de Tutorial - Use Como Base

## Tutorial Básico Completo (5 Etapas)

### Etapa 1: Boas-Vindas
```
Nome do Asset: Tutorial_01_Welcome
```

**Configuração:**
- Dialogue: [Criar DialogueBase "Tutorial_Welcome"]
  - Text: "Bem-vindo ao tutorial do Kings of Antikva! Vamos aprender a jogar."
  - Speaker: "Narrador"
- Step Type: `DialogueOnly`
- Pieces To Spawn: (vazio)
- Clear Board Before Spawn: false
- Wait For Dialogue End: true
- Delay Before Next Step: 0.5

---

### Etapa 2: Explicar Seleção
```
Nome do Asset: Tutorial_02_Selection_Explain
```

**Configuração:**
- Dialogue: [Criar DialogueBase "Tutorial_Selection"]
  - Text: "Para selecionar um personagem, clique nele com o mouse."
- Step Type: `DialogueOnly`
- Pieces To Spawn: (vazio)
- Wait For Dialogue End: true
- Delay Before Next Step: 0.5

---

### Etapa 3: Praticar Seleção
```
Nome do Asset: Tutorial_03_Selection_Practice
```

**Configuração:**
- Dialogue: [Opcional - pode repetir a explicação ou deixar vazio]
- Step Type: `WaitForSelection`
- Pieces To Spawn:
  ```
  Element 0:
    - Piece Prefab: [Seu prefab de soldado]
    - Field Index: 44
    - Is Player Piece: true
  ```
- Clear Board Before Spawn: true
- Wait For Dialogue End: false
- Delay Before Next Step: 1.0

**Events:**
- On Step Complete: PlaySuccessSound (opcional)

---

### Etapa 4: Explicar Movimento
```
Nome do Asset: Tutorial_04_Movement_Explain
```

**Configuração:**
- Dialogue: [Criar DialogueBase "Tutorial_Movement"]
  - Text: "Muito bem! Agora vamos aprender a mover. Clique no personagem e depois clique em um campo verde."
- Step Type: `DialogueOnly`
- Pieces To Spawn: (vazio)
- Wait For Dialogue End: true
- Delay Before Next Step: 0.5

---

### Etapa 5: Praticar Movimento
```
Nome do Asset: Tutorial_05_Movement_Practice
```

**Configuração:**
- Dialogue: [Opcional]
- Step Type: `WaitForMovement`
- Pieces To Spawn:
  ```
  Element 0:
    - Piece Prefab: [Seu prefab de soldado]
    - Field Index: 40
    - Is Player Piece: true
  ```
- Clear Board Before Spawn: true
- Wait For Dialogue End: false
- Delay Before Next Step: 1.5

**Events:**
- On Step Complete: PlaySuccessSound

---

### Etapa 6: Explicar Ataque
```
Nome do Asset: Tutorial_06_Attack_Explain
```

**Configuração:**
- Dialogue: [Criar DialogueBase "Tutorial_Attack"]
  - Text: "Excelente! Agora vamos atacar. Clique no seu personagem e depois clique no inimigo."
- Step Type: `DialogueOnly`
- Pieces To Spawn: (vazio)
- Wait For Dialogue End: true
- Delay Before Next Step: 0.5

---

### Etapa 7: Praticar Ataque
```
Nome do Asset: Tutorial_07_Attack_Practice
```

**Configuração:**
- Dialogue: [Opcional]
- Step Type: `WaitForAttack`
- Pieces To Spawn:
  ```
  Element 0:
    - Piece Prefab: [Seu prefab de soldado]
    - Field Index: 44
    - Is Player Piece: true
  
  Element 1:
    - Piece Prefab: [Seu prefab de soldado inimigo]
    - Field Index: 54 (campo acima do jogador)
    - Is Player Piece: false
  ```
- Clear Board Before Spawn: true
- Wait For Dialogue End: false
- Delay Before Next Step: 2.0

**Events:**
- On Step Complete: PlaySuccessSound

---

### Etapa 8: Conclusão
```
Nome do Asset: Tutorial_08_Complete
```

**Configuração:**
- Dialogue: [Criar DialogueBase "Tutorial_Complete"]
  - Text: "Parabéns! Você completou o tutorial básico. Agora está pronto para jogar!"
- Step Type: `DialogueOnly`
- Pieces To Spawn: (vazio)
- Clear Board Before Spawn: true
- Wait For Dialogue End: true
- Delay Before Next Step: 0

---

## Tutorial Sequence

```
Nome do Asset: Main_Tutorial_Sequence
```

**Configuração:**
- Tutorial Name: "Tutorial Básico - Kings of Antikva"
- Steps (em ordem):
  ```
  [0] Tutorial_01_Welcome
  [1] Tutorial_02_Selection_Explain
  [2] Tutorial_03_Selection_Practice
  [3] Tutorial_04_Movement_Explain
  [4] Tutorial_05_Movement_Practice
  [5] Tutorial_06_Attack_Explain
  [6] Tutorial_07_Attack_Practice
  [7] Tutorial_08_Complete
  ```

---

## Setup da Cena

### GameObject: TutorialSystem

**Componentes:**

1. **TutorialModeController**
   - Enable Tutorial Mode: ✓ true

2. **TutorialManager**
   - Current Sequence: Main_Tutorial_Sequence
   - Board Controller: [Arraste o BoardController da cena]
   - Dialogue Manager: [Arraste o DialogueManager da cena]

3. **TutorialDebugger** (opcional)
   - Enable Debug Logs: ✓ true
   - Show Step Transitions: ✓ true
   - Show Event Triggers: ✓ true
   - Skip Step Key: N
   - Restart Tutorial Key: R

---

## Campos Recomendados (Tabuleiro 10x10)

### Para Praticar Seleção
```
Campo 44 (centro do tabuleiro)
```

### Para Praticar Movimento
```
Campo 40 (pode mover para 41, 50, etc)
```

### Para Praticar Ataque
```
Jogador: Campo 44
Inimigo: Campo 54 (uma linha acima)
```

### Visualizar Campos
1. Selecione BoardController
2. Marque "Show Field Indices in Scene"
3. Veja os números na Scene View

---

## DialogueBases Necessários

Crie estes DialogueBases em `/Assets/Game/Tutorial/Dialogues/`:

1. **Tutorial_Welcome**
   - String ID: "tutorial_welcome"
   - Text: Mensagem de boas-vindas

2. **Tutorial_Selection**
   - String ID: "tutorial_selection"
   - Text: Como selecionar

3. **Tutorial_Movement**
   - String ID: "tutorial_movement"
   - Text: Como mover

4. **Tutorial_Attack**
   - String ID: "tutorial_attack"
   - Text: Como atacar

5. **Tutorial_Complete**
   - String ID: "tutorial_complete"
   - Text: Mensagem de conclusão

---

## Checklist de Criação

### Preparação
- [ ] Criar pasta `/Assets/Game/Tutorial`
- [ ] Criar subpastas `/Steps`, `/Sequences`, `/Dialogues`
- [ ] Criar 5 DialogueBases com textos

### Steps (8 etapas)
- [ ] Tutorial_01_Welcome
- [ ] Tutorial_02_Selection_Explain
- [ ] Tutorial_03_Selection_Practice
- [ ] Tutorial_04_Movement_Explain
- [ ] Tutorial_05_Movement_Practice
- [ ] Tutorial_06_Attack_Explain
- [ ] Tutorial_07_Attack_Practice
- [ ] Tutorial_08_Complete

### Sequence
- [ ] Main_Tutorial_Sequence com todas as etapas

### Cena
- [ ] TutorialSystem GameObject criado
- [ ] TutorialManager configurado
- [ ] Referências setadas

### Validação
- [ ] Validar com Tutorial Validator
- [ ] Testar em Play Mode
- [ ] Verificar todos os steps funcionam

---

## Variações do Template

### Tutorial Curto (3 etapas)
1. Welcome
2. Practice Movement
3. Complete

### Tutorial Médio (5 etapas)
1. Welcome
2. Explain + Practice Movement
3. Explain + Practice Attack
4. Complete

### Tutorial Longo (Este template - 8 etapas)
Explicação + Prática para cada mecânica

---

## Dicas de Customização

### Adicionar Feedback Visual
```csharp
// No TutorialExample.cs
public void HighlightCharacter()
{
    // Ativar highlight visual
}

public void RemoveHighlight()
{
    // Desativar highlight
}
```

Use nos eventos:
- On Step Start: HighlightCharacter
- On Step Complete: RemoveHighlight

### Adicionar Sons
Configure no TutorialExample:
- Success Sound para steps concluídos
- Error Sound para ações erradas

### Adicionar UI Customizada
- Crie UI Canvas com dicas
- Ative/desative via eventos

---

## Testando o Tutorial

1. Abra a TutorialScene
2. Entre em Play Mode
3. Siga as instruções:
   - Leia mensagem de boas-vindas
   - Clique no personagem (seleção)
   - Mova o personagem para campo verde
   - Ataque o inimigo
   - Veja mensagem de conclusão

4. Use atalhos para debug:
   - N: Pular step
   - R: Reiniciar

---

**Use este template como base e customize conforme necessário! 🎮**
