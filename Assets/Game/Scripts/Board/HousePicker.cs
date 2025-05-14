using UnityEngine;
using UnityEngine.Events;

public class HousePicker : Field
{
    public bool select { get; private set; }
    public bool hasPiece {
        get {
            return piece != null;
        }
    }

    public GameObject piece { get; private set; }

    [SerializeField]
    TextMesh txtForce;

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

    public void SetPiece(GameObject piece, UnityAction call = null){
        this.piece = piece;
        if(call != null) onSelect.AddListener(call);
    }
}
