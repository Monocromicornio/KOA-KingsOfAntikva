using UnityEngine;

public class Field: MonoBehaviour
{
    public int index { get; set; }
    public int row { get; set; }
    public int column { get; set; }
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
    /// <param name="column">The column index.</param>
    /// <param name="row">The row index.</param>
    public void Configure(int index, int column, int row)
    {
        string [] alphabet = AlphabetHelper.GetAlphabet();

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
}