using UnityEngine;

public static class TutorialEvents
{
    public delegate void PieceMovedHandler(MonoBehaviour piece, GameField fromField, GameField toField);
    public static event PieceMovedHandler OnPieceMoved;

    public delegate void PieceAttackedHandler(MonoBehaviour attacker, MonoBehaviour target);
    public static event PieceAttackedHandler OnPieceAttacked;

    public delegate void PieceSelectedHandler(MonoBehaviour piece);
    public static event PieceSelectedHandler OnPieceSelected;

    public static void TriggerPieceMoved(Piece piece, GameField fromField, GameField toField)
    {
        OnPieceMoved?.Invoke(piece, fromField, toField);
    }

    public static void TriggerPieceMoved(OfflinePiece piece, GameField fromField, GameField toField)
    {
        OnPieceMoved?.Invoke(piece, fromField, toField);
    }

    public static void TriggerPieceAttacked(Piece attacker, Piece target)
    {
        OnPieceAttacked?.Invoke(attacker, target);
    }

    public static void TriggerPieceAttacked(OfflinePiece attacker, OfflinePiece target)
    {
        OnPieceAttacked?.Invoke(attacker, target);
    }

    public static void TriggerPieceSelected(Piece piece)
    {
        OnPieceSelected?.Invoke(piece);
    }

    public static void TriggerPieceSelected(OfflinePiece piece)
    {
        OnPieceSelected?.Invoke(piece);
    }
}
