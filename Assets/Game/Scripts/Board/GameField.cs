using UnityEngine;

public class GameField : Field
{
    private MatchController matchController => MatchController.instance;

    public bool select => visualActive.activeSelf;
    public bool hasPiece => piece != null;
    public Piece piece { get; private set; }

    [SerializeField]
    ForceText forceText;

    [SerializeField]
    GameObject visualActive;

    private void Awake()
    {
        visualActive?.SetActive(false);
    }

    public void SetPiece(Piece piece)
    {
        this.piece = piece;

        if (piece == null)
        {
            forceText.force = "";
            return;
        }

        InteractivePiece combatPiece = piece.GetComponent<InteractivePiece>();
        if (combatPiece == null) return;
        forceText.force = combatPiece.force.ToString();
    }

    private void OnMouseDown()
    {
        if (select)
        {
            Selection();
        }
    }

    public void Selection()
    {
        matchController.currentePiece.SelectedAField(this);
    }

    public void Select()
    {
        visualActive.SetActive(true);
    }

    public void Deselect()
    {
        visualActive.SetActive(false);
    }
}
