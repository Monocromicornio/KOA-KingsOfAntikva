using System.Collections;
using com.onlineobject.objectnet;
using UnityEngine;

public class Piece : NetworkBehaviour
{
    private static Piece activePiece;

    private MatchController matchController => MatchController.instance;
    private BoardController board => matchController.boardController;

    private bool finished => matchController.finished;
    public TurnState turn { get; private set; }
    public PieceColor pieceColor { get; private set; }

    private GameField firstField;
    private NetworkVariable<int> fieldIndex = -1;
    public int indexCurrentField => (int)fieldIndex;
    public GameField field
    {
        get
        {
            if (fieldIndex < 0) return firstField;
            return board.GetGameField(indexCurrentField);
        }
    }
    public GameField targetField;

    public GameObject body;
    public PieceType type;

    public float timeToDestroy { get; private set; }
    private bool onValueChangeSetted = false;

    private void Awake()
    {
        pieceColor = PieceColor.undefined;
        turn = TurnState.undefined;
        timeToDestroy = 3.5f;
    }

    private void Start()
    {
        if (matchController == null) return;
        matchController.OnInstantiatedPiece(this);
        gameObject.SetActive(false);
    }

    private void PassiveUpdate()
    {
        if (onValueChangeSetted) return;
        onValueChangeSetted = true;
        fieldIndex.OnValueChange((int oldValue, int newValue) =>
        {
            board.GetGameField(oldValue)?.SetPiece(null);
            field?.SetPiece(this);
        });
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
            NetworkManager manager = NetworkManager.Instance();
            turn = manager.IsServerConnection() ? TurnState.homeTeam : TurnState.awayTeam;
            TurnBluePiece();
        }
        else
        {
            TurnRedPiece();
        }

        GameField gameField = board.SearchMyField(this);
        if (gameField != null) SetFirstField(gameField);
        else Debug.LogWarning($"Gamefield null on {name} ({turn})");

        if (gameField.index < 16)
        {
            turn = TurnState.homeTeam;
            TurnBluePiece();
        }
        else
        {
            turn = TurnState.awayTeam;
            TurnRedPiece();
        }

        gameObject.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (pieceColor == PieceColor.red) return;
        if (matchController.currentTurn != turn) return;

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
        firstField = field;
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
        matchController?.OnDestroyPiece(this);
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
        yield return new WaitForSeconds(timeToDestroy);
        if (IsActive()) NetworkGameObject.NetworkDestroy(gameObject);
    }

    private void ChangeTurn()
    {
        if (!IsActive() || matchController.turn != turn) return;
        SendMessage("EndTurn", targetField, SendMessageOptions.DontRequireReceiver);
        matchController.ChangeTurn();
    }

    public void SetWin()
    {
        NetworkExecute(OnWin);
    }

    private void OnWin()
    {
        if (!IsActive()) return;
        SendMessage("Win");
    }

    public void SetLose()
    {
        NetworkExecute(OnLose);
    }

    private void OnLose()
    {
        if (!IsActive()) return;
        SendMessage("Destroy");
    }

    public void TurnRedPiece()
    {
        if (pieceColor == PieceColor.red) return;

        matchController.OnDestroyPiece(this);

        pieceColor = PieceColor.red;
        FakePiece fakePiece = GetComponent<FakePiece>();
        if (fakePiece != null)
        {
            fakePiece.enabled = true;
            return;
        }

        gameObject.AddComponent<FakePiece>();
    }

    public void TurnBluePiece()
    {
        if (pieceColor == PieceColor.blue) return;

        matchController.AddPieceFromPlayerSquad(this);

        pieceColor = PieceColor.blue;
        FakePiece fakePiece = GetComponent<FakePiece>();
        if (fakePiece == null) return;
        fakePiece.enabled = false;
    }
}