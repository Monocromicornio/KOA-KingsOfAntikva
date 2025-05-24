using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private static Piece activePiece;

    protected MatchController matchController => MatchController.instance;
    protected SoundController soundController => matchController.soundController;

    protected BoardController board => matchController.boardController;
    protected GameField[] gameFields => board.gameFields;
    
    protected GameMode.GameType gameType => matchController.gameType;

    protected bool finished => matchController.finished;

    public GameField field { get; private set; }
    public GameField targetField { get; private set; }
    public int indexCurrentField => field.index;

    public PieceType type;

    [SerializeField]
    private GameObject gDie;

    [SerializeField]
    private GameObject gParticleChest;

    [Header("Sound")]
    [SerializeField]
    private AudioSource auDie, auDown;

    bool die = false;

    protected GameObject gChest;

    private void Awake()
    {
        if (type == PieceType.Flag)
        {
            gChest = transform.Find("Bau").gameObject;
        }
    }

    private void Start()
    {
        if (type == PieceType.Flag)
        {
            gChest = transform.Find("Bau").gameObject;
        }
    }

    protected virtual void OnMouseDown()
    {
        if (!matchController.isBlueTurn) return;

        if (activePiece != this)
        {
            activePiece?.SendMessage("Deselect", SendMessageOptions.DontRequireReceiver);
            activePiece = this;
        }

        if (tag == "Player" == matchController.isBlueTurn)
        {
            matchController.SetPiece(this);
            SendMessage("GetPiece", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SelectPeace()
    {
        matchController.ChangeTurn();
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
        targetField = field;
        SendMessage("NewTarget", targetField, SendMessageOptions.DontRequireReceiver);
        CheckPieceOnField();
    }

    public bool CheckPieceOnField()
    {
        if (field == targetField) return true;
        if (targetField == null) return false;

        if (transform.position == targetField.transform.position)
        {
            targetField.SetPiece(null);
            field?.SetPiece(null);

            field = targetField;
            field.SetPiece(this);

            SendMessage("ChangeField", targetField, SendMessageOptions.DontRequireReceiver);
            print("--------------END TURN------------------");
            SendMessage("EndTurn", targetField, SendMessageOptions.DontRequireReceiver);
            matchController.ChangeTurn();
            return true;
        }

        return false;
    }

    //@
    private int IndexHouse(int index)
    {
        int indexfield = 0;

        int indexloop = 0;

        foreach (GameField fc in gameFields)
        {
            if (fc.index == index)
            {
                indexfield = indexloop;
            }

            indexloop++;
        }

        return indexfield;
    }

    public void OpenChest()
    {
        //anim.SetBool("Open", true);

        gParticleChest.SetActive(true);
        soundController.VictoryConfirm();
        /*
        yield return new WaitForSeconds(waitTime);
        matchController.WinGame();
        */
    }

    public void CelebrateVitory()
    {
        if (!finished) return;

        /*
        SetAnimation("Win", true);
        yield return new WaitForSeconds(waitTime);
        soundController.VictoryPeaple();
        */
    }

    public void SetDie()
    {
        /*if(AttackEffect)
        {
            if (gameType == GameMode.GameType.Normal || gameType == GameMode.GameType.Hard)
            {
                if (tag == "Enemy")
                {
                    if (AttackEffectSoldier)
                    {
                        Instantiate(AttackEffectSoldier, AttackEffectPos.position, transform.rotation);
                    }
                }
                else
                {
                    if (auAttackYell)
                    {
                        Instantiate(AttackEffect, AttackEffectPos.position, transform.rotation);
                    }
                }
            }
            else
            {
                if (auAttackYell)
                {
                    Instantiate(AttackEffect, AttackEffectPos.position, transform.rotation);
                }
            }           

        }*/

        //anim.SetBool("Die", true);
        die = true;
        StartCoroutine(ShouPain(0.5f));
        StartCoroutine(ManDown(2.5f));
        StartCoroutine(IEDestroyAfterDying(3.5f));
    }

    public bool isDie()
    {
        return die;
    }

    private IEnumerator IEDestroyAfterDying(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        //if (type != PieceType.Bomb && bDieCounter)
        {
            if (matchController.currentePiece)
            {
                //matchController.currentePiece.GetComponent<Piece>().SetTakeHome(indexCurrentField);
            }
            matchController.ChangeTurn();
        }
        //if (type == PieceType.Bomb && !bDieCounter)
        {
            if (matchController.currentePiece)
            {
                //matchController.currentePiece.GetComponent<Piece>().SetTakeHome(indexCurrentField);
            }
            //turn.ChangeTurn();
        }
        //else if (type != PieceType.Bomb && !bDieCounter)
        {
            if (matchController.currentePiece)
            {
                //matchController.currentePiece.GetComponent<Piece>().SetTakeHome(indexCurrentField);
            }
            matchController.ChangeTurn();
        }
        //if (type == PieceType.Bomb && bDieCounter)
        {
            //CancelMovement();
            //turn.ChangeTurn();
        }
        //turn.ChangeTurn();
        Destroy(gameObject);
    }

    private IEnumerator ShouPain(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (gameType == GameMode.GameType.Hard)
        {
            if (tag == "Enemy")
            {
                soundController.DieSoldier();
            }
            else
            {
                auDie.Play();
            }
        }
        else
        {
            auDie.Play();
        }
    }

    private IEnumerator ManDown(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (gameType == GameMode.GameType.Hard)
        {
            if (tag == "Enemy")
            {
                soundController.DownSoldier();
            }
            else
            {
                if (auDown)
                {
                    auDown.Play();
                }
            }
        }
        else
        {
            if (auDown)
            {
                auDown.Play();
            }
        }

        yield return new WaitForSeconds(waitTime);
        //Instantiate(gDie, auDie.transform.position, auDie.transform.rotation);
        Instantiate(gDie, transform.position, gDie.transform.rotation);
        //Colar o player no chão
    }
}
