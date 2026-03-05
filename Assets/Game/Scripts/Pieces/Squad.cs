using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Squad : MonoBehaviour
{
    [SerializeField]
    protected TableData table;
    protected BoardController board => BoardController.instance;
    protected Field[] fields => board.fields.ToArray();
    protected EditableField[] editables => board.editableFields;
    protected GameField[] gameFields => board.gameFields;

    public PieceData pieceData;
    protected List<Piece> _pieces;
    public List<Piece> pieces
    {
        get
        {
            _pieces ??= new List<Piece>();
            return _pieces;
        }
    }

    /// <summary>
    /// Loads and instantiates all pieces for this squad. Returns a Task that completes when all pieces are ready.
    /// </summary>
    public virtual Task LoadPieces()
    {
        return Task.CompletedTask;
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
}