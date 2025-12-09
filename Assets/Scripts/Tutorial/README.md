# Sistema de Tutorial - Kings of Antikva

## Visão Geral

Este sistema permite criar tutoriais modulares e interativos que se integram com o DialogueSystem do jogo.

## Como Usar

### 1. Criar uma Etapa de Tutorial (TutorialStep)

1. Clique com botão direito na pasta Project
2. Selecione `Create > Tutorial > Tutorial Step`
3. Configure a etapa:

#### Configurações Principais:

- **Dialogue**: Arraste um DialogueBase para mostrar texto
- **Step Type**: Escolha o tipo de condição:
  - `DialogueOnly`: Apenas mostra diálogo e prossegue
  - `WaitForMovement`: Aguarda o jogador mover uma peça
  - `WaitForAttack`: Aguarda o jogador atacar
  - `WaitForSelection`: Aguarda o jogador selecionar uma peça
  - `WaitForCustomCondition`: Para condições personalizadas

#### Board Setup:

- **Pieces To Spawn**: Lista de peças para spawnar
  - `Piece Prefab`: O prefab da peça
  - `Field Index`: O índice do campo no tabuleiro
  - `Is Player Piece`: Se é peça do jogador (azul) ou inimigo (vermelho)
- **Clear Board Before Spawn**: Limpa peças anteriores antes de spawnar novas

#### Completion Conditions:

- **Wait For Dialogue End**: Aguarda o diálogo terminar antes de verificar condições

### 2. Criar uma Sequência de Tutorial (TutorialSequence)

1. Clique com botão direito na pasta Project
2. Selecione `Create > Tutorial > Tutorial Sequence`
3. Arraste as TutorialSteps criadas para o array `Steps` na ordem desejada

### 3. Configurar o TutorialManager

1. Adicione o componente `TutorialManager` em um GameObject na cena
2. Configure as referências:
   - `Current Sequence`: A sequência de tutorial a executar
   - `Board Controller`: Referência ao BoardController da cena
   - `Dialogue Manager`: Referência ao DialogueManager

## Exemplo de Uso

### Tutorial de Ataque:

**Etapa 1 - Explicação**
- Dialogue: "Para atacar, clique no seu personagem e depois no inimigo"
- Step Type: DialogueOnly
- Pieces To Spawn: (vazio)

**Etapa 2 - Prática**
- Dialogue: (pode ser vazio ou reforçar a instrução)
- Step Type: WaitForAttack
- Pieces To Spawn:
  - Peça 0: Player piece no campo 10
  - Peça 1: Enemy piece no campo 20
- Clear Board: true

### Tutorial de Movimento:

**Etapa 1 - Explicação**
- Dialogue: "Para mover, clique no personagem e depois no campo verde"
- Step Type: DialogueOnly

**Etapa 2 - Prática**
- Step Type: WaitForMovement
- Pieces To Spawn:
  - Peça 0: Player piece no campo 15
- Clear Board: true

## Eventos

Você pode usar os eventos `onStepStart` e `onStepComplete` em cada TutorialStep para executar ações customizadas:

- Ativar/desativar elementos da UI
- Tocar sons especiais
- Mostrar hints visuais
- Etc.

## Condições Customizadas

Para criar condições customizadas, use o componente `TutorialConditionChecker`:

1. Adicione o componente em um GameObject
2. Chame `SetConditionMet()` quando a condição for satisfeita
3. Configure a TutorialStep com `StepType.WaitForCustomCondition`

## Scripts Principais

- **TutorialManager**: Gerencia a execução do tutorial
- **TutorialStep**: Define cada etapa individual
- **TutorialSequence**: Agrupa etapas em uma sequência
- **TutorialEvents**: Sistema de eventos para detectar ações do jogador
- **TutorialConditionChecker**: Helper para condições customizadas

## Notas

- O sistema automaticamente spawna e remove peças conforme necessário
- As peças spawnadas são rastreadas e limpas ao final do tutorial
- O sistema se integra perfeitamente com o DialogueSystem existente
