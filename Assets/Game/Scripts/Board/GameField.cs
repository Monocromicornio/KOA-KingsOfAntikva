using UnityEngine;

public class GameField : Field
{
    private MatchController matchController => MatchController.instance;

    public bool select => selectFeedback.activeSelf;
    public bool canSelect => canSelectFeedback.activeSelf;

    [SerializeField]
    GameObject selectFeedback, canSelectFeedback;

    private void Awake()
    {
        selectFeedback?.SetActive(false);
        canSelectFeedback?.SetActive(false);
    }

    private void Start()
    {
        if (matchController == null) return;

        if (matchController.networkManager.IsClientConnection())
        {
            forceText.transform.eulerAngles = new Vector3(90, 0, 180);
        }
    }

    private void Update()
    {
        if (matchController == null) return;

        if (canSelectFeedback.activeSelf && !matchController.IsMyTurn())
        {
            canSelectFeedback.SetActive(false);
        }
    }

    private void OnMouseOver()
    {
        if (matchController == null || !matchController.IsMyTurn()) return;

        canSelectFeedback.SetActive(true);
    }

    private void OnMouseExit()
    {
        canSelectFeedback.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (!canSelect) return;

        if (select)
        {
            Piece.activePiece?.SelectedAField(this);
            return;
        }
        
        piece?.Select();
    }

    public void Select()
    {
        selectFeedback.SetActive(true);
    }

    public void Deselect()
    {
        selectFeedback.SetActive(false);
    }
}
