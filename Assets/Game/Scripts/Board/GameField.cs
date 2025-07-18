using UnityEngine;

public class GameField : Field
{
    private MatchController matchController => MatchController.instance;

    public bool select => visualActive.activeSelf;

    [SerializeField]
    GameObject visualActive;

    private void Awake()
    {
        visualActive?.SetActive(false);
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
