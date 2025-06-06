using System.Collections.Generic;
using UnityEngine;

public class Squad : MonoBehaviour
{
    [SerializeField]
    protected TableData table;
    protected BoardController board => BoardController.instance;
    protected Field[] fields => board.fields.ToArray();
    protected EditableField[] editables => board.editableFields;
    protected GameField[] gameFields => board.gameFields;

    [SerializeField]
    protected PieceData pieceData;
    protected List<Piece> _pieces;
    public List<Piece> pieces
    {
        get
        {
            _pieces ??= new List<Piece>();
            return _pieces;
        }
    }

    public virtual void LoadPieces()
    {
        // Implementação padrão (vazia)
    }

    protected Piece GetPieceByName(string pieaceName, Piece[] defaultPieces)
    {
        Piece piece = System.Array.Find(
            defaultPieces,
            p => p.name == pieaceName
        );

        if (piece == null)
        {
            Debug.LogWarning($"No default piece found with the name {pieaceName}");
        }

        return piece;
    }

    protected Piece GetPieceByName(string pieaceName)
    {
        string pName = pieaceName.Substring(0, 2);
        var pieceConfig = pieceData.pieces.Find((p) => p.name == pName);
        Piece piece = pieceConfig.prefab;

        if (piece == null)
        {
            Debug.LogWarning($"No default piece found with the name {pieaceName}");
        }

        return piece;
    }

    protected void LinkPieceToGameField(Piece piece, GameField gameField)
    {
        gameField.SetPiece(piece);
        piece.SetFirstField(gameField);
        pieces.Add(piece);
    }
}