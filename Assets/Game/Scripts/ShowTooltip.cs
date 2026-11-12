using UnityEngine;
using TMPro;

/// <summary>
/// Shows the piece tooltip: the piece name, its description and its force, taken from the Piece (in the current language).
/// The tooltip must contain TextMeshProUGUI children named "Name", "Description" and "Force" (at any depth).
/// </summary>
public class ShowTooltip : MonoBehaviour, ILocalizedByCode
{
    [SerializeField] Transform tooltip;

    private Piece piece;
    private InteractivePiece interactivePiece;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI forceText;

    void Awake()
    {
        tooltip = transform.Find("Tooltip");
        if (tooltip != null)
        {
            tooltip.gameObject.SetActive(false);

            nameText = FindText(tooltip, "Name");
            descriptionText = FindText(tooltip, "Description");
            forceText = FindText(tooltip, "Force");
        }

        piece = GetComponent<Piece>();
        if (piece == null)
            piece = GetComponentInParent<Piece>();

        if (piece != null)
            interactivePiece = piece.GetComponent<InteractivePiece>();
    }

    private static TextMeshProUGUI FindText(Transform root, string objectName)
    {
        foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == objectName) return text;
        }
        return null;
    }

    public void Show()
    {
        if (tooltip != null)
        {
            UpdateTexts();
            UpdateForce();
            tooltip.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (tooltip != null) tooltip.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the tooltip name and description from the piece, in the current language.
    /// Only overwrites a text when the piece has a non-empty value for it.
    /// </summary>
    private void UpdateTexts()
    {
        if (piece == null) return;

        if (nameText != null && !string.IsNullOrEmpty(piece.DisplayName))
            nameText.text = piece.DisplayName;

        if (descriptionText != null && !string.IsNullOrEmpty(piece.Description))
            descriptionText.text = piece.Description;
    }

    /// <summary>
    /// Updates the tooltip force text from the piece's InteractivePiece force value.
    /// </summary>
    private void UpdateForce()
    {
        if (forceText == null) return;

        forceText.text = interactivePiece != null ? interactivePiece.force.ToString() : "-";
    }
}
