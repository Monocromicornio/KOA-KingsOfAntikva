using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the direction of a selectable field relative to the piece.
/// </summary>
public enum FieldDirection
{
    Up,
    Down,
    Right,
    Left
}

/// <summary>
/// Holds information about a selectable field: its direction, reference, and whether it contains an enemy.
/// </summary>
public struct SelectableFieldInfo
{
    public FieldDirection Direction;
    public GameField Field;
    public bool HasEnemy;
    public Piece EnemyPiece;

    public SelectableFieldInfo(FieldDirection direction, GameField field, bool hasEnemy, Piece enemyPiece)
    {
        Direction = direction;
        Field = field;
        HasEnemy = hasEnemy;
        EnemyPiece = enemyPiece;
    }
}

[RequireComponent(typeof(Piece))]
public class SelectablePiece : MonoBehaviour
{
    private const string LOG_PREFIX = "[SelectablePiece]";

    private enum Axis
    {
        columnPositive,
        columnNegative,
        rowPositive,
        rowNegative,
    }

    public Piece piece { get; private set; }
    private int currentField => piece.indexCurrentField;
    private MatchController matchController => MatchController.instance;
    private SoundController soundController => matchController.soundController;
    private BoardController board => matchController.boardController;
    private GameField[] gameFields => board.gameFields;

    public Dictionary<string, List<GameField>> selectedFields { get; private set; }

    /// <summary>
    /// Cached list of selectable field info from the last selection. Available after a piece is selected.
    /// </summary>
    public List<SelectableFieldInfo> LastSelectionInfo { get; private set; }

    private bool getted;

    [SerializeField]
    [Min(1)]
    private int distance;

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
        LastSelectionInfo = new List<SelectableFieldInfo>();
        piece = GetComponent<Piece>();
    }

    private void OnEnable()
    {
        TurnTimerEvents.OnPlayerTimerEnded += OnPlayerTimerEnded;
    }

    private void OnDisable()
    {
        TurnTimerEvents.OnPlayerTimerEnded -= OnPlayerTimerEnded;
    }

    private void OnPlayerTimerEnded()
    {
        EndTurn();
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

        AnalyzeSelectableFields();
        ShowOriginIcon();
        soundController.Select();
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
        soundController.Cancel();
    }

    private void ActiveSelectablesFields()
    {
        //Column axis
        SelectFieldsInSameAxis(Axis.columnPositive);
        SelectFieldsInSameAxis(Axis.columnNegative);

        //Row axis
        SelectFieldsInSameAxis(Axis.rowPositive);
        SelectFieldsInSameAxis(Axis.rowNegative);
    }

    public GameField[][] GetSelectablesFields()
    {
        GameField[][] gameFields = new GameField[4][];
        //Column axis
        gameFields[0] = GetFieldsInSameAxis(Axis.columnPositive).ToArray();
        gameFields[1] = GetFieldsInSameAxis(Axis.columnNegative).ToArray();

        //Row axis
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
        if (target < 0 || target >= gameFields.Length) return null;

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
                field.SelectAsAttack();
            }
            else
            {
                field.Select(direction);
            }
        }
    }

    private List<GameField> GetFieldsInSameAxis(Axis axis)
    {
        List<GameField> gameFields = new List<GameField>();
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
                isSameSquad = field.piece.pieceColor == piece.pieceColor;
            }

            if (isSameSquad == true) break;
            gameFields.Add(field);
            AddToSelectFields(field, key);
            if (isSameSquad == false) break;
        }

        return gameFields;
    }

    private GameField GetFieldInSameAxis(Axis axis, int current, int target)
    {
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
    /// Checks if a field contains an enemy piece relative to this piece.
    /// </summary>
    private bool IsEnemyField(GameField field)
    {
        if (!field.hasPiece || field.piece == null) return false;
        return field.piece.pieceColor != piece.pieceColor;
    }

    /// <summary>
    /// Shows the origin/selected icon on the piece's own field.
    /// </summary>
    private void ShowOriginIcon()
    {
        if (selectedIconPrefab == null) return;

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

    // ========================
    // Field Analysis Methods
    // ========================

    private static readonly Dictionary<string, FieldDirection> DirectionKeyMap = new Dictionary<string, FieldDirection>
    {
        { "column_up",   FieldDirection.Up },
        { "column_down", FieldDirection.Down },
        { "row_right",   FieldDirection.Right },
        { "row_left",    FieldDirection.Left }
    };

    /// <summary>
    /// Analyzes all selectable fields after selection, populating LastSelectionInfo with direction and enemy data.
    /// </summary>
    private void AnalyzeSelectableFields()
    {
        LastSelectionInfo.Clear();

        foreach (KeyValuePair<string, List<GameField>> entry in selectedFields)
        {
            if (!DirectionKeyMap.TryGetValue(entry.Key, out FieldDirection direction)) continue;

            foreach (GameField field in entry.Value)
            {
                bool hasEnemy = false;
                Piece enemyPiece = null;

                if (field.hasPiece && field.piece != null)
                {
                    hasEnemy = field.piece.pieceColor != piece.pieceColor;
                    if (hasEnemy) enemyPiece = field.piece;
                }

                LastSelectionInfo.Add(new SelectableFieldInfo(direction, field, hasEnemy, enemyPiece));
            }
        }

        LogSelectionAnalysis();
    }

    /// <summary>
    /// Returns the direction of a specific GameField relative to this piece, or null if not in selectable range.
    /// </summary>
    public FieldDirection? GetFieldDirection(GameField targetField)
    {
        foreach (KeyValuePair<string, List<GameField>> entry in selectedFields)
        {
            if (entry.Value.Contains(targetField) && DirectionKeyMap.TryGetValue(entry.Key, out FieldDirection direction))
            {
                return direction;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether any selectable field in the given direction contains an enemy.
    /// </summary>
    public bool HasEnemyInDirection(FieldDirection direction)
    {
        foreach (SelectableFieldInfo info in LastSelectionInfo)
        {
            if (info.Direction == direction && info.HasEnemy) return true;
        }

        return false;
    }

    /// <summary>
    /// Returns all selectable fields that contain an enemy piece.
    /// </summary>
    public List<SelectableFieldInfo> GetEnemyFields()
    {
        List<SelectableFieldInfo> enemies = new List<SelectableFieldInfo>();

        foreach (SelectableFieldInfo info in LastSelectionInfo)
        {
            if (info.HasEnemy) enemies.Add(info);
        }

        return enemies;
    }

    /// <summary>
    /// Returns all selectable fields in a specific direction.
    /// </summary>
    public List<SelectableFieldInfo> GetFieldsInDirection(FieldDirection direction)
    {
        List<SelectableFieldInfo> result = new List<SelectableFieldInfo>();

        foreach (SelectableFieldInfo info in LastSelectionInfo)
        {
            if (info.Direction == direction) result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Logs the full analysis of selectable fields: enemies detected and available directions.
    /// </summary>
    private void LogSelectionAnalysis()
    {
        List<SelectableFieldInfo> enemies = GetEnemyFields();

        if (enemies.Count > 0)
        {
            foreach (SelectableFieldInfo enemy in enemies)
            {
                Debug.Log($"{LOG_PREFIX} '{piece.name}' tem INIMIGO '{enemy.EnemyPiece.name}' " +
                          $"na direcao {enemy.Direction} (field index {enemy.Field.index})");
            }
        }
        else
        {
            Debug.Log($"{LOG_PREFIX} '{piece.name}' nao tem inimigos nos espacos selecionaveis");
        }

        foreach (SelectableFieldInfo info in LastSelectionInfo)
        {
            if (!info.HasEnemy)
            {
                Debug.Log($"{LOG_PREFIX} '{piece.name}' -> campo livre na direcao {info.Direction} " +
                          $"(field index {info.Field.index}, row {info.Field.row}, col {info.Field.column})");
            }
        }
    }
}
