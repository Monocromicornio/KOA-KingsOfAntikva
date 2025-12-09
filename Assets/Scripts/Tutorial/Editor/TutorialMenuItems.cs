using UnityEngine;
using UnityEditor;
using System.IO;

public class TutorialMenuItems
{
    private const string TUTORIAL_FOLDER = "Assets/Game/Tutorial";
    private const string STEPS_FOLDER = TUTORIAL_FOLDER + "/Steps";
    private const string SEQUENCES_FOLDER = TUTORIAL_FOLDER + "/Sequences";

    [MenuItem("Window/Tutorial/Open Tutorial Documentation")]
    public static void OpenDocumentation()
    {
        string readmePath = "Assets/Scripts/Tutorial/README.md";
        if (File.Exists(readmePath))
        {
            Application.OpenURL(Path.GetFullPath(readmePath));
        }
        else
        {
            EditorUtility.DisplayDialog("Documentação", 
                "README.md não encontrado em Assets/Scripts/Tutorial/", 
                "OK");
        }
    }

    [MenuItem("Window/Tutorial/Open Quick Guide")]
    public static void OpenQuickGuide()
    {
        string guidePath = "Assets/Scripts/Tutorial/GUIA_RAPIDO.md";
        if (File.Exists(guidePath))
        {
            Application.OpenURL(Path.GetFullPath(guidePath));
        }
        else
        {
            EditorUtility.DisplayDialog("Guia Rápido", 
                "GUIA_RAPIDO.md não encontrado em Assets/Scripts/Tutorial/", 
                "OK");
        }
    }

    [MenuItem("Window/Tutorial/Create Tutorial Folders")]
    public static void CreateTutorialFolders()
    {
        CreateFolderIfNotExists(TUTORIAL_FOLDER);
        CreateFolderIfNotExists(STEPS_FOLDER);
        CreateFolderIfNotExists(SEQUENCES_FOLDER);
        
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Tutorial Folders", 
            "Pastas criadas com sucesso:\n\n" +
            TUTORIAL_FOLDER + "\n" +
            STEPS_FOLDER + "\n" +
            SEQUENCES_FOLDER, 
            "OK");
    }

    [MenuItem("Window/Tutorial/Find Tutorial Manager in Scene")]
    public static void FindTutorialManager()
    {
        TutorialManager manager = Object.FindFirstObjectByType<TutorialManager>();
        
        if (manager != null)
        {
            Selection.activeGameObject = manager.gameObject;
            EditorGUIUtility.PingObject(manager.gameObject);
        }
        else
        {
            bool create = EditorUtility.DisplayDialog(
                "Tutorial Manager", 
                "Nenhum TutorialManager encontrado na cena.\n\nDeseja criar um?", 
                "Sim", 
                "Não");
            
            if (create)
            {
                CreateTutorialManager();
            }
        }
    }

    [MenuItem("GameObject/Tutorial/Create Tutorial System", false, 10)]
    public static void CreateTutorialManager()
    {
        GameObject tutorialSystem = new GameObject("TutorialSystem");
        
        tutorialSystem.AddComponent<TutorialModeController>();
        tutorialSystem.AddComponent<TutorialManager>();
        tutorialSystem.AddComponent<TutorialDebugger>();
        
        Selection.activeGameObject = tutorialSystem;
        
        EditorUtility.DisplayDialog(
            "Tutorial System", 
            "Tutorial System criado com sucesso!\n\n" +
            "Componentes adicionados:\n" +
            "- TutorialModeController\n" +
            "- TutorialManager\n" +
            "- TutorialDebugger\n\n" +
            "Configure as referências no Inspector.", 
            "OK");
    }

    [MenuItem("Assets/Create/Tutorial/Complete Tutorial Example", priority = 0)]
    public static void CreateCompleteTutorialExample()
    {
        string folderPath = GetSelectedFolderPath();
        
        TutorialStep step1 = CreateTutorialStep(folderPath, "Tutorial_Step_01_Welcome");
        step1.stepType = TutorialStepType.DialogueOnly;
        step1.delayBeforeNextStep = 1.0f;
        
        TutorialStep step2 = CreateTutorialStep(folderPath, "Tutorial_Step_02_Practice");
        step2.stepType = TutorialStepType.WaitForMovement;
        step2.clearBoardBeforeSpawn = true;
        step2.piecesToSpawn = new TutorialSpawnData[1];
        step2.piecesToSpawn[0] = new TutorialSpawnData { fieldIndex = 40, isPlayerPiece = true };
        
        TutorialSequence sequence = ScriptableObject.CreateInstance<TutorialSequence>();
        sequence.tutorialName = "Example Tutorial";
        sequence.steps = new TutorialStep[] { step1, step2 };
        
        string sequencePath = folderPath + "/Tutorial_Example_Sequence.asset";
        AssetDatabase.CreateAsset(sequence, sequencePath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Selection.activeObject = sequence;
        EditorGUIUtility.PingObject(sequence);
        
        EditorUtility.DisplayDialog(
            "Tutorial Example", 
            "Tutorial de exemplo criado com sucesso!\n\n" +
            "Criados:\n" +
            "- Tutorial_Step_01_Welcome\n" +
            "- Tutorial_Step_02_Practice\n" +
            "- Tutorial_Example_Sequence\n\n" +
            "Configure os Dialogues e Piece Prefabs.", 
            "OK");
    }

    private static TutorialStep CreateTutorialStep(string folder, string name)
    {
        TutorialStep step = ScriptableObject.CreateInstance<TutorialStep>();
        string path = folder + "/" + name + ".asset";
        AssetDatabase.CreateAsset(step, path);
        return step;
    }

    private static void CreateFolderIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = Path.GetDirectoryName(path).Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            
            if (!AssetDatabase.IsValidFolder(parentFolder))
            {
                CreateFolderIfNotExists(parentFolder);
            }
            
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }

    private static string GetSelectedFolderPath()
    {
        string path = "Assets";
        
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }
            break;
        }
        
        return path;
    }
}
