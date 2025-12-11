using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHighlight : MonoBehaviour
{
    public static TutorialHighlight instance;

    [Header("Overlay Settings")]
    [SerializeField] private Canvas highlightCanvas;
    [SerializeField] private Image overlayImage;
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Cutout Panels (4 black panels)")]
    [SerializeField] private RectTransform topPanel;
    [SerializeField] private RectTransform bottomPanel;
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;

    [Header("Highlight Settings")]
    [SerializeField] private Image highlightFrame;
    [SerializeField] private float padding = 20f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.95f, 1, 1.05f);
    [SerializeField] private float pulseSpeed = 1f;

    private RectTransform highlightRect;
    private RectTransform canvasRect;
    private bool isActive;
    private Coroutine pulseCoroutine;
    private Coroutine followCoroutine;
    
    private Vector2 targetSize;
    private Vector2 targetPosition;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        canvasRect = highlightCanvas.GetComponent<RectTransform>();

        if (highlightFrame != null)
        {
            highlightRect = highlightFrame.GetComponent<RectTransform>();
        }

        highlightCanvas.gameObject.SetActive(true);
        HideImmediate();
    }

    public void ShowHighlight(RectTransform target)
    {
        if (target == null) return;

        if (!highlightCanvas.gameObject.activeInHierarchy)
        {
            highlightCanvas.gameObject.SetActive(true);
        }

        isActive = true;

        StartCoroutine(FadeIn());
        PositionCutout(target);

        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseHighlight());
    }

    public void ShowHighlight(Transform worldTarget, Camera camera = null)
    {
        if (worldTarget == null) return;
        if (camera == null) camera = Camera.main;

        if (!highlightCanvas.gameObject.activeInHierarchy)
        {
            highlightCanvas.gameObject.SetActive(true);
        }

        isActive = true;

        StartCoroutine(FadeIn());
        
        if (followCoroutine != null) StopCoroutine(followCoroutine);
        followCoroutine = StartCoroutine(FollowWorldObject(worldTarget, camera));

        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseHighlight());
    }

    public void Hide()
    {
        isActive = false;
        
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
        
        if (highlightCanvas.gameObject.activeInHierarchy)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            HideImmediate();
        }
    }

    public void HideImmediate()
    {
        isActive = false;
        
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
        
        if (overlayImage != null)
        {
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        }

        if (highlightFrame != null)
        {
            highlightFrame.gameObject.SetActive(false);
        }

        HideCutoutPanels();
    }

    private void PositionCutout(RectTransform target)
    {
        Vector2 targetPos = GetCanvasPosition(target);
        Vector2 targetSizeWithPadding = target.rect.size + Vector2.one * padding;

        targetPosition = targetPos;
        targetSize = targetSizeWithPadding;

        UpdateCutoutPanels(targetPos, targetSizeWithPadding);

        if (highlightFrame != null)
        {
            highlightFrame.gameObject.SetActive(true);
            highlightRect.anchoredPosition = targetPos;
            highlightRect.sizeDelta = targetSizeWithPadding;
        }
    }

    private void UpdateCutoutPanels(Vector2 center, Vector2 size)
    {
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        float left = (canvasWidth / 2f) + center.x - halfWidth;
        float right = (canvasWidth / 2f) + center.x + halfWidth;
        float top = (canvasHeight / 2f) + center.y + halfHeight;
        float bottom = (canvasHeight / 2f) + center.y - halfHeight;

        if (topPanel != null)
        {
            topPanel.gameObject.SetActive(true);
            topPanel.anchorMin = Vector2.zero;
            topPanel.anchorMax = Vector2.one;
            topPanel.offsetMin = new Vector2(0, top);
            topPanel.offsetMax = new Vector2(0, 0);
        }

        if (bottomPanel != null)
        {
            bottomPanel.gameObject.SetActive(true);
            bottomPanel.anchorMin = Vector2.zero;
            bottomPanel.anchorMax = Vector2.one;
            bottomPanel.offsetMin = new Vector2(0, 0);
            bottomPanel.offsetMax = new Vector2(0, -(canvasHeight - bottom));
        }

        if (leftPanel != null)
        {
            leftPanel.gameObject.SetActive(true);
            leftPanel.anchorMin = Vector2.zero;
            leftPanel.anchorMax = Vector2.one;
            leftPanel.offsetMin = new Vector2(0, bottom);
            leftPanel.offsetMax = new Vector2(-(canvasWidth - left), -(canvasHeight - top));
        }

        if (rightPanel != null)
        {
            rightPanel.gameObject.SetActive(true);
            rightPanel.anchorMin = Vector2.zero;
            rightPanel.anchorMax = Vector2.one;
            rightPanel.offsetMin = new Vector2(right, bottom);
            rightPanel.offsetMax = new Vector2(0, -(canvasHeight - top));
        }
    }

    private void HideCutoutPanels()
    {
        if (topPanel != null) topPanel.gameObject.SetActive(false);
        if (bottomPanel != null) bottomPanel.gameObject.SetActive(false);
        if (leftPanel != null) leftPanel.gameObject.SetActive(false);
        if (rightPanel != null) rightPanel.gameObject.SetActive(false);
    }

    private Vector2 GetCanvasPosition(RectTransform target)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out localPoint);
        return localPoint;
    }

    private IEnumerator FollowWorldObject(Transform worldTarget, Camera camera)
    {
        while (isActive && worldTarget != null)
        {
            Vector3 screenPos = camera.WorldToScreenPoint(worldTarget.position);

            if (screenPos.z > 0)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out localPoint);

                Renderer renderer = worldTarget.GetComponent<Renderer>();
                Vector2 worldSize = Vector2.one * 100f;

                if (renderer != null)
                {
                    Bounds bounds = renderer.bounds;
                    Vector3 min = camera.WorldToScreenPoint(bounds.min);
                    Vector3 max = camera.WorldToScreenPoint(bounds.max);
                    worldSize = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
                }

                Vector2 sizeWithPadding = worldSize + Vector2.one * padding;

                targetPosition = localPoint;
                targetSize = sizeWithPadding;

                UpdateCutoutPanels(localPoint, sizeWithPadding);

                if (highlightFrame != null)
                {
                    highlightFrame.gameObject.SetActive(true);
                    highlightRect.anchoredPosition = localPoint;
                    highlightRect.sizeDelta = sizeWithPadding;
                }
            }

            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, overlayColor.a, elapsed / fadeDuration);
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
            yield return null;
        }

        overlayImage.color = overlayColor;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color startColor = overlayImage.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0, elapsed / fadeDuration);
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
            yield return null;
        }

        HideCutoutPanels();

        if (highlightFrame != null)
        {
            highlightFrame.gameObject.SetActive(false);
        }
    }

    private IEnumerator PulseHighlight()
    {
        if (highlightRect == null) yield break;

        float time = 0f;
        Vector3 originalScale = Vector3.one;

        while (isActive)
        {
            time += Time.deltaTime * pulseSpeed;
            float scale = scaleCurve.Evaluate(Mathf.PingPong(time, 1f));
            highlightRect.localScale = originalScale * scale;
            yield return null;
        }

        highlightRect.localScale = originalScale;
    }
}
