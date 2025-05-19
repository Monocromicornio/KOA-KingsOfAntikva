using UnityEngine;

public class Field: MonoBehaviour
{
    public int index { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public string ColumnName { get; set; }
    public string NickName { get; set; }

    public bool isEditableField{
        get {
            return this is EditableField;
        }
    }

    public bool isGameField{
        get {
            return this is GameField;
        }
    }

    /// <summary>
    /// Configures the board fields.
    /// </summary>
    /// <param name="field">The field component (FieldController or HousePicker).</param>
    /// <param name="index">The field index.</param>
    /// <param name="fieldIndex">The column index.</param>
    /// <param name="rowIndex">The row index.</param>
    public void Configure(int index, int fieldIndex, int rowIndex)
    {
        string [] alphabet = AlphabetHelper.GetAlphabet();
        int iColumn = fieldIndex + 1;
        int iRow = rowIndex + 1;

        this.index = index;
        Row = iRow;
        Column = iColumn;
        ColumnName = alphabet[fieldIndex];
        NickName = alphabet[fieldIndex] + iColumn.ToString();
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
}