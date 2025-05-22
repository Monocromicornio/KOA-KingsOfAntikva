using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Piece))]
public class SelectField : MonoBehaviour
{
    private Piece piece;
    private MatchController matchController => MatchController.instance;
    private SoundController soundController => matchController.soundController;
    private BoardController board => matchController.boardController;
    private GameField[] gameFields => board.gameFields;

    private Dictionary<string, List<GameField>> selectedFields = new Dictionary<string, List<GameField>>();
    private bool getted;

    [SerializeField]
    [Min(1)]
    private int distance;

    private void Awake()
    {
        piece = GetComponent<Piece>();
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
        ActiveSelectablesFields();

        if (selectedFields.Count == 0) return;

        soundController.Select();
    }

    private void Deselect()
    {
        foreach (List<GameField> fields in selectedFields.Values)
        {
            foreach (GameField gameField in fields)
            {
                gameField.Deselect();
            }
        }

        selectedFields.Clear();
        soundController.Cancel();
    }

    private void ActiveSelectablesFields()
    {
        //X axis
        SelectFieldsInSameRow(piece.indexCurrentField, distance);
        SelectFieldsInSameRow(piece.indexCurrentField, -distance);

        //Y axis
        SelectFieldsInSameColumn(piece.indexCurrentField, distance);
        SelectFieldsInSameColumn(piece.indexCurrentField, -distance);
    }

    private void SetSelectField(GameField gameField, string key)
    {
        if (gameField == null) return;

        if (!selectedFields.ContainsKey(key))
        {
            selectedFields.Add(key, new List<GameField>());
        }
        selectedFields[key].Add(gameField);

        if (gameField.hasPiece)
        {
            Piece pieace = gameField.piece;
            string pieceTag = pieace.tag;

            if (pieceTag == tag) return;

            gameField.Select();
            gameField.AttackMode = true;
            piece.Attack = true;
            pieace.Attacked = true;
        }

        gameField.Select();
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
                if (index < 0) index = 0;

                return fields[index];
            }
        }

        return null;
    }

#region Column
    private void SelectFieldsInSameColumn(int current, int distance, bool add = true)
    {
        int columnLength = board.ColumnLength();
        string key = add? "column_up" : "column_down";

        for (int i = 1; i <= distance; i++)
        {
            int interval = columnLength * i * (add ? 1 : -1);
            int target = current + interval;
            GameField field = GetFieldInSameColumn(current, target);

            SetSelectField(field, key);

            if (field == null || field.hasPiece) return;
        }
    }

    private GameField GetFieldInSameColumn(int current, int target)
    {
        if (!OnTheSameColumn(current, target)) return null;

        return GetField(target);
    }

    private bool OnTheSameColumn(int current, int target)
    {
        if (!IsItAGameFieldIndex(new[] { current, target })) return false;

        string currentColumn = gameFields[current].ColumnName;
        string targetColumn = gameFields[target].ColumnName;

        return currentColumn == targetColumn;
    }
#endregion

#region Row
    private void SelectFieldsInSameRow(int current, int distance, bool add = true)
    {
        string key = add? "row_right" : "row_left";

        for (int i = 1; i <= distance; i++)
        {
            int interval = i * (add ? 1 : -1);
            int target = current + interval;
            GameField field = GetFieldInSameRow(current, target);

            SetSelectField(field, key);

            if (field == null || field.hasPiece) return;
        }
    }

    private GameField GetFieldInSameRow(int current, int target)
    {
        if (!OnTheSameRow(current, target)) return null;

        return GetField(target);
    }

    private bool OnTheSameRow(int current, int target)
    {
        if (!IsItAGameFieldIndex(new[] { current, target })) return false;

        int currentRow = gameFields[current].Row;
        int targetRow = gameFields[target].Row;

        return currentRow == targetRow;
    }
    #endregion

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
}