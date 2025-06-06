using System.Collections;
using com.onlineobject.objectnet;
using UnityEngine;

public class Piece : NetworkBehaviour
{
    private static Piece activePiece;

    protected MatchController matchController => MatchController.instance;
    protected BoardController board => matchController.boardController;
    protected GameField[] gameFields => board.gameFields;

    protected GameMode.GameType gameType => matchController.gameType;

    protected bool finished => matchController.finished;
    protected TurnState myTurn = TurnState.blue;

    private NetworkVariable<int> fieldIndex = -1;
    public GameField field
    {
        get
        {
            if (fieldIndex < 0) return null;
            return board.GetGameField((int)fieldIndex);
        }
    }
    private NetworkVariable<int> targetFieldIndex = -1;
    public GameField targetField
    {
        get
        {
            if (targetFieldIndex < 0) return null;
            return board.GetGameField((int)targetFieldIndex);
        }
    }
    public int indexCurrentField => fieldIndex;

    public GameObject body;
    public PieceType type;

    /*public void CheckColor()
    {
        if (ower == Ower.Free)
        {
            PlayerSquad squad = matchController.playerSquad;
            squad.TurnFakePiece(this);
        }
    }*/

    protected virtual void OnMouseDown()
    {
        if (myTurn == TurnState.red) return;
        if (matchController.turn == TurnState.red) return;

        if (activePiece != this)
        {
            activePiece?.SendMessage("Deselect", SendMessageOptions.DontRequireReceiver);
            activePiece = this;
        }

        matchController.SetPiece(this);
        SendMessage("GetPiece", SendMessageOptions.DontRequireReceiver);
    }

    public virtual void SetFirstField(GameField field)
    {
        if (!IsActive()) return;

        fieldIndex = field.index;
        targetFieldIndex = -1;

        transform.position = field.transform.position;
        transform.Rotate(0, 0, 0, Space.Self);
    }

    public void SelectedAField(GameField field)
    {
        if (!IsActive() || finished) return;

        matchController.MadeActionOnTurn();

        targetFieldIndex = field.index;
        bool onField = CheckPieceOnField();
        if (!onField) SendMessage("NewTarget", targetField, SendMessageOptions.DontRequireReceiver);
    }

    public bool CheckPieceOnField()
    {
        if (!IsActive()) return false;

        if (field == targetField)
        {
            ChangeTurn();
            return true;
        }
        if (targetField == null) return false;

        if (transform.position == targetField.transform.position)
        {
            targetField.SetPiece(null);
            field?.SetPiece(null);

            fieldIndex = targetField.index;
            field.SetPiece(this);

            SendMessage("ChangeField", targetField, SendMessageOptions.DontRequireReceiver);
            ChangeTurn();
            return true;
        }

        return false;
    }

    protected virtual void OnDestroy()
    {
        matchController?.RemovePieceFromPlayerSquad(this);
        if (activePiece == this) activePiece = null;
        field?.SetPiece(null);
    }

    protected void Destroy()
    {
        OnDestroy();
        StartCoroutine(WaitToDestroy());
    }

    private IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(3.5f);
        Destroy(gameObject);
    }

    private void ChangeTurn()
    {
        SendMessage("EndTurn", targetField, SendMessageOptions.DontRequireReceiver);
        matchController.ChangeTurn();
    }

    public void Win()
    {
        //print("WIN!!!!");
    }

    public void Lose()
    {
        SendMessage("Destroy");
    }

    public FakePiece TurnFakePiece(GameObject prefabFake)
    {
        myTurn = TurnState.red;
        GameObject fakeBody = Instantiate(prefabFake, transform.position, transform.rotation, transform);

        FakePiece fakePiece = gameObject.AddComponent<FakePiece>();
        fakePiece.SetFakeObj(fakeBody);

        return fakePiece;
    }
}