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

    private NetworkVariable<int> fieldIndex = -1;
    public int indexCurrentField => (int)fieldIndex;
    public GameField field
    {
        get
        {
            if (fieldIndex < 0) return null;
            return board.GetGameField(indexCurrentField);
        }
    }
    public GameField targetField;

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
        if (IsActive())
        {
            TurnNormalPiece();
            GameField gameField = board.SearchMyField(this);
            if (gameField != null) SetFirstField(gameField);
        }
        else
        {
            TurnFakePiece();
        }

        fieldIndex.OnValueChange((int oldValue, int newValue) =>
        {
            Debug.Log($"Value was updated from {oldValue} to {newValue} ");
            field?.SetPiece(this);
        });

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

    public void SetFirstField(GameField field)
    {
        if (!IsActive()) return;
        fieldIndex = field.index;
        targetField = null;

        transform.position = this.field.transform.position;
        this.field.SetPiece(this);
    }

    public void SelectedAField(GameField field)
    {
        if (!IsActive() || finished) return;

        matchController.MadeActionOnTurn();
        targetField = field;
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