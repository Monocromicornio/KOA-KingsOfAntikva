using UnityEngine;
using UnityEditor;

public class OfflinePieceSetup : EditorWindow
{
    private GameObject selectedPrefab;
    private bool removeOnlineScripts = true;
    private bool addOfflineScripts = true;

    [MenuItem("Window/Tutorial/Offline Piece Setup")]
    public static void ShowWindow()
    {
        GetWindow<OfflinePieceSetup>("Offline Piece Setup");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Offline Piece Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Esta ferramenta ajuda a converter prefabs de peças online para versões offline usadas no tutorial.",
            MessageType.Info);

        EditorGUILayout.Space();
        
        selectedPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab Source",
            selectedPrefab,
            typeof(GameObject),
            false
        );

        EditorGUILayout.Space();
        
        removeOnlineScripts = EditorGUILayout.Toggle("Remove Online Scripts", removeOnlineScripts);
        addOfflineScripts = EditorGUILayout.Toggle("Add Offline Scripts", addOfflineScripts);

        EditorGUILayout.Space();

        GUI.enabled = selectedPrefab != null;
        if (GUILayout.Button("Setup Selected GameObject in Scene", GUILayout.Height(40)))
        {
            SetupOfflinePiece(selectedPrefab);
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "INSTRUÇÕES:\n\n" +
            "1. Arraste um prefab ou selecione um GameObject na cena\n" +
            "2. Clique em 'Setup Selected GameObject'\n" +
            "3. Os scripts online serão removidos (opcional)\n" +
            "4. Os scripts offline serão adicionados (opcional)\n" +
            "5. Configure as referências manualmente no Inspector\n" +
            "6. Salve como novo prefab em /Prefab/Pieces/Tutorial/",
            MessageType.None);

        EditorGUILayout.Space();

        if (GUILayout.Button("Open Tutorial Pieces Folder"))
        {
            string path = "Assets/Prefab/Pieces";
            if (System.IO.Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Folder Not Found",
                    "A pasta 'Assets/Prefab/Pieces' não foi encontrada.",
                    "OK");
            }
        }
    }

    private void SetupOfflinePiece(GameObject target)
    {
        if (target == null)
        {
            EditorUtility.DisplayDialog("Erro", "Nenhum GameObject selecionado!", "OK");
            return;
        }

        GameObject instance = PrefabUtility.IsPartOfPrefabAsset(target) 
            ? (GameObject)PrefabUtility.InstantiatePrefab(target) 
            : target;

        int operationsCount = 0;

        if (removeOnlineScripts)
        {
            operationsCount += RemoveOnlineScripts(instance);
        }

        if (addOfflineScripts)
        {
            operationsCount += AddOfflineScripts(instance);
        }

        if (operationsCount > 0)
        {
            EditorUtility.SetDirty(instance);
            
            string message = $"Setup concluído!\n\n" +
                           $"Operações realizadas: {operationsCount}\n\n" +
                           $"PRÓXIMOS PASSOS:\n" +
                           $"1. Configure as referências no Inspector\n" +
                           $"2. Salve como prefab em Assets/Prefab/Pieces/Tutorial/\n" +
                           $"3. Teste na TutorialScene";
            
            EditorUtility.DisplayDialog("Setup Completo", message, "OK");
            Selection.activeGameObject = instance;
        }
        else
        {
            EditorUtility.DisplayDialog("Nenhuma Operação", 
                "Nenhuma operação foi realizada. Verifique as opções selecionadas.", 
                "OK");
        }
    }

    private int RemoveOnlineScripts(GameObject target)
    {
        int removed = 0;

        Component piece = target.GetComponent("Piece");
        if (piece != null)
        {
            DestroyImmediate(piece);
            removed++;
        }

        Component animPiece = target.GetComponent("AnimPiece");
        if (animPiece != null)
        {
            DestroyImmediate(animPiece);
            removed++;
        }

        Component selectablePiece = target.GetComponent("SelectablePiece");
        if (selectablePiece != null)
        {
            DestroyImmediate(selectablePiece);
            removed++;
        }

        Component movePiece = target.GetComponent("MovePiece");
        if (movePiece != null)
        {
            DestroyImmediate(movePiece);
            removed++;
        }

        Component attackPiece = target.GetComponent("AttackPiece");
        if (attackPiece != null)
        {
            DestroyImmediate(attackPiece);
            removed++;
        }

        Component interactivePiece = target.GetComponent("InteractivePiece");
        if (interactivePiece != null)
        {
            DestroyImmediate(interactivePiece);
            removed++;
        }

        Component fakePiece = target.GetComponent("FakePiece");
        if (fakePiece != null)
        {
            DestroyImmediate(fakePiece);
            removed++;
        }

        Component networkDetection = target.GetComponent("NetworkInstantiateDetection");
        if (networkDetection != null)
        {
            DestroyImmediate(networkDetection);
            removed++;
        }

        return removed;
    }

    private int AddOfflineScripts(GameObject target)
    {
        int added = 0;

        if (target.GetComponent<OfflinePiece>() == null)
        {
            target.AddComponent<OfflinePiece>();
            added++;
        }

        if (target.GetComponent<OfflineAnimPiece>() == null)
        {
            target.AddComponent<OfflineAnimPiece>();
            added++;
        }

        if (target.GetComponent<OfflineSelectablePiece>() == null)
        {
            target.AddComponent<OfflineSelectablePiece>();
            added++;
        }

        bool hasMovePiece = target.GetComponent("MovePiece") != null || 
                           target.GetComponent<OfflineMovePiece>() != null;
        
        if (hasMovePiece && target.GetComponent<OfflineMovePiece>() == null)
        {
            target.AddComponent<OfflineMovePiece>();
            added++;
        }

        bool hasAttackPiece = target.GetComponent("AttackPiece") != null || 
                             target.GetComponent<OfflineAttackPiece>() != null;
        
        if (hasAttackPiece && target.GetComponent<OfflineAttackPiece>() == null)
        {
            target.AddComponent<OfflineAttackPiece>();
            added++;
        }

        return added;
    }
}
