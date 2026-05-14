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

    [Header("Selection Icons")]
    [Tooltip("Prefab spawned above enemy pieces that can be attacked.")]
    [SerializeField] private GameObject attackIconPrefab;

    [Tooltip("Prefab shown on the selected piece's own field.")]
    [SerializeField] private GameObject selectedIconPrefab;

    [Tooltip("Height offset for the attack icon above enemy pieces.")]
    [SerializeField] private float attackIconHeight = 2.5f;

    [Tooltip("Height offset for the selected icon on the piece's field.")]
    [SerializeField] private float selectedIconHeight = 0.1f;

    private GameField originField;

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

    /// <summary>
    /// Forces deselection of all highlighted fields and resets selection state.
    /// Call this when the piece is removed or the tutorial step changes to prevent stale field highlights.
    /// </summary>
    public void ForceDeselect()
    {
        Deselect();
        getted = false;
    }

    private void Select()
    {
        getted = true;
        ActiveSelectablesFields();

        if (selectedFields.Count == 0) return;

        ShowOriginIcon();
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

        HideOriginIcon();
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
        FieldDirection direction = AxisToDirection(axis);
        List<GameField> gameFields = GetFieldsInSameAxis(axis);

        foreach (GameField field in gameFields)
        {
            if (IsEnemyField(field))
            {
                field.SelectAsAttack(attackIconPrefab, attackIconHeight);
            }
            else
            {
                field.Select(direction);
            }
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

    // ========================
    // Direction & Icon Helpers
    // ========================

    /// <summary>
    /// Converts the internal Axis enum to the public FieldDirection enum.
    /// </summary>
    private static FieldDirection AxisToDirection(Axis axis)
    {
        switch (axis)
        {
            case Axis.columnPositive: return FieldDirection.Up;
            case Axis.columnNegative: return FieldDirection.Down;
            case Axis.rowPositive:    return FieldDirection.Right;
            case Axis.rowNegative:    return FieldDirection.Left;
            default:                  return FieldDirection.Down;
        }
    }

    /// <summary>
    /// Checks if a field contains an enemy piece relative to this piece (supports both online and offline pieces).
    /// </summary>
    private bool IsEnemyField(GameField field)
    {
        if (!field.hasPiece) return false;

        PieceColor targetColor;
        if (field.piece != null)
            targetColor = field.piece.pieceColor;
        else if (field.offlinePiece != null)
            targetColor = field.offlinePiece.pieceColor;
        else
            return false;

        return targetColor != piece.pieceColor;
    }

    /// <summary>
    /// Shows the origin/selected icon on the piece's own field.
    /// </summary>
    private void ShowOriginIcon()
    {
        if (selectedIconPrefab == null || board == null) return;

        originField = board.GetGameField(currentField);
        originField?.SelectAsOrigin(selectedIconPrefab, selectedIconHeight);
    }

    /// <summary>
    /// Hides the origin/selected icon from the piece's field.
    /// </summary>
    private void HideOriginIcon()
    {
        if (originField != null)
        {
            originField.ClearIcon();
            originField = null;
        }
    }
}
