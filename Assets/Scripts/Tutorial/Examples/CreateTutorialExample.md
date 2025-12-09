# Exemplo Prático: Criando um Tutorial Completo

## Passo a Passo para Criar o Tutorial de Ataque e Movimento

### 1. Criar os Dialogues

Primeiro, crie os DialogueBase assets:

#### Dialogue 1: Boas-vindas
- **Nome**: `Tutorial_Welcome`
- **String Table**: Sua tabela de localização
- **Dialogue Info**:
  - String ID: "tutorial_welcome"
  - Speaker: "Narrador"
  - Text: "Bem-vindo ao tutorial! Vamos aprender a jogar."

#### Dialogue 2: Explicação de Ataque
- **Nome**: `Tutorial_Attack_Explanation`
- **String ID**: "tutorial_attack_explain"
- **Text**: "Para atacar, você deve clicar no seu personagem e depois clicar no inimigo"

#### Dialogue 3: Explicação de Movimento
- **Nome**: `Tutorial_Movement_Explanation`
- **String ID**: "tutorial_movement_explain"
- **Text**: "Para mover, clique no seu personagem e depois clique no campo verde do tabuleiro"

### 2. Criar os Tutorial Steps

#### Step 1: Boas-vindas
```
Nome: Tutorial_Step_01_Welcome
- Dialogue: Tutorial_Welcome
- Step Type: DialogueOnly
- Pieces To Spawn: (vazio)
- Wait For Dialogue End: true
- Delay Before Next Step: 1.0
```

#### Step 2: Explicação de Ataque
```
Nome: Tutorial_Step_02_Attack_Explain
- Dialogue: Tutorial_Attack_Explanation
- Step Type: DialogueOnly
- Pieces To Spawn: (vazio)
- Wait For Dialogue End: true
- Delay Before Next Step: 0.5
```

#### Step 3: Prática de Ataque
```
Nome: Tutorial_Step_03_Attack_Practice
- Dialogue: (vazio ou repetir explicação)
- Step Type: WaitForAttack
- Clear Board Before Spawn: true
- Pieces To Spawn:
  [0]:
    - Piece Prefab: Seu prefab de soldado do jogador
    - Field Index: 20 (ajuste conforme seu tabuleiro)
    - Is Player Piece: true
  [1]:
    - Piece Prefab: Seu prefab de soldado inimigo
    - Field Index: 30 (campo adjacente ao jogador)
    - Is Player Piece: false
- Wait For Dialogue End: false
- Delay Before Next Step: 1.5
```

#### Step 4: Explicação de Movimento
```
Nome: Tutorial_Step_04_Movement_Explain
- Dialogue: Tutorial_Movement_Explanation
- Step Type: DialogueOnly
- Pieces To Spawn: (vazio)
- Wait For Dialogue End: true
- Delay Before Next Step: 0.5
```

#### Step 5: Prática de Movimento
```
Nome: Tutorial_Step_05_Movement_Practice
- Dialogue: (vazio)
- Step Type: WaitForMovement
- Clear Board Before Spawn: true
- Pieces To Spawn:
  [0]:
    - Piece Prefab: Seu prefab de soldado do jogador
    - Field Index: 15
    - Is Player Piece: true
- Wait For Dialogue End: false
- Delay Before Next Step: 1.0
```

#### Step 6: Conclusão
```
Nome: Tutorial_Step_06_Complete
- Dialogue: Tutorial_Complete (criar um dialogue de conclusão)
- Step Type: DialogueOnly
- Clear Board Before Spawn: true
- Pieces To Spawn: (vazio)
- Wait For Dialogue End: true
```

### 3. Criar a Tutorial Sequence

1. Clique com botão direito: `Create > Tutorial > Tutorial Sequence`
2. Nome: `Main_Tutorial_Sequence`
3. Tutorial Name: "Tutorial Básico"
4. Steps (arraste os steps criados na ordem):
   - [0] Tutorial_Step_01_Welcome
   - [1] Tutorial_Step_02_Attack_Explain
   - [2] Tutorial_Step_03_Attack_Practice
   - [3] Tutorial_Step_04_Movement_Explain
   - [4] Tutorial_Step_05_Movement_Practice
   - [5] Tutorial_Step_06_Complete

### 4. Configurar a TutorialScene

Na sua TutorialScene:

1. **Criar GameObject "TutorialSystem"**:
   - Adicione componente `TutorialModeController`
     - Enable Tutorial Mode: true
   - Adicione componente `TutorialManager`
     - Current Sequence: Main_Tutorial_Sequence
     - Board Controller: (arraste o BoardController da cena)
     - Dialogue Manager: (arraste o DialogueManager da cena)

2. **Criar GameObject "TutorialSetup"**:
   - Adicione componente `TutorialSceneSetup`
     - Tutorial Manager: (referência ao TutorialManager)
     - Board Controller: (referência ao BoardController)
     - Dialogue Manager: (referência ao DialogueManager)
     - Match Controller: (referência ao MatchController, se existir)
     - Tutorial Sequence: Main_Tutorial_Sequence

### 5. Como Encontrar os Field Index

Para descobrir qual índice usar para os campos:

1. Na cena, selecione o BoardController
2. No Inspector, expanda o array "Game Fields"
3. Os índices são mostrados em ordem (0, 1, 2, 3...)
4. Use o Scene view para identificar visualmente qual campo você quer
5. Anote o índice correspondente

**Dica**: Campos típicos para tutorial:
- Campo central: geralmente índice 40-45 (em um tabuleiro 10x10)
- Campos próximos: índice ± 1 (horizontal) ou ± 10 (vertical)

### 6. Testar o Tutorial

1. Abra a TutorialScene
2. Entre em Play Mode
3. O tutorial deve iniciar automaticamente
4. Siga as instruções na tela:
   - Leia o diálogo inicial
   - Quando aparecerem as peças de ataque, clique no personagem azul
   - Clique no personagem vermelho para atacar
   - Na etapa de movimento, clique no personagem
   - Clique em um campo verde para mover

### 7. Adicionar Eventos Customizados (Opcional)

Para adicionar feedback visual/sonoro:

1. Crie um GameObject com o componente `TutorialExample`
2. Configure os assets (UI, sons)
3. Nos Tutorial Steps, use os eventos:
   - `On Step Start`: arraste métodos como `ShowHighlight`
   - `On Step Complete`: arraste métodos como `HideHighlight`, `PlaySuccessSound`

## Dicas de Índices de Campos Comuns

Para um tabuleiro padrão 10x10 (100 campos):

```
Linha 1:  0-9
Linha 2:  10-19
Linha 3:  20-29
Linha 4:  30-39
Linha 5:  40-49 (meio do tabuleiro)
Linha 6:  50-59
Linha 7:  60-69
Linha 8:  70-79
Linha 9:  80-89
Linha 10: 90-99
```

Para colocar duas peças frente a frente:
- Peça do jogador: 44
- Peça inimiga: 54 (uma linha acima)

Ou horizontalmente:
- Peça do jogador: 44
- Peça inimiga: 45 (ao lado)
