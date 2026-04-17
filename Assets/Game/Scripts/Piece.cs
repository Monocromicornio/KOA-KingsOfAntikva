using com.onlineobject.objectnet;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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


    public int indexCurrentField = -1;
    public int indexPreviousField = -1;
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
    public bool isDying { get; private set; }
    private bool onValueChangeSetted = false;
    private bool hasActedThisTurn = false;

    private bool syncronizeVariablesDelegateSetup = false;

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

    private void PassiveUpdate()
    {
        if (onValueChangeSetted) return;
        onValueChangeSetted = true;
        fieldIndex.OnValueChange((int oldValue, int newValue) =>
        {
            Debug.Log("[Piece] Field index value changed via Passive Update");
           // board.GetGameField(oldValue)?.SetPiece(null);
          //  field?.SetPiece(this);
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
        //if (!IsActive()) return;
        fieldIndex.SetValue(field.index);
        previousFieldIndex.SetValue(field.index);

        indexCurrentField = field.index;
        indexPreviousField = field.index;

        targetField = null;

        transform.position = this.field.transform.position;
        this.field.SetPiece(this);


        MinimapController.instance.RegisterPiece(this, field.index);

        if (syncronizeVariablesDelegateSetup == false)
        {
            this.fieldIndex.OnSynchonize(() => { return this.indexCurrentField; },
                            (int value) =>
                            {
                                Debug.Log("[Piece] ON SYNCRONIZE Field index value changed to " + value);
                                board.GetGameField(indexCurrentField)?.SetPiece(null);
                                this.field.SetPiece(this);
                                this.indexCurrentField = value;
                            });


            this.previousFieldIndex.OnSynchonize(() => { return this.indexPreviousField; },
                            (int value) =>
                            {
                                Debug.Log("[Piece] ON SYNCRONIZE Previous Field index value changed to " + value);
                                this.indexPreviousField = value;
                            });

            syncronizeVariablesDelegateSetup = true;
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
        if (!isDying)
        {
            // Destroyed externally (e.g. NetworkDestroy from remote) — run cleanup once
            matchController?.OnDestroyPiece(this);
            field?.SetPiece(null);
        }

        if (activePiece == this) activePiece = null;

        if (MinimapController.instance != null)
        {
            MinimapController.instance.UnregisterPiece(this);
        }
    }

    private void Destroy()
    {
        if (isDying) return;
        isDying = true;
        // Clear field reference immediately so other pieces don't see this as a valid target
        field?.SetPiece(null);
        matchController?.OnDestroyPiece(this);
        StartCoroutine(WaitToDestroy());
    }

    private IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(timeToDestroy);

        if (this == null || gameObject == null) yield break;

        if (hasConnection && IsActive())
        {
            NetworkGameObject.NetworkDestroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ChangeTurn()
    {
        bool isTutorialMode = TutorialModeController.IsTutorialActive();

        if (!isTutorialMode)
        {
            if (!IsActive())
            {
                Debug.LogWarning($"[Piece:{name}] ChangeTurn BLOCKED — IsActive()=false. IsMyTurn={matchController.IsMyTurn()}, turn={matchController.turn}");
                return;
            }

            // IsMyTurn() guard is only meaningful in online matches to prevent the remote copy
            // of a moved piece from firing a second ChangeTurn after the network has already applied it.
            if (hasConnection && !matchController.IsMyTurn())
            {
                Debug.LogWarning($"[Piece:{name}] ChangeTurn BLOCKED — IsMyTurn()=false (online). turn={matchController.turn}, myTurn={matchController.myTurn}");
                return;
            }
        }
        else
        {
            if (!IsActive())
            {
                Debug.LogWarning($"[Piece:{name}] ChangeTurn BLOCKED (tutorial) — IsActive()=false.");
                return;
            }
        }

        Debug.Log($"[Piece:{name}] ChangeTurn PASSING — calling matchController.ChangeTurn(). turn={matchController.turn}");
        SendMessage("EndTurn", targetField, SendMessageOptions.DontRequireReceiver);

        if (!isTutorialMode)
        {
            matchController.ChangeTurn();
        }
    }

    public void SetWin()
    {
        if (hasConnection) NetworkExecute(OnWin);
        else OnWin();
    }

    /// <summary>
    /// Triggers the win state locally only, without sending network messages.
    /// Used at end-game so each client handles its own visuals independently.
    /// </summary>
    public void SetWinLocal()
    {
        OnWin();
    }

    private void OnWin()
    {
        //if (!IsActive()) return;
        SendMessage("Win");
    }

    public void SetLose()
    {
        if (hasConnection) NetworkExecute(OnLose);
        else OnLose();
    }

    /// <summary>
    /// Triggers the lose/death state locally only, without sending network messages.
    /// Used at end-game so pieces only die on the winner's screen.
    /// </summary>
    public void SetLoseLocal()
    {
        OnLose();
    }

    private void OnLose()
    {
        //if (!IsActive()) return;
        SendMessage("Destroy");
    }

    /// <summary>
    /// Triggers the explosion effect on all clients via network.
    /// Used by BombPiece to show the explosion only during counter-attack.
    /// </summary>
    public void TriggerExplosion()
    {
        if (hasConnection) NetworkExecute(OnTriggerExplosion);
        else OnTriggerExplosion();
    }

    private void OnTriggerExplosion()
    {
        SendMessage("Explode", SendMessageOptions.DontRequireReceiver);
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