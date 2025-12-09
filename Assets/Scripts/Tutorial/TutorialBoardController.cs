using UnityEngine;

public class TutorialBoardController : MonoBehaviour
{
    public BoardController boardController;

    private void Awake()
    {
        if (boardController == null)
        {
            boardController = FindFirstObjectByType<BoardController>();
        }
    }

    public GameField GetField(int index)
    {
        if (boardController == null)
        {
            Debug.LogError("BoardController not found!");
            return null;
        }

        return boardController.GetGameField(index);
    }

    public void ClearAllPieces()
    {
        if (boardController == null) return;

        GameField[] fields = boardController.gameFields;
        foreach (GameField field in fields)
        {
            if (field.hasPiece)
            {
                Piece piece = field.piece;
                if (piece != null)
                {
                    Destroy(piece.gameObject);
                }
            }
        }
    }

    public void HighlightField(int fieldIndex, bool highlight)
    {
        GameField field = GetField(fieldIndex);
        if (field == null) return;

        if (highlight)
        {
            field.Select();
        }
        else
        {
            field.Deselect();
        }
    }

    public BoardController GetBoardController()
    {
        return boardController;
    }
}
