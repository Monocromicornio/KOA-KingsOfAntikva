using System.Collections;
using com.onlineobject.objectnet;
using UnityEngine;

public class Piece : NetworkBehaviour
{
    private static Piece activePiece;

    private MatchController matchController => MatchController.instance;
    private BoardController board => matchController.boardController;
    private GameField[] gameFields => board.gameFields;

    private GameMode.GameType gameType => matchController.gameType;

    private bool finished => matchController.finished;
    private TurnState myTurn = TurnState.blue;

    private int fieldIndex = -1;
    public GameField field
    {
        get
        {
            if (fieldIndex < 0) return null;
            return board.GetGameField((int)fieldIndex);
        }
    }
    private int targetFieldIndex = -1;
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

    private void Start()
    {
        if (matchController == null) return;
        matchController.OnInstantiatedPiece(this);
        gameObject.SetActive(false);
    }

    public void SetControlToClient()
    {
        NetworkExecuteOnClient(SetControl);
    }

    private void SetControl()
    {
        TakeControl();
    }

    public void ActivePiece()
    {
        if (IsActive()) TurnNormalPiece();
        else TurnFakePiece();
        gameObject.SetActive(true);
    }

    private void OnMouseDown()
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

    public void SetFirstFieldIndex(int field)
    {
        fieldIndex = field;
        targetFieldIndex = -1;

        transform.position = this.field.transform.position;
        transform.Rotate(0, 0, 0, Space.Self);
    }

    public void SetFirstField(GameField field)
    {
        NetworkExecute<int>(SetFirstFieldIndex, field.index);
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

    private void OnDestroy()
    {
        matchController?.RemovePieceFromPlayerSquad(this);
        if (activePiece == this) activePiece = null;
        field?.SetPiece(null);
    }

    private void Destroy()
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

    public void TurnFakePiece()
    {
        if (myTurn == TurnState.red) return;

        myTurn = TurnState.red;
        FakePiece fakePiece = GetComponent<FakePiece>();
        if (fakePiece != null)
        {
            fakePiece.enabled = true;
            return;
        }

        gameObject.AddComponent<FakePiece>();
    }

    public void TurnNormalPiece()
    {
        if (myTurn == TurnState.blue) return;

        myTurn = TurnState.blue;
        FakePiece fakePiece = GetComponent<FakePiece>();
        if (fakePiece == null) return;
        fakePiece.enabled = false;
    }
}