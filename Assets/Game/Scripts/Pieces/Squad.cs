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
    protected Piece[] defaultPieces;
    public List<Piece> pieces { get; private set; }

    protected virtual void Awake()
    {
        pieces = new List<Piece>();
    }

    private void Start()
    {
        LoadPieces();
    }

    protected virtual void LoadPieces()
    {
        // Implementação padrão (vazia)
    }

    protected Piece GetPieceByName(string pieaceName)
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

    protected void LinkPieceToGameField(Piece piece, GameField gameField)
    {
        gameField.SetPiece(piece);
        piece.SetFirstField(gameField);
        pieces.Add(piece);
    }
}