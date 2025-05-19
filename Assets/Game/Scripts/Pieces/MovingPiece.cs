using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Piece))]
public class MovingPiece : MonoBehaviour
{
    private Piece piece;
    private MatchController matchController => MatchController.instance;

    private BoardController board => matchController.boardController;
    private GameField[] gameFields => board.GetGameFieldFromFields();
    private List<GameField> selectedFields = new List<GameField>();

    [SerializeField]
    [Min(1)]
    private int distance;

    private void Awake()
    {
        piece = GetComponent<Piece>();
    }

    private void Select()
    {
        ActiveSelectablesFields();
    }

    private void Deselect()
    {
        foreach (GameField gameField in selectedFields)
        {
            gameField.Deselect();
        }

        selectedFields.Clear();
    }

    private void ActiveSelectablesFields()
    {
        //X axis
        SelectFieldsInSameRow(piece.iFieldLive, distance);
        SelectFieldsInSameRow(piece.iFieldLive, -distance);

        //Y axis
        SelectFieldsInSameColumn(piece.iFieldLive, distance);
        SelectFieldsInSameColumn(piece.iFieldLive, -distance);
    }

    void SetSelectField(GameField gameField)
    {
        if (gameField == null) return;
        selectedFields.Add(gameField);

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

    GameField GetField(int target)
    {
        if (target < 0 || target >= gameFields.Length) return null;

        return gameFields[target];
    }

#region Column
    void SelectFieldsInSameColumn(int current, int distance, bool add = true)
    {
        int columnLength = board.ColumnLength();

        for (int i = 1; i <= distance; i++)
        {
            int interval = columnLength * i * (add ? 1 : -1);
            int target = current + interval;
            GameField field = GetFieldInSameColumn(current, target);

            SetSelectField(field);

            if (field == null || field.hasPiece) return;
        }
    }

    GameField GetFieldInSameColumn(int current, int target)
    {
        if (!OnTheSameColumn(current, target)) return null;

        return GetField(target);
    }

    bool OnTheSameColumn(int current, int target)
    {
        if (!IsItAGameFieldIndex(new[] { current, target })) return false;

        string currentColumn = gameFields[current].ColumnName;
        string targetColumn = gameFields[target].ColumnName;

        return currentColumn == targetColumn;
    }
#endregion

#region Row
    void SelectFieldsInSameRow(int current, int distance, bool add = true)
    {
        for (int i = 1; i <= distance; i++)
        {
            int interval = i * (add ? 1 : -1);
            int target = current + interval;
            GameField field = GetFieldInSameRow(current, target);

            SetSelectField(field);

            if (field == null || field.hasPiece) return;
        }
    }

    GameField GetFieldInSameRow(int current, int target)
    {
        if (!OnTheSameRow(current, target)) return null;

        return GetField(target);
    }

    bool OnTheSameRow(int current, int target)
    {
        if (!IsItAGameFieldIndex(new[] { current, target })) return false;

        int currentRow = gameFields[current].Row;
        int targetRow = gameFields[target].Row;

        return currentRow == targetRow;
    }
    #endregion

    bool IsItAGameFieldIndex(int[] indexes)
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