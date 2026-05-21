using UnityEngine;
using TMPro;

public class ShowTooltip : MonoBehaviour
{
    [SerializeField] Transform tooltip;

    private Piece piece;
    private InteractivePiece interactivePiece;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI forceText;

    void Awake()
    {
        tooltip = transform.Find("Tooltip");
        if (tooltip != null)
        {
            tooltip.gameObject.SetActive(false);

            Transform descTransform = tooltip.Find("Description");
            if (descTransform != null)
                descriptionText = descTransform.GetComponent<TextMeshProUGUI>();

            Transform forceTransform = tooltip.Find("Force");
            if (forceTransform != null)
                forceText = forceTransform.GetComponent<TextMeshProUGUI>();
        }

        piece = GetComponent<Piece>();
        if (piece == null)
            piece = GetComponentInParent<Piece>();

        if (piece != null)
            interactivePiece = piece.GetComponent<InteractivePiece>();
    }

    public void Show()
    {
        if (tooltip != null)
        {
            UpdateDescription();
            UpdateForce();
            tooltip.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (tooltip != null) tooltip.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the tooltip description text from the piece's description field.
    /// Only overwrites if the piece has a non-empty description.
    /// </summary>
    private void UpdateDescription()
    {
        if (descriptionText == null || piece == null) return;
        if (string.IsNullOrEmpty(piece.Description)) return;

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
