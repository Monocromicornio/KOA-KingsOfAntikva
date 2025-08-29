using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class ForceText : MonoBehaviour
{
    private TextMesh textMesh;

    [SerializeField]
    private InteractivePiece startPiece;
    public InteractivePiece piece
    {
        set
        {
            startPiece ??= value;
            txtForce = GetTextByPieceType(value);

            textMesh ??= GetComponent<TextMesh>();
            if (textMesh == null) return;
            textMesh.text = txtForce;
        }
    }
    private string txtForce;

    private void Start()
    {
        piece = startPiece;
    }

    private string GetTextByPieceType(InteractivePiece piece)
    {
        if (piece == null || piece.piece.pieceColor == PieceColor.red) return "";
        
        PieceType pieceType = piece.piece.type;
        if (pieceType == PieceType.Bomb
        ||  pieceType == PieceType.Flag)
        {
            return "";
        }

        if (pieceType == PieceType.Spy) return "S";
        
        return piece.force.ToString();
    }
}
