using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteAlways]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] public string Key;
    [TextArea] [SerializeField] private string _fallback;

    // ───── TMP auto-size options ─────
    [Header("TMP Auto Size")]
    [SerializeField] private bool _enableTmpVisualChanges = false;
    [SerializeField] private bool _enableTmpAutoSize = true;
    [SerializeField] private float _tmpMinSize = 18f;
    [SerializeField] private float _tmpMaxSize = 120f;

    // ───── Horizontal bounds (padding inside the label) ─────
    [Header("Horizontal Bounds (Padding)")]
    [SerializeField] private float _leftPadding = 12f;
    [SerializeField] private float _rightPadding = 12f;

    // ───── Fit to box (play mode) ─────
    [Header("Fit To Box (play mode)")]
    [Tooltip("Keeps the text inside its box in every language: leaves room at the sides, never splits a single word and shrinks the font only when needed.")]
    [SerializeField] private bool _fitToBox = true;
    [Tooltip("Smallest font size, as a fraction of the font size designed in the scene.")]
    [Range(0.3f, 1f)] [SerializeField] private float _fitMinScale = 0.55f;
    [Tooltip("Free space kept at the left and right edges, as a fraction of the box width.")]
    [Range(0f, 0.2f)] [SerializeField] private float _fitSidePadding = 0.07f;

    TMP_Text _tmp;
    Text _uiText;
    Piece _piece;

    TextFitter.Design _design;
    bool _designCaptured;
    string _lastFittedText;

    void OnEnable()
    {
        Cache();
        ApplyBounds();        // apply both in Edit & Play
        ApplyTmpAutoSize();

        if (Application.isPlaying)
            StartCoroutine(EnsureSubscribedThenApply());
        else
            Apply();          // editor fallback preview
    }

    void OnDisable()
    {
        if (!Application.isPlaying) return;
        var mgr = LocalizationManager.Instance;
        if (mgr != null) mgr.OnLanguageChanged -= Apply;
    }

    void OnValidate()
    {
        Cache();
        ApplyBounds();
        ApplyTmpAutoSize();
        if (!Application.isPlaying) Apply();
    }

    void Cache()
    {
        if (_tmp == null) _tmp = GetComponent<TMP_Text>();
        if (_uiText == null) _uiText = GetComponent<Text>();
        if(_piece == null) _piece = GetComponent<Piece>();
    }

    IEnumerator EnsureSubscribedThenApply()
    {
        while (LocalizationManager.Instance == null) yield return null;
        var mgr = LocalizationManager.Instance;
        mgr.OnLanguageChanged -= Apply;
        mgr.OnLanguageChanged += Apply;
        Apply();
    }

    public void SetFallbackForEditor(string text) => _fallback = text;

    public void Apply()
    {
        string value = null;

        if (Application.isPlaying && LocalizationManager.Instance != null && !string.IsNullOrEmpty(Key))
            LocalizationManager.Instance.TryGet(Key, out value);

        var finalText = string.IsNullOrEmpty(value) ? _fallback : value;

        if (_tmp != null)
        {
            ApplyToTmp(finalText, false);
        }
        else if (_uiText != null)
        {
            _uiText.text = finalText;
        }
        else if (_piece != null)
        {
            _piece.SetDescription(finalText);
        }
    }

    /// <summary>
    /// Shows the given text exactly as play mode would, including the fit to the box. For editor tools and tests:
    /// outside play mode the label itself only shows its fallback text, unfitted.
    /// </summary>
    public void ApplyTextForPreview(string text)
    {
        Cache();
        if (_tmp != null) ApplyToTmp(text, true);
    }

    void ApplyToTmp(string text, bool preview)
    {
        // The manual "TMP Auto Size" options above take over the label completely; otherwise it is fitted to its box.
        bool fit = _fitToBox && !_enableTmpVisualChanges && (Application.isPlaying || preview);
        if (fit)
        {
            if (!_designCaptured)
            {
                _design = TextFitter.Capture(_tmp);
                _designCaptured = true;
            }
            _lastFittedText = text;
            TextFitter.Fit(_tmp, text, _design, _fitMinScale, _fitSidePadding);
        }
        else
        {
            _tmp.text = text;
            ApplyBounds();
            ApplyTmpAutoSize();
        }
        _tmp.ForceMeshUpdate(true);
    }

    // The box can get its size after the first Apply (layout groups, canvas scaling) or change later: fit again.
    void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying || !_designCaptured || _tmp == null || _lastFittedText == null || !isActiveAndEnabled) return;
        TextFitter.Fit(_tmp, _lastFittedText, _design, _fitMinScale, _fitSidePadding);
    }

    // ─────────────────────────────────
    void ApplyTmpAutoSize()
    {
        if (_enableTmpVisualChanges == false) return;

        if (_tmp == null) return;

        _tmp.enableAutoSizing = _enableTmpAutoSize;
        if (_enableTmpAutoSize)
        {
            if (_tmpMinSize <= 0f) _tmpMinSize = 1f;
            if (_tmpMaxSize < _tmpMinSize) _tmpMaxSize = _tmpMinSize;

            _tmp.fontSizeMin = _tmpMinSize;
            _tmp.fontSizeMax = _tmpMaxSize;
        }

        // sensible defaults for fitting inside a button
        _tmp.enableWordWrapping = true;
        if (_tmp.overflowMode == TextOverflowModes.Truncate || _tmp.overflowMode == TextOverflowModes.Overflow)
            _tmp.overflowMode = TextOverflowModes.Ellipsis; // optional; keeps it tidy if still too long
    }

    void ApplyBounds()
    {
        if (_enableTmpVisualChanges == false) return;

        if (_tmp == null) return;

        // TMP uses a Vector4 margin: (left, top, right, bottom)
        var m = _tmp.margin;
        m.x = _leftPadding;
        m.z = _rightPadding;
        _tmp.margin = m;
    }
}
