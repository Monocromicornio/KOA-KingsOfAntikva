using UnityEngine;
using UnityEngine.Events;

public class EditableField : Field
{
    public bool select { get; private set; }

    [Header("Feedback")]
    [SerializeField]
    GameObject visualActive;

    private UnityEvent onSelect = new UnityEvent();

    void Awake()
    {
        visualActive.SetActive(false);
        onSelect.AddListener(Select);
    }

    private void OnMouseDown()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        onSelect.Invoke();
    }

    private void Select()
    {
        select = true;
        visualActive.SetActive(true);
    }

    public void Deselect()
    {
        select = false;
        visualActive.SetActive(false);
    }

    public override void SetPiece(Piece piece)
    {
        base.SetPiece(piece);
        if (piece != null)
        {
            piece.transform.position = transform.position;
        }
    }

    public override void SetOfflinePiece(OfflinePiece piece)
    {
        base.SetOfflinePiece(piece);
        if (piece != null)
        {
            piece.transform.position = transform.position;
        }
    }

    public void SetSelecteblePiece(Piece piece, UnityAction onSelect)
    {
        SetPiece(piece);
        this.onSelect.AddListener(onSelect);
    }
}
