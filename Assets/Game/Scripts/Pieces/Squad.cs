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

    void Start()
    {
        LoadPieces();
    }

    protected virtual void LoadPieces()
    {
        // Implementação padrão (vazia)
    }

    
    protected Piece GetPieceByName(string pieaceName){
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
}