using TMPro;
using UnityEngine;

/// <summary>
/// Manages the Selected Piece UI panel in the canvas.
/// Shows piece force and description when a piece is selected, hides when deselected.
/// </summary>
public class SelectedPieceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private void Awake()
    {
        AutoFindReferences();
        Piece.OnPieceSelected += OnPieceSelected;
        Piece.OnPieceDeselected += OnPieceDeselected;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Piece.OnPieceSelected -= OnPieceSelected;
        Piece.OnPieceDeselected -= OnPieceDeselected;
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
        UpdateInfo(piece);
        gameObject.SetActive(true);
    }

    private void OnPieceDeselected()
    {
        gameObject.SetActive(false);
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
