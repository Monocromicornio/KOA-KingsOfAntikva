using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField]
    private GameObject loadingScreenPanel;

    [SerializeField]
    private Image loadingBarFill;

    [SerializeField]
    private TextMeshProUGUI loadingText;

    [SerializeField]
    private TextMeshProUGUI statusText;

    [Header("Loading Messages")]
    [SerializeField]
    private string[] loadingMessages = new string[]
    {
        "Carregando...",
        "Conectando com oponente...",
        "Sincronizando dados...",
        "Preparando partida...",
        "Aguardando oponente..."
    };

    [Header("Settings")]
    [SerializeField]
    private float messageChangeInterval = 2f;

    [SerializeField]
    private float minDisplayTime = 1f;

    private bool isShowing = false;
    private float currentProgress = 0f;
    private Coroutine loadingCoroutine;
    private Coroutine messageCoroutine;
    private float showStartTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(false);
        }
    }

    public void Show(string customMessage = null)
    {
        if (isShowing) return;

        isShowing = true;
        showStartTime = Time.time;
        currentProgress = 0f;

        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(true);
        }

        if (loadingText != null)
        {
            loadingText.text = customMessage ?? loadingMessages[0];
        }

        if (statusText != null)
        {
            statusText.text = "";
        }

        UpdateLoadingBar(0f);

        if (customMessage == null)
        {
            messageCoroutine = StartCoroutine(CycleLoadingMessages());
        }

        Debug.Log("[LoadingScreen] Tela de carregamento exibida");
    }

    public void Hide()
    {
        StartCoroutine(HideAfterMinTime());
    }

    private IEnumerator HideAfterMinTime()
    {
        float elapsedTime = Time.time - showStartTime;
        float remainingTime = minDisplayTime - elapsedTime;

        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        isShowing = false;

        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(false);
        }

        Debug.Log("[LoadingScreen] Tela de carregamento ocultada");
    }

    public void UpdateProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        UpdateLoadingBar(currentProgress);
    }

    public void SetStatusText(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
    }

    public void SetLoadingText(string text)
    {
        if (loadingText != null)
        {
            loadingText.text = text;
        }
    }

    private void UpdateLoadingBar(float fillAmount)
    {
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = fillAmount;
        }
    }

    private IEnumerator CycleLoadingMessages()
    {
        int messageIndex = 0;

        while (isShowing)
        {
            if (loadingText != null)
            {
                loadingText.text = loadingMessages[messageIndex];
            }

            messageIndex = (messageIndex + 1) % loadingMessages.Length;

            yield return new WaitForSeconds(messageChangeInterval);
        }
    }

    public void SimulateProgress(float duration)
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }

        loadingCoroutine = StartCoroutine(SimulateProgressCoroutine(duration));
    }

    private IEnumerator SimulateProgressCoroutine(float duration)
    {
        float elapsed = 0f;
        float startProgress = currentProgress;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Lerp(startProgress, 0.95f, elapsed / duration);
            UpdateProgress(progress);
            yield return null;
        }
    }

    public bool IsShowing()
    {
        return isShowing;
    }
}
