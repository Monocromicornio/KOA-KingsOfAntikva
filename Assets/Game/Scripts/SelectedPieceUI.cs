using TMPro;
using UnityEngine;

/// <summary>
/// Manages the Selected Piece UI panel in the canvas.
/// Shows piece force and description when a piece is selected, hides when deselected.
/// The description follows the current language (see Piece.Description).
/// </summary>
public class SelectedPieceUI : MonoBehaviour, ILocalizedByCode
{
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private Piece displayedPiece;
    private LocalizationManager subscribedLocalizationManager;

    private void Awake()
    {
        AutoFindReferences();
        Piece.OnPieceSelected += OnPieceSelected;
        Piece.OnPieceDeselected += OnPieceDeselected;
        SubscribeToLanguageChanges();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Piece.OnPieceSelected -= OnPieceSelected;
        Piece.OnPieceDeselected -= OnPieceDeselected;
        UnsubscribeFromLanguageChanges();
    }

    private void AutoFindReferences()
    {
        Transform bg = transform.Find("BG");
        if (bg == null) return;

        if (forceText == null)
        {
            Transform forceTransform = bg.Find("Force");
            if (forceTransform != null)
                forceText = forceTransform.GetComponent<TextMeshProUGUI>();
        }

        if (descriptionText == null)
        {
            Transform descTransform = bg.Find("Description");
            if (descTransform != null)
                descriptionText = descTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnPieceSelected(Piece piece)
    {
        displayedPiece = piece;
        SubscribeToLanguageChanges();
        UpdateInfo(piece);
        gameObject.SetActive(true);
    }

    private void OnPieceDeselected()
    {
        displayedPiece = null;
        gameObject.SetActive(false);
    }

    // The panel starts inactive (no coroutines), so the subscription is retried on every selection
    // in case the LocalizationManager did not exist yet during Awake.
    private void SubscribeToLanguageChanges()
    {
        var localizationManager = LocalizationManager.Instance;
        if (localizationManager == null || localizationManager == subscribedLocalizationManager) return;

        UnsubscribeFromLanguageChanges();
        subscribedLocalizationManager = localizationManager;
        subscribedLocalizationManager.OnLanguageChanged += RefreshDisplayedPiece;
    }

    private void UnsubscribeFromLanguageChanges()
    {
        if (subscribedLocalizationManager == null) return;

        subscribedLocalizationManager.OnLanguageChanged -= RefreshDisplayedPiece;
        subscribedLocalizationManager = null;
    }

    private void RefreshDisplayedPiece()
    {
        if (displayedPiece != null && gameObject.activeInHierarchy)
            UpdateInfo(displayedPiece);
    }

    /// <summary>
    /// Updates the UI texts with the selected piece's force and description.
    /// </summary>
    private void UpdateInfo(Piece piece)
    {
        if (piece == null) return;

        InteractivePiece interactive = piece.GetComponent<InteractivePiece>();

        if (forceText != null)
            forceText.text = interactive != null ? interactive.force.ToString() : "-";

        if (descriptionText != null)
            descriptionText.text = !string.IsNullOrEmpty(piece.Description) ? piece.Description : "";
    }
}
