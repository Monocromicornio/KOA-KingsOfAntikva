using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OfflinePiece))]
public class OfflineSelectablePiece : MonoBehaviour
{
    private enum Axis
    {
        columnPositive,
        columnNegative,
        rowPositive,
        rowNegative,
    }

    public OfflinePiece piece { get; private set; }
    private int currentField => piece.indexCurrentField;
    
    private BoardController board;
    private GameField[] gameFields => board != null ? board.gameFields : null;

    public Dictionary<string, List<GameField>> selectedFields { get; private set; }
    private bool getted;

    [SerializeField]
    [Min(1)]
    private int distance = 1;

    private void Awake()
    {
        selectedFields = new Dictionary<string, List<GameField>>();
        piece = GetComponent<OfflinePiece>();
    }

    private void Start()
    {
        TutorialBoardController tutorialBoard = FindFirstObjectByType<TutorialBoardController>();
        if (tutorialBoard != null)
        {
            board = tutorialBoard.GetBoardController();
        }
        
        if (board == null)
        {
            board = FindFirstObjectByType<BoardController>();
        }
    }

    public void GetPiece()
    {
        getted = !getted;

        if (getted) Select();
        else Deselect();
    }

    public void EndTurn()
    {
        Deselect();
    }

    private void Select()
    {
        getted = true;
        ActiveSelectablesFields();

        if (selectedFields.Count == 0) return;
    }

    private void Deselect()
    {
        getted = false;
        foreach (List<GameField> fields in selectedFields.Values)
        {
            foreach (GameField gameField in fields)
            {
                gameField.Deselect();
            }
        }

        selectedFields.Clear();
    }

    private void ActiveSelectablesFields()
    {
        SelectFieldsInSameAxis(Axis.columnPositive);
        SelectFieldsInSameAxis(Axis.columnNegative);
        SelectFieldsInSameAxis(Axis.rowPositive);
        SelectFieldsInSameAxis(Axis.rowNegative);
    }

    public GameField[][] GetSelectablesFields()
    {
        GameField[][] gameFields = new GameField[4][];
        gameFields[0] = GetFieldsInSameAxis(Axis.columnPositive).ToArray();
        gameFields[1] = GetFieldsInSameAxis(Axis.columnNegative).ToArray();
        gameFields[2] = GetFieldsInSameAxis(Axis.rowPositive).ToArray();
        gameFields[3] = GetFieldsInSameAxis(Axis.rowNegative).ToArray();

        return gameFields;
    }

    private void AddToSelectFields(GameField gameField, string key)
    {
        if (gameField == null) return;

        if (!selectedFields.ContainsKey(key))
        {
            selectedFields.Add(key, new List<GameField>());
        }
        selectedFields[key].Add(gameField);
    }

    private GameField GetField(int target)
    {
        if (gameFields == null || target < 0 || target >= gameFields.Length) return null;

        return gameFields[target];
    }

    public GameField GetEmptyFieldFromActive(GameField gameField)
    {
        if (gameField == null || !gameField.hasPiece) return gameField;
        
        foreach (List<GameField> fields in selectedFields.Values)
        {
            if (fields.Contains(gameField))
            {
                int index = fields.IndexOf(gameField) - 1;
                if (index >= 0) return fields[index];
            }
        }

        return null;
    }

    private void SelectFieldsInSameAxis(Axis axis)
    {
        List<GameField> gameFields = GetFieldsInSameAxis(axis);
        foreach (GameField field in gameFields)
        {
            field.Select();
        }
    }

    private List<GameField> GetFieldsInSameAxis(Axis axis)
    {
        List<GameField> fields = new List<GameField>();
        
        if (board == null || gameFields == null) return fields;
        
        int interval;
        string key;

        switch (axis)
        {
            case Axis.columnPositive:
                key = "column_up";
                interval = board.ColumnLength();
                break;
            case Axis.columnNegative:
                key = "column_down";
                interval = board.ColumnLength() * -1;
                break;
            case Axis.rowPositive:
                key = "row_right";
                interval = 1;
                break;
            case Axis.rowNegative:
                key = "row_left";
                interval = -1;
                break;
            default:
                key = "";
                interval = 0;
                break;
        }

        for (int i = 1; i <= distance; i++)
        {
            int target = currentField + (interval * i);

            GameField field = GetFieldInSameAxis(axis, currentField, target);
            if (field == null) break;

            bool? isSameSquad = null;

            if (field.hasPiece)
            {
                PieceColor targetColor = field.piece != null ? field.piece.pieceColor : field.offlinePiece.pieceColor;
                isSameSquad = targetColor == piece.pieceColor;
            }

            if (isSameSquad == true) break;
            fields.Add(field);
            AddToSelectFields(field, key);
            if (isSameSquad == false) break;
        }

        return fields;
    }

    private GameField GetFieldInSameAxis(Axis axis, int current, int target)
    {
        if (gameFields == null) return null;
        
        if (axis == Axis.columnPositive || axis == Axis.columnNegative)
        {
            if (!OnTheSameColumn(current, target)) return null;
            return GetField(target);
        }

        if (!OnTheSameRow(current, target)) return null;
        return GetField(target);
    }

    private bool OnTheSameColumn(int current, int target)
    {
        if (!IsItAGameFieldIndex(new[] { current, target })) return false;

        string currentColumn = gameFields[current].ColumnName;
        string targetColumn = gameFields[target].ColumnName;

        return currentColumn == targetColumn;
    }

    private bool OnTheSameRow(int current, int target)
    {
        if (!IsItAGameFieldIndex(new[] { current, target })) return false;

        int currentRow = gameFields[current].row;
        int targetRow = gameFields[target].row;

        return currentRow == targetRow;
    }

    private bool IsItAGameFieldIndex(int[] indexes)
    {
        if (gameFields == null) return false;
        
        foreach (int index in indexes)
        {
            if (index < 0 || index >= gameFields.Length)
            {
                return false;
            }
        }

        return true;
    }
}
