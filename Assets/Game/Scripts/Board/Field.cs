using UnityEngine;

public class Field : MonoBehaviour
{
    public int index { get; set; }
    public int row { get; set; }
    public int column { get; set; }
    public string ColumnName { get; set; }
    public string NickName { get; set; }

    public bool isEditableField
    {
        get
        {
            return this is EditableField;
        }
    }

    public bool isGameField
    {
        get
        {
            return this is GameField;
        }
    }

    public bool hasPiece => piece != null || offlinePiece != null;
    public Piece piece { get; protected set; }
    public OfflinePiece offlinePiece { get; protected set; }

    [SerializeField]
    protected ForceText forceText;

    public void ClearPiece()
    {
        piece = null;
        offlinePiece = null;
        if (forceText != null)
        {
            forceText.piece = null;
        }
    }

    /// <summary>
    /// Configures the board fields.
    /// </summary>
    /// <param name="field">The field component (FieldController or HousePicker).</param>
    /// <param name="index">The field index.</param>
    /// <param name="column">The column index.</param>
    /// <param name="row">The row index.</param>
    public void Configure(int index, int column, int row)
    {
        string[] alphabet = AlphabetHelper.GetAlphabet();

        this.index = index;
        this.row = row + 1;
        this.column = column + 1;

        ColumnName = alphabet[column];
        NickName = alphabet[column] + this.column.ToString();
    }

    public Component GetFieldType()
    {
        if (isEditableField)
        {
            return GetComponent<EditableField>();
        }
        else if (isGameField)
        {
            return GetComponent<GameField>();
        }
        else
        {
            return null; // Retorna null se não for nenhum dos dois tipos.
        }
    }

    public virtual void SetPiece(Piece newPiece)
    {
        if (newPiece == null && this.piece == null)
        {
            return;
        }

        this.piece = newPiece;
        this.offlinePiece = null;

        if (forceText == null) return;

        if (newPiece == null)
        {
            forceText.piece = null;
            return;
        }

        forceText.piece = newPiece.GetComponent<InteractivePiece>();
    }

    public virtual void SetOfflinePiece(OfflinePiece newPiece)
    {
        if (newPiece == null && this.offlinePiece == null)
        {
            return;
        }

        this.offlinePiece = newPiece;
        this.piece = null;

        if (forceText == null) return;

        if (newPiece == null)
        {
            forceText.offlinePiece = null;
            return;
        }

        forceText.offlinePiece = newPiece.GetComponent<OfflineInteractivePiece>();
    }
}