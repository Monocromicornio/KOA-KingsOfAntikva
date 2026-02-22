using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenUIHelper : MonoBehaviour
{
    [MenuItem("GameObject/UI Loading/Loading Screen System", false, 10)]
    static void CreateLoadingScreenSystem(MenuCommand menuCommand)
    {
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null && existingCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning("É recomendado criar o Loading Screen em um Canvas separado com Screen Space - Overlay");
        }

        GameObject loadingManager = new GameObject("LoadingScreenManager");
        LoadingScreenManager manager = loadingManager.AddComponent<LoadingScreenManager>();
        
        GameObject canvas = new GameObject("LoadingCanvas");
        canvas.transform.SetParent(loadingManager.transform);
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.sortingOrder = 9999;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("LoadingPanel");
        panel.transform.SetParent(canvas.transform);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f);

        GameObject loadingText = new GameObject("LoadingText");
        loadingText.transform.SetParent(panel.transform);
        RectTransform loadingTextRect = loadingText.AddComponent<RectTransform>();
        loadingTextRect.anchoredPosition = new Vector2(0, -100);
        loadingTextRect.sizeDelta = new Vector2(600, 60);
        
        TextMeshProUGUI loadingTMP = loadingText.AddComponent<TextMeshProUGUI>();
        loadingTMP.text = "Carregando...";
        loadingTMP.fontSize = 36;
        loadingTMP.alignment = TextAlignmentOptions.Center;
        loadingTMP.color = Color.white;

        GameObject barBackground = new GameObject("LoadingBarBackground");
        barBackground.transform.SetParent(panel.transform);
        RectTransform barBackRect = barBackground.AddComponent<RectTransform>();
        barBackRect.anchoredPosition = new Vector2(0, -200);
        barBackRect.sizeDelta = new Vector2(600, 30);
        
        Image barBackImage = barBackground.AddComponent<Image>();
        barBackImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject barFill = new GameObject("LoadingBarFill");
        barFill.transform.SetParent(barBackground.transform);
        RectTransform barFillRect = barFill.AddComponent<RectTransform>();
        barFillRect.anchorMin = new Vector2(0, 0);
        barFillRect.anchorMax = new Vector2(1, 1);
        barFillRect.sizeDelta = Vector2.zero;
        barFillRect.anchoredPosition = Vector2.zero;
        
        Image barFillImage = barFill.AddComponent<Image>();
        barFillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        barFillImage.type = Image.Type.Filled;
        barFillImage.fillMethod = Image.FillMethod.Horizontal;
        barFillImage.fillOrigin = 0;
        barFillImage.fillAmount = 0f;

        GameObject statusText = new GameObject("StatusText");
        statusText.transform.SetParent(panel.transform);
        RectTransform statusTextRect = statusText.AddComponent<RectTransform>();
        statusTextRect.anchoredPosition = new Vector2(0, -260);
        statusTextRect.sizeDelta = new Vector2(600, 40);
        
        TextMeshProUGUI statusTMP = statusText.AddComponent<TextMeshProUGUI>();
        statusTMP.text = "";
        statusTMP.fontSize = 24;
        statusTMP.alignment = TextAlignmentOptions.Center;
        statusTMP.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        typeof(LoadingScreenManager)
            .GetField("loadingScreenPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, panel);
        
        typeof(LoadingScreenManager)
            .GetField("loadingBarFill", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, barFillImage);
        
        typeof(LoadingScreenManager)
            .GetField("loadingText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, loadingTMP);
        
        typeof(LoadingScreenManager)
            .GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, statusTMP);

        panel.SetActive(false);

        Selection.activeGameObject = loadingManager;
        
        Debug.Log("[LoadingScreenUIHelper] Loading Screen System criado com sucesso! Configure as mensagens no Inspector.");

        GameObject handlerObj = new GameObject("SceneLoadingHandler");
        handlerObj.AddComponent<SceneLoadingHandler>();
        handlerObj.transform.SetParent(loadingManager.transform);
        
        Debug.Log("[LoadingScreenUIHelper] SceneLoadingHandler também foi criado.");
    }
}
#endif
