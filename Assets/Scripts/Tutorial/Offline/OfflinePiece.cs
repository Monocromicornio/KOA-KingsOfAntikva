using System.Collections;
using UnityEngine;

public class OfflinePiece : MonoBehaviour
{
    public static OfflinePiece activePiece { get; private set; }

    private BoardController board;
    
    public PieceColor pieceColor { get; private set; }

    private GameField firstField;
    private int fieldIndex = -1;
    public int indexCurrentField => fieldIndex;
    
    public GameField field
    {
        get
        {
            if (board == null || fieldIndex < 0) return firstField;
            return board.GetGameField(fieldIndex);
        }
    }

    [HideInInspector]
    public GameField targetField;

    public GameObject body;
    public PieceType type;

    public float timeToDestroy { get; private set; }

    private void Awake()
    {
        pieceColor = PieceColor.undefined;
        timeToDestroy = 3.5f;
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

        if (board == null)
        {
            Debug.LogWarning($"BoardController não encontrado para {name}");
        }
    }

    public void ActivePiece()
    {
        if (pieceColor == PieceColor.undefined)
        {
            TurnBluePiece();
        }

        gameObject.SetActive(true);
    }

    public virtual void Select()
    {
        if (pieceColor == PieceColor.red) return;

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
        fieldIndex = field.index;
        targetField = null;

        transform.position = this.field.transform.position;
        this.field.SetOfflinePiece(this);
    }

    public void SetField(GameField field)
    {
        if (field == null)
        {
            Debug.LogWarning($"Tentando setar field null em {name}");
            return;
        }

        if (field.hasPiece)
        {
            Debug.LogWarning($"[OfflinePiece] Field {field.index} já tem uma peça! Limpando antes de spawnar {name}");
            if (field.offlinePiece != null)
            {
                field.SetOfflinePiece(null);
            }
            else if (field.piece != null)
            {
                field.SetPiece(null);
            }
        }

        firstField = field;
        fieldIndex = field.index;
        targetField = null;

        transform.position = field.transform.position;
        field.SetOfflinePiece(this);
    }

    public void SelectedAField(GameField field)
    {
        Debug.Log($"[OfflinePiece] SelectedAField called on {name}. Target field: {field?.index ?? -1}");
        
        targetField = field;
        bool onField = CheckPieceOnField();
        
        Debug.Log($"[OfflinePiece] CheckPieceOnField returned: {onField}");
        
        if (!onField) 
        {
            Debug.Log($"[OfflinePiece] Sending NewTarget message");
            SendMessage("NewTarget", targetField, SendMessageOptions.DontRequireReceiver);
        }
    }

    public bool CheckPieceOnField()
    {
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
            
            if (targetField.offlinePiece != null)
            {
                targetField.SetOfflinePiece(null);
            }
            else if (targetField.piece != null)
            {
                targetField.SetPiece(null);
            }
            
            field?.SetOfflinePiece(null);

            fieldIndex = targetField.index;
            field.SetOfflinePiece(this);

            TutorialEvents.TriggerPieceMoved(this, oldField, field);

            SendMessage("ChangeField", targetField, SendMessageOptions.DontRequireReceiver);
            ChangeTurn();
            return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        if (activePiece == this) activePiece = null;
        field?.SetOfflinePiece(null);
    }

    private void Destroy()
    {
        OnDestroy();
        StartCoroutine(WaitToDestroy());
    }

    private IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(gameObject);
    }

    private void ChangeTurn()
    {
        SendMessage("EndTurn", targetField, SendMessageOptions.DontRequireReceiver);
    }

    public void SetWin()
    {
        OnWin();
    }

    private void OnWin()
    {
        SendMessage("Win", SendMessageOptions.DontRequireReceiver);
    }

    public void SetLose()
    {
        OnLose();
    }

    private void OnLose()
    {
        SendMessage("Destroy", SendMessageOptions.DontRequireReceiver);
    }

    public void TurnRedPiece()
    {
        if (pieceColor == PieceColor.red) return;

        pieceColor = PieceColor.red;
        OfflineFakePiece fakePiece = GetComponent<OfflineFakePiece>();
        if (fakePiece != null)
        {
            fakePiece.enabled = true;
            return;
        }

        gameObject.AddComponent<OfflineFakePiece>();
    }

    public void TurnBluePiece()
    {
        if (pieceColor == PieceColor.blue) return;

        pieceColor = PieceColor.blue;
        OfflineFakePiece fakePiece = GetComponent<OfflineFakePiece>();
        if (fakePiece == null) return;
        fakePiece.enabled = false;
    }
}
