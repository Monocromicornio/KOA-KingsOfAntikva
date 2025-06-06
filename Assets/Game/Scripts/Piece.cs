using System.Collections;
using com.onlineobject.objectnet;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private static Piece activePiece;

    protected MatchController matchController => MatchController.instance;
    protected BoardController board => matchController.boardController;
    protected GameField[] gameFields => board.gameFields;

    protected GameMode.GameType gameType => matchController.gameType;

    protected bool finished => matchController.finished;
    protected TurnState myTurn = TurnState.blue;

    public GameField field { get; private set; }
    public GameField targetField { get; private set; }
    public int indexCurrentField => field.index;

    public GameObject body;
    public PieceType type;

    protected virtual void OnMouseDown()
    {
        if (matchController.turn != myTurn) return;

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
        this.field = field;
        targetField = null;

        transform.position = field.transform.position;
        transform.Rotate(0, 0, 0, Space.Self);
    }

    public void SelectedAField(GameField field)
    {
        if (finished) return;

        matchController.MadeActionOnTurn();

        targetField = field;
        bool onField = CheckPieceOnField();
        if (!onField) SendMessage("NewTarget", targetField, SendMessageOptions.DontRequireReceiver);
    }

    public bool CheckPieceOnField()
    {
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

            field = targetField;
            field.SetPiece(this);

            SendMessage("ChangeField", targetField, SendMessageOptions.DontRequireReceiver);
            ChangeTurn();
            return true;
        }

        return false;
    }

    protected virtual void OnDestroy()
    {
        print("DESTROY " + name);
        if (activePiece == this) activePiece = null;
        field?.SetPiece(null);
        matchController?.RemovePieceFromPlayerSquad(this);
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
        GameObject fakeBody = Instantiate(prefabFake, transform.position, transform.rotation, transform);

        FakePiece fakePiece = gameObject.AddComponent<FakePiece>();
        fakePiece.body = body;
        fakePiece.type = type;
        if (field != null)
        {
            field.SetPiece(null);
            fakePiece.SetFirstField(field);
        }
        field = null;

        DestroyImmediate(this);
        fakePiece.SendMessage("Awake");
        fakePiece.SetFakeObj(fakeBody);

        return fakePiece;
    }
}