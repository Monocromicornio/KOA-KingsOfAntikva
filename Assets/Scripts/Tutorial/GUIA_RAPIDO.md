# 🎮 Guia Rápido - Sistema de Tutorial

## ⚡ Setup Rápido (5 minutos)

### 0. Criar Prefabs Offline (IMPORTANTE!)

O tutorial precisa de **prefabs offline** das peças porque não usa NetworkManager.

**Opção A - Ferramenta Automática:**
1. `Window > Tutorial > Offline Piece Setup`
2. Arraste um prefab de peça (ex: SO Blue)
3. Clique em "Setup Selected GameObject in Scene"
4. Configure referências do Animator no Inspector
5. Salve como novo prefab: `SO Blue Tutorial.prefab`

**Opção B - Manual:**
Veja `/Assets/Scripts/Tutorial/Offline/README_OFFLINE.md`

### 1. Configurar a Cena de Tutorial

Na sua `TutorialScene`, crie esta estrutura:

```
TutorialScene
├── TutorialSystem (GameObject vazio)
│   ├── TutorialModeController
│   ├── TutorialManager
│   └── TutorialDebugger (opcional)
│
├── BoardController (já existente)
├── DialogueManager (já existente)
└── ... (outros objetos da cena)
```

**Configuração do TutorialManager**:
- Current Sequence: (sua TutorialSequence)
- Board Controller: arraste o BoardController
- Dialogue Manager: arraste o DialogueManager

### 2. Criar Seu Primeiro Tutorial (3 passos)

#### Passo 1: Criar TutorialStep
`Botão direito > Create > Tutorial > Tutorial Step`

Configure:
- **Dialogue**: Seu DialogueBase
- **Step Type**: Escolha entre:
  - `DialogueOnly` - Só mostra texto
  - `WaitForAttack` - Aguarda ataque
  - `WaitForMovement` - Aguarda movimento
  - `WaitForSelection` - Aguarda seleção

#### Passo 2: Adicionar Peças (se necessário)
No mesmo TutorialStep:
- **Pieces To Spawn** > Adicione elementos:
  - Piece Prefab: **USE PREFABS OFFLINE** (ex: SO Blue Tutorial)
  - Field Index: índice do campo (veja abaixo como descobrir)
  - Is Player Piece: true/false

#### Passo 3: Criar TutorialSequence
`Botão direito > Create > Tutorial > Tutorial Sequence`

- **Tutorial Name**: "Meu Tutorial"
- **Steps**: Arraste seus TutorialSteps

Pronto! ✅

---

## 🔍 Como Descobrir Field Index

### Método Visual (Recomendado):

1. Selecione o `BoardController` na cena
2. No Inspector, marque **"Show Field Indices in Scene"**
3. Na Scene View, você verá números amarelos em cada campo
4. Anote o número do campo que deseja usar

### Método Manual:

Para tabuleiro 10x10:
```
Exemplo: Duas peças frente a frente no centro
- Jogador: campo 44
- Inimigo: campo 54 (uma linha acima)
```

---

## 📝 Templates Prontos

### Tutorial de Ataque Básico

**Step 1: Explicação**
```
Step Type: DialogueOnly
Dialogue: "Clique no seu personagem, depois no inimigo para atacar"
```

**Step 2: Prática**
```
Step Type: WaitForAttack
Clear Board: ✓
Pieces To Spawn:
  [0] Player no campo 44
  [1] Enemy no campo 54
```

### Tutorial de Movimento Básico

**Step 1: Explicação**
```
Step Type: DialogueOnly
Dialogue: "Clique no personagem, depois no campo verde para mover"
```

**Step 2: Prática**
```
Step Type: WaitForMovement
Clear Board: ✓
Pieces To Spawn:
  [0] Player no campo 40
```

---

## 🎯 Dicas Importantes

### ✅ Boas Práticas:
- Use `DialogueOnly` para explicações
- Use `WaitFor...` para prática
- Sempre marque `Clear Board Before Spawn: true` nas etapas de prática
- Use `Delay Before Next Step: 1.0` para dar tempo ao jogador

### ⚠️ Evite:
- Spawnar peças em campos inválidos
- Esquecer de configurar o BoardController
- Não limpar o tabuleiro entre etapas

---

## 🐛 Debug e Testes

Adicione o componente `TutorialDebugger` no GameObject do tutorial:

**Atalhos de Teclado**:
- `N` - Pular etapa atual
- `R` - Reiniciar tutorial

**Logs**:
- Mostra quando peças são movidas/atacadas/selecionadas
- Mostra transições entre etapas

---

## 🔧 Troubleshooting

### Problema: Tutorial não inicia
**Solução**: 
- Verifique se `TutorialModeController.enableTutorialMode = true`
- Confira se a `TutorialSequence` está configurada no `TutorialManager`

### Problema: Peças não aparecem
**Solução**:
- Verifique se os Field Index são válidos
- Confirme se os prefabs estão configurados
- Veja se `Clear Board` não está removendo as peças

### Problema: Tutorial não avança após ação
**Solução**:
- Verifique se o `StepType` está correto
- Confirme se as peças spawnadas são as que estão sendo usadas
- Ative os logs do `TutorialDebugger`

---

## 📚 Estrutura de Arquivos Recomendada

```
/Assets
  /Scripts
    /Tutorial
      ✓ TutorialManager.cs
      ✓ TutorialStep.cs
      ✓ TutorialSequence.cs
      ✓ TutorialEvents.cs
      ✓ ...outros scripts

  /Game
    /Tutorial
      /Steps
        - Tutorial_Step_01_Welcome.asset
        - Tutorial_Step_02_Attack_Explain.asset
        - Tutorial_Step_03_Attack_Practice.asset
        - ...
      /Sequences
        - Main_Tutorial.asset
        - Advanced_Tutorial.asset
      /Dialogues
        - Tutorial_Welcome.asset
        - Tutorial_Attack.asset
        - ...
```

---

## 🎓 Exemplos Avançados

### Usar Eventos Customizados

No TutorialStep, configure:
- **On Step Start**: 
  - Ativar UI especial
  - Tocar som
  - Mostrar dica visual

- **On Step Complete**:
  - Desativar UI
  - Tocar som de sucesso
  - Dar recompensa

### Criar Condição Customizada

```csharp
public class MyCustomCondition : MonoBehaviour
{
    void OnPlayerDoSomethingSpecial()
    {
        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.CompleteCurrentStep();
        }
    }
}
```

Configure o Step:
```
Step Type: WaitForCustomCondition
```

---

## 📞 Próximos Passos

1. ✅ Criar seus DialogueBases
2. ✅ Criar TutorialSteps básicos
3. ✅ Montar sua TutorialSequence
4. ✅ Testar na TutorialScene
5. ✅ Adicionar eventos e polish
6. ✅ Criar tutoriais avançados

**Leia também**: 
- `README.md` - Documentação completa
- `CreateTutorialExample.md` - Exemplo detalhado passo a passo
