using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimapCell : MonoBehaviour
{
    [SerializeField] private Image cellImage;
    [SerializeField] private TextMeshProUGUI cellText;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    [SerializeField] private Color bluePieceColor = new Color(0.3f, 0.5f, 1f, 0.8f);
    [SerializeField] private Color redPieceColor = new Color(1f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color[] markingColors = new Color[]
    {
        new Color(1f, 1f, 0f, 0.6f),
        new Color(1f, 0.5f, 0f, 0.6f),
        new Color(0.5f, 0f, 1f, 0.6f),
        new Color(0f, 1f, 0.5f, 0.6f)
    };

    private int cellIndex;
    private Piece currentPiece;
    private string currentMarking;

    private void Awake()
    {
        if (cellImage == null)
            cellImage = GetComponent<Image>();
        
        if (cellText == null)
            cellText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize(int index)
    {
        cellIndex = index;
        
        if (cellImage == null)
            cellImage = GetComponent<Image>();
        
        if (cellText == null)
            cellText = GetComponentInChildren<TextMeshProUGUI>();
        
        Clear();
    }

    public void UpdateCell(Piece piece)
    {
        currentPiece = piece;

        if (piece == null)
        {
            Clear();
            return;
        }

        if (cellImage == null || cellText == null)
            return;

        if (piece.pieceColor == PieceColor.blue)
        {
            cellImage.color = bluePieceColor;
            cellText.text = GetPieceValue(piece.type).ToString();
        }
        else if (piece.pieceColor == PieceColor.red)
        {
            cellImage.color = redPieceColor;
            cellText.text = "?";
        }
        else
        {
            Clear();
        }
    }

    /// <summary>
    /// Applies a marking to this cell, returning the resulting marking string.
    /// Toggling the same marking twice removes it. Returns null when cleared.
    /// </summary>
    public string SetMarking(string marking)
    {
        if (currentPiece == null || currentPiece.pieceColor != PieceColor.red)
            return currentMarking;

        if (cellImage == null || cellText == null)
            return currentMarking;

        if (currentMarking == marking)
        {
            currentMarking = null;
            cellImage.color = redPieceColor;
        }
        else
        {
            currentMarking = marking;
            cellImage.color = GetMarkingColor(marking);
        }
        
        cellText.text = currentMarking ?? "?";
        return currentMarking;
    }

    /// <summary>
    /// Restores a previously stored marking onto this cell without the toggle behaviour.
    /// Called when a piece moves so its marking follows it to the new cell.
    /// </summary>
    public void RestoreMarking(string marking)
    {
        if (currentPiece == null || currentPiece.pieceColor != PieceColor.red)
            return;

        if (cellImage == null || cellText == null)
            return;

        currentMarking = marking;
        cellImage.color = string.IsNullOrEmpty(marking) ? redPieceColor : GetMarkingColor(marking);
        cellText.text = string.IsNullOrEmpty(marking) ? "?" : marking;
    }

    public void Clear()
    {
        currentPiece = null;
        currentMarking = null;
        
        if (cellImage != null)
            cellImage.color = emptyColor;
        
        if (cellText != null)
            cellText.text = "";
    }

    public bool HasMarking()
    {
        return !string.IsNullOrEmpty(currentMarking);
    }

    public void ClearMarking()
    {
        if (currentMarking != null)
        {
            currentMarking = null;
            if (currentPiece != null && currentPiece.pieceColor == PieceColor.red)
            {
                if (cellImage != null)
                    cellImage.color = redPieceColor;
                if (cellText != null)
                    cellText.text = "?";
            }
        }
    }

    private int GetPieceValue(PieceType type)
    {
        switch (type)
        {
            case PieceType.Soldier: return 1;
            case PieceType.Antibomb: return 2;
            case PieceType.Sergeant: return 3;
            case PieceType.Lieutenant: return 4;
            case PieceType.Captain: return 5;
            case PieceType.Major: return 6;
            case PieceType.Colonel: return 7;
            case PieceType.General: return 8;
            case PieceType.Minister: return 9;
            case PieceType.Spy: return 1;
            case PieceType.Flag: return 0;
            case PieceType.Bomb: return 0;
            default: return 0;
        }
    }

    private Color GetMarkingColor(string marking)
    {
        int hash = marking.GetHashCode();
        int index = Mathf.Abs(hash) % markingColors.Length;
        return markingColors[index];
    }
}
