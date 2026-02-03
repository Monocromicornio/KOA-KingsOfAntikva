using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FirstTimePlayerManager : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("Se ativado, sempre dispara o diálogo independente de ser primeira vez")]
    public bool alwaysTriggerDialogueForTest = false;

    [Header("Dialogue Configuration")]
    [Tooltip("O diálogo que será disparado na primeira vez")]
    public DialogueBase firstTimeDialogue;

    [Tooltip("Tempo de delay antes de iniciar o diálogo")]
    public float dialogueDelay = 0.5f;

    [Header("Tutorial Button")]
    [Tooltip("Botão que carregará a cena de tutorial")]
    public Button tutorialButton;

    [Tooltip("Nome da cena de tutorial a ser carregada")]
    public string tutorialSceneName = "TutorialScene";

    private const string FIRST_TIME_PLAYERPREFS_KEY = "HasPlayedBefore";
    private bool isFirstTime = false;
    private bool dataChecked = false;

    private void Start()
    {
        SetupTutorialButton();
        StartCoroutine(CheckFirstTimeAndTriggerDialogue());
    }

    private void SetupTutorialButton()
    {
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(LoadTutorialScene);
            Debug.Log("[FirstTimePlayerManager] Listener adicionado ao botão de tutorial");
        }
        else
        {
            Debug.LogWarning("[FirstTimePlayerManager] Botão de tutorial não está atribuído!");
        }
    }

    private void LoadTutorialScene()
    {
        Debug.Log($"[FirstTimePlayerManager] Carregando cena de tutorial: {tutorialSceneName}");
        SceneManager.LoadScene(tutorialSceneName);
    }

    private IEnumerator CheckFirstTimeAndTriggerDialogue()
    {
        if (alwaysTriggerDialogueForTest)
        {
            Debug.Log("[FirstTimePlayerManager] Modo de teste ativado - disparando diálogo sempre");
            yield return new WaitForSeconds(dialogueDelay);
            TriggerFirstTimeDialogue();
            yield break;
        }

        bool hasPlayedBeforeInPrefs = PlayerPrefs.GetInt(FIRST_TIME_PLAYERPREFS_KEY, 0) == 1;

        if (hasPlayedBeforeInPrefs)
        {
            Debug.Log("[FirstTimePlayerManager] Jogador já jogou antes (PlayerPrefs)");
            dataChecked = true;
            yield break;
        }

        if (PlayerProfileManager.Instance == null)
        {
            Debug.LogWarning("[FirstTimePlayerManager] PlayerProfileManager não encontrado, usando apenas PlayerPrefs");
            isFirstTime = true;
        }
        else
        {
            yield return new WaitUntil(() => PlayerProfileManager.Instance.pontuation >= 0);

            if (PlayerProfileManager.Instance.pontuation == 0)
            {
                Debug.Log("[FirstTimePlayerManager] Pontuação do banco é 0 - primeira vez confirmada");
                isFirstTime = true;
            }
            else
            {
                Debug.Log($"[FirstTimePlayerManager] Jogador já possui pontuação: {PlayerProfileManager.Instance.pontuation}");
                isFirstTime = false;
            }
        }

        dataChecked = true;

        if (isFirstTime)
        {
            PlayerPrefs.SetInt(FIRST_TIME_PLAYERPREFS_KEY, 1);
            PlayerPrefs.Save();
            Debug.Log("[FirstTimePlayerManager] Marcado como 'já jogou' no PlayerPrefs");

            yield return new WaitForSeconds(dialogueDelay);
            TriggerFirstTimeDialogue();
        }
    }

    private void TriggerFirstTimeDialogue()
    {
        if (firstTimeDialogue == null)
        {
            Debug.LogWarning("[FirstTimePlayerManager] Nenhum DialogueBase foi atribuído para primeira vez!");
            return;
        }

        if (DialogueManager.instance == null)
        {
            Debug.LogError("[FirstTimePlayerManager] DialogueManager.instance não encontrado na cena!");
            return;
        }

        Debug.Log($"[FirstTimePlayerManager] Disparando diálogo de primeira vez: {firstTimeDialogue.name}");
        DialogueManager.instance.SetDialogueClosable(false);
        DialogueManager.instance.EnqueueDialogue(firstTimeDialogue);
    }

    public bool IsFirstTime()
    {
        return isFirstTime && dataChecked;
    }

    public void ResetFirstTimeFlag()
    {
        PlayerPrefs.DeleteKey(FIRST_TIME_PLAYERPREFS_KEY);
        PlayerPrefs.Save();
        Debug.Log("[FirstTimePlayerManager] Flag de primeira vez foi resetada");
    }
}
