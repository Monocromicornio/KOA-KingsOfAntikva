using System.Collections;
using com.onlineobject.objectnet;
using UnityEngine;

public class Piece : NetworkBehaviour
{
    public static Piece activePiece { get; private set; }

    private MatchController matchController => MatchController.instance;
    private bool hasConnection => matchController.hasConnection;
    private BoardController board => matchController.boardController;

    private bool finished => matchController.finished;
    public PieceColor pieceColor { get; private set; }

    private GameField firstField;
    private NetworkVariable<int> fieldIndex = -1;
    private NetworkVariable<int> previousFieldIndex = -1;

    public int indexCurrentField => (int)fieldIndex;
    public int indexPreviousField => (int)previousFieldIndex;
    public GameField field
    {
        get
        {
            if (fieldIndex < 0) return firstField;
            return board.GetGameField(indexCurrentField);
        }
    }
    [HideInInspector]
    public GameField targetField;

    public GameObject body;
    public PieceType type;

    public float timeToDestroy { get; private set; }
    public bool isMyPiece { get; private set; }
    private bool onValueChangeSetted = false;
    private bool hasActedThisTurn = false;

    private void Awake()
    {
        pieceColor = PieceColor.undefined;
        timeToDestroy = 3.5f;
    }

    private void Start()
    {
        if (matchController == null) return;
        matchController.OnInstantiatedPiece(this);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (MinimapController.instance != null && fieldIndex >= 0)
        {
            MinimapController.instance.RegisterPiece(this);
        }
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

    /// <summary>
    /// Explicitly marks whether this piece belongs to the local player. Must be called right after instantiation.
    /// </summary>
    public void SetAsMyPiece(bool isMy)
    {
        isMyPiece = isMy;
    }

    private void SetControl()
    {
        // NetworkExecuteOnClient also executes locally on the host. Guard against this so the
        // correct isMyPiece value set by SetAsMyPiece() at spawn time is never overwritten on the host.
        if (!matchController.networkManager.IsServerConnection())
            isMyPiece = true;

        TakeControl();
    }

    public void ActivePiece()
    {
        // isMyPiece is set synchronously at spawn time (SetAsMyPiece), avoiding the race condition
        // that occurred when relying on IsActive() which depends on async network ownership state.
        if (hasConnection)
        {
            if (isMyPiece)
            {
                TurnBluePiece();
            }
            else
            {
                TurnRedPiece();
            }
        }
        else if (pieceColor == PieceColor.undefined)
        {
            TurnBluePiece();
        }

        GameField gameField = board.SearchMyField(this);
        if (gameField != null) SetFirstField(gameField);
        else Debug.LogWarning($"Gamefield null on {name} ({matchController.myTurn})");

        gameObject.SetActive(true);
    }

    public void ResetTurnAction()
    {
        hasActedThisTurn = false;
    }

    public virtual void Select()
    {
        if (pieceColor == PieceColor.red) return;
        if (hasActedThisTurn) return;
        
        bool isTutorialMode = TutorialModeController.IsTutorialActive();
        
        if (!isTutorialMode)
        {
            if (!matchController.IsMyTurn()) return;
            if (!hasConnection && matchController.turn == TurnState.awayTeam) return;
            if (matchController.currentTurn == TurnState.wait) return;
        }

        if (activePiece != this)
        {
            activePiece?.SendMessage("Deselect", SendMessageOptions.DontRequireReceiver);
            activePiece = this;
        }

        TutorialEvents.TriggerPieceSelected(this);

        SendMessage("GetPiece", SendMessageOptions.DontRequireReceiver);
    }

    public void SetFirstField(GameField field)
    {
        firstField = field;
        if (!IsActive()) return;
        fieldIndex.SetValue(field.index);
        previousFieldIndex.SetValue(field.index);

        targetField = null;

        transform.position = this.field.transform.position;
        this.field.SetPiece(this);

        if (MinimapController.instance != null)
        {
            MinimapController.instance.RegisterPiece(this);
        }
    }

    public void SelectedAField(GameField field)
    {
        if (!IsActive() || finished) return;
        if (hasActedThisTurn) return;

        bool isTutorialMode = TutorialModeController.IsTutorialActive();
        
        if (!isTutorialMode)
        {
            matchController.MadeActionOnTurn();
        }
        
        hasActedThisTurn = true;
        targetField = field;
        
        bool onField = CheckPieceOnField();
        if (!onField) SendMessage("NewTarget", targetField, SendMessageOptions.DontRequireReceiver);
        
        SendMessage("Deselect", SendMessageOptions.DontRequireReceiver);
    }

    public void ForceSelectField(GameField field)
    {
        if (!IsActive() || finished) return;

        targetField = field;
        
        bool onField = CheckPieceOnField();
        if (!onField) SendMessage("NewTarget", targetField, SendMessageOptions.DontRequireReceiver);
        
        SendMessage("Deselect", SendMessageOptions.DontRequireReceiver);
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

        const float distanceThreshold = 0.1f;
        if (Vector3.Distance(transform.position, targetField.transform.position) <= distanceThreshold)
        {
            GameField oldField = field;
            int oldIndex = fieldIndex;
            previousFieldIndex.SetValue(oldIndex);
            targetField.SetPiece(null);
            field?.SetPiece(null);

            fieldIndex.SetValue(targetField.index);
            field.SetPiece(this);

            TutorialEvents.TriggerPieceMoved(this, oldField, field);

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

        if (MinimapController.instance != null)
        {
            MinimapController.instance.UnregisterPiece(this);
        }
    }

    private void Destroy()
    {
        OnDestroy();
        StartCoroutine(WaitToDestroy());
    }

    private IEnumerator WaitToDestroy()
    {
        if (!IsActive()) yield break;

        yield return new WaitForSeconds(timeToDestroy);
        if (hasConnection) NetworkGameObject.NetworkDestroy(gameObject);
        else Destroy(gameObject);
    }

    private void ChangeTurn()
    {
        bool isTutorialMode = TutorialModeController.IsTutorialActive();

        if (!isTutorialMode)
        {
            if (!IsActive() ) return; //|| !matchController.IsMyTurn()
        }
        else
        {
            if (!IsActive()) return;
        }
        
        SendMessage("EndTurn", targetField, SendMessageOptions.DontRequireReceiver);
        
        if (!isTutorialMode)
        {
            matchController.ChangeTurn();
        }
    }

    public void SetWin()
    {
        if (hasConnection) { NetworkExecute(OnWin); NetworkExecuteOnClient(OnWin); }        
        else OnWin();
    }

    private void OnWin()
    {
        //if (!IsActive()) return;
        SendMessage("Win");
    }

    public void SetLose()
    {
        if (hasConnection) { NetworkExecute(OnLose); NetworkExecuteOnClient(OnLose); }
        else OnLose();
    }

    private void OnLose()
    {
        //if (!IsActive()) return;
        SendMessage("Destroy");
    }

    public void TurnRedPiece()
    {
        if (pieceColor == PieceColor.red) return;

        if (matchController != null)
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