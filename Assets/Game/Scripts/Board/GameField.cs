using UnityEngine;

public class GameField : Field
{
    public Transform Target;

    public bool select { get; private set; }
    public bool hasPiece => piece != null;
    public Piece piece { get; private set; }

    public bool AttackMode = false;

    [SerializeField]
    TextMesh TxtForce;

    GameObject VisualActive;

    Turn turn;

    AudioSource Confirm;

    GameMode gamemode;

    GameMode.GameType iGameMode = 0;

    bool bPlayer = false;

    void Start()
    {
        if (GameObject.Find("Confirm"))
        {
            Confirm = GameObject.Find("Confirm").GetComponent<AudioSource>();
        }

        if (transform.Find("Preview"))
        {
            VisualActive = transform.Find("Preview").gameObject;
            VisualActive.SetActive(false);
        }

        turn = FindObjectOfType<Turn>();

        gamemode = FindObjectOfType<GameMode>();
        iGameMode = gamemode.type;
    }

    public void SetTextForce(string force)
    {
        //TxtForce.gameObject.SetActive(true);
        TxtForce.text = force;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPiece)
        {
            if (piece)
            {
                if (piece.tag == "Player")
                {
                    bPlayer = true;
                }
                else
                {
                    bPlayer = false;
                }
            }
        }

        if (bPlayer)
        {

            if (hasPiece)
            {
                if (piece)
                {
                    string sForceType = piece.GetComponent<Piece>().GetForceType();

                    if (sForceType != "F")
                    {
                        if (sForceType != "B")
                        {
                            if (!TxtForce.gameObject.activeSelf)// && turn.Liberate)
                            {
                                TxtForce.gameObject.SetActive(true);
                                TxtForce.text = piece.GetComponent<Piece>().GetForceType();
                            }
                        }
                    }
                }
            }
            else
            {
                TxtForce.gameObject.SetActive(false);
            }

        }
        else
        {
            if (iGameMode == GameMode.GameType.Training)
            {
                if (hasPiece)
                {
                    if (!TxtForce.gameObject.activeSelf)// && turn.Liberate)
                    {
                        TxtForce.gameObject.SetActive(true);
                        TxtForce.text = piece.GetComponent<Piece>().GetForceType();
                    }
                }
                else
                {
                    TxtForce.gameObject.SetActive(false);
                }
            }
            else if (iGameMode == GameMode.GameType.Normal)
            {
                if (hasPiece)
                {
                    //if (!TxtForce.gameObject.activeSelf)
                    //{
                    if (piece.GetComponent<Piece>().isDie())
                    {
                        TxtForce.gameObject.SetActive(true);
                        TxtForce.text = piece.GetComponent<Piece>().GetForceType();
                    }
                    //}
                }
                else
                {
                    TxtForce.gameObject.SetActive(false);
                }
            }
            else
            {
                TxtForce.gameObject.SetActive(false);
            }

        }

    }

    public void SetPiece(Piece piece)
    {
        this.piece = piece;

        if (piece == null)
        {
            SetTextForce("");
            return;
        }

        SetTextForce(piece.GetForceType());
    }

    private void OnMouseDown()
    {
        if (turn.isPlayerTurn)
        {
            if (select && !AttackMode)
            {
                Selection();
            }
        }
    }

    public void Selection()
    {
        turn.Liberate = false;

        turn.currentePiece.SelectedAField(this);
    }

    public void Select()
    {
        select = true;

        if (turn.isPlayerTurn)
        {
            VisualActive.SetActive(true);
        }
    }

    public void Deselect()
    {
        select = false;
        VisualActive.SetActive(false);
        AttackMode = false;
    }
}
