# ⚡ Guia Rápido de Conversão de Prefab

## 🎯 Objetivo

Converter um prefab online (ex: `SO Blue.prefab`) para versão offline (`SO Blue Tutorial.prefab`) em **2 minutos**.

---

## 📋 Passo a Passo (Método Recomendado)

### 1️⃣ Abrir Ferramenta (5 segundos)

```
Window > Tutorial > Offline Piece Setup
```

### 2️⃣ Instanciar Prefab na Cena (10 segundos)

1. Arraste o prefab original (ex: `SO Blue`) para a cena
2. Selecione o GameObject na Hierarchy

### 3️⃣ Usar Ferramenta (10 segundos)

1. Na janela `Offline Piece Setup`, arraste o GameObject da cena para o campo `Prefab Source`
2. Certifique-se que ambas opções estão marcadas:
   - ✅ Remove Online Scripts
   - ✅ Add Offline Scripts
3. Clique em **"Setup Selected GameObject in Scene"**

### 4️⃣ Configurar Referências (30 segundos)

No Inspector do GameObject:

#### OfflinePiece
- ✅ Verificar que `Piece Type` está configurado
- ✅ Verificar que `Body` está preenchido

#### OfflineAnimPiece
- ✅ Campo `Animator`: arraste o componente `Animator` do próprio GameObject
- ✅ Campo `G Die`: arraste o efeito de morte (se houver)
- ✅ Campo `Au Die`: arraste o AudioSource de morte (se houver)
- ✅ Campo `Au Down`: arraste o AudioSource de queda (se houver)

#### OfflineMovePiece
- ✅ Campo `Anim`: arraste o componente `OfflineAnimPiece`
- ✅ Verificar valores de movimento (Move Speed, Lift Height, etc)

#### OfflineAttackPiece (se houver)
- ✅ Campo `Anim`: arraste o componente `OfflineAnimPiece`
- ✅ Campo `Force`: configure a força do ataque
- ✅ Campo `Attack Effect`: arraste o efeito de ataque (se houver)

### 5️⃣ Salvar como Prefab (20 segundos)

1. Arraste o GameObject da Hierarchy para a pasta `Assets/Prefab/Pieces/Tutorial/`
2. Renomeie para `SO Blue Tutorial` (ou nome apropriado)
3. Delete o GameObject da cena

### 6️⃣ Testar (45 segundos)

1. Abra `TutorialScene`
2. Arraste o novo prefab para a cena
3. Play Mode
4. Teste seleção, movimento e ataque

---

## 📝 Checklist Visual

```
Antes (Online):
SO Blue.prefab
├── ✅ Piece              
├── ✅ AnimPiece          
├── ✅ SelectablePiece    
├── ✅ MovePiece          
└── ✅ NetworkInstantiate 

Depois (Offline):
SO Blue Tutorial.prefab
├── ✅ OfflinePiece           
├── ✅ OfflineAnimPiece       (Animator configurado!)
├── ✅ OfflineSelectablePiece 
└── ✅ OfflineMovePiece       (Anim configurado!)
```

---

## 🎯 Configurações Críticas

### ⚠️ SEMPRE Configure Estes Campos

| Componente | Campo | O Que Arrastar |
|-----------|-------|----------------|
| `OfflineAnimPiece` | `Animator` | Componente Animator do próprio GameObject |
| `OfflineMovePiece` | `Anim` | Componente OfflineAnimPiece |
| `OfflineAttackPiece` | `Anim` | Componente OfflineAnimPiece |

**Se não configurar:** As animações não funcionarão! 🚫

---

## 🔍 Verificação Rápida

Antes de salvar, verifique:

- [ ] Nenhum script "online" restante (Piece, AnimPiece, etc)
- [ ] Todos scripts offline adicionados
- [ ] Campo `Animator` no `OfflineAnimPiece` preenchido
- [ ] Campo `Anim` no `OfflineMovePiece` preenchido
- [ ] Campo `Anim` no `OfflineAttackPiece` preenchido (se houver)
- [ ] Prefab salvo com nome "Tutorial" no final

---

## 🚀 Método Alternativo (Manual)

Se preferir fazer manualmente:

### 1. Duplicar Prefab
```
1. Duplicate "SO Blue.prefab"
2. Renomear para "SO Blue Tutorial.prefab"
3. Abrir prefab no modo de edição
```

### 2. Remover Scripts Online
```
Remover (em ordem):
1. Network Instantiate Detection
2. AttackPiece (se houver)
3. InteractivePiece (se houver)
4. MovePiece
5. SelectablePiece
6. AnimPiece
7. Piece
```

### 3. Adicionar Scripts Offline
```
Adicionar (em ordem):
1. OfflinePiece
2. OfflineAnimPiece
3. OfflineSelectablePiece
4. OfflineMovePiece
5. OfflineAttackPiece (se era peça de ataque)
```

### 4. Configurar Referências
Mesmo que no método recomendado (passo 4)

---

## 💡 Dicas Rápidas

### Copiar Valores
Se quiser manter os valores dos campos antigos:

1. **Antes de remover** o script online, anote os valores no Inspector
2. Ou tire um screenshot do Inspector
3. Depois de adicionar o script offline, configure com os mesmos valores

### Peças Iguais
Se vai converter várias peças similares (ex: SO Blue, SO Red):

1. Converta uma completamente
2. Use como referência para as outras
3. Os valores geralmente são os mesmos

### Animator Compartilhado
Muitas peças compartilham o mesmo Animator Controller:

- Verifique se o campo `Animator` já está configurado no prefab original
- Se sim, deve ser mantido automaticamente

---

## ⚠️ Problemas Comuns

### "NullReferenceException no OfflineAnimPiece"
**Causa:** Campo `Animator` não configurado  
**Solução:** Arraste o componente `Animator` para o campo no Inspector

### "Peça não se move"
**Causa:** Campo `Anim` do `OfflineMovePiece` não configurado  
**Solução:** Arraste o componente `OfflineAnimPiece` para o campo

### "Componentes faltando"
**Causa:** Ferramenta não detectou todos os componentes necessários  
**Solução:** Adicione manualmente os componentes faltantes

### "Erro ao salvar prefab"
**Causa:** Ainda há scripts online no GameObject  
**Solução:** Remova todos os scripts online antes de salvar

---

## ✅ Resultado Final

Depois da conversão, você terá:

```
/Assets/Prefab/Pieces/Tutorial/
  ✅ SO Blue Tutorial.prefab
  ✅ SO Red Tutorial.prefab
  ✅ RE Blue Tutorial.prefab
  ... etc
```

Cada prefab:
- ✅ Funciona sem NetworkManager
- ✅ Funciona no tutorial
- ✅ Mantém todas as funcionalidades
- ✅ Animações funcionam
- ✅ Movimento funciona
- ✅ Ataque funciona

---

## 🎓 Próximo Passo

Depois de criar os prefabs offline:

1. Use no `TutorialStep`:
   ```
   Pieces To Spawn:
   - Piece Prefab: SO Blue Tutorial
   - Field Index: 44
   - Is Player Piece: true
   ```

2. Teste na `TutorialScene`

3. Crie mais etapas do tutorial!

---

**Conversão concluída! Agora você tem prefabs offline funcionais! 🎮✨**
