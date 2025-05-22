using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    protected MatchController matchController => MatchController.instance;
    protected SoundController soundController => matchController.soundController;

    protected BoardController board => matchController.boardController;
    protected GameField[] gameFields => board.gameFields;

    protected GameMode.GameType gameType => matchController.gameType;

    protected Turn turn => matchController.turn;

    protected bool finished => matchController.finished;

    protected GameField field;
    public GameField targetField { get; private set; }

    public int indexCurrentField => field.index;

    public ForceText forceTxt;

    public enum ItemType    
    {
        Soldado,
        Antibomba,
        Sargento,
        Tenente,
        Capitao,
        Major,
        Coronel,
        General,
        Ministro,
        Espia,
        Bandeira,
        Bomba
    }

    public ItemType Types;

    public int Force;

    [SerializeField]
    GameObject AttackEffect;

    [SerializeField]
    GameObject AttackEffectSoldier;

    [SerializeField]
    Transform AttackEffectPos;

    [SerializeField]
    GameObject gDie;

    [SerializeField]
    GameObject gParticleChest;

    [Header("Sound")]
    [SerializeField]
    AudioSource auAttackYell;

    [SerializeField]
    AudioSource auDie, auDown;

    string sDebug="";

    bool bDie = false;

    protected GameObject gChest;

    void Awake()
    {
        if (Types == ItemType.Bandeira)
        {
            gChest = transform.Find("Bau").gameObject;
        }
    }

    void Start()
    {
        if (forceTxt == null) print("force txt null " + name);
        forceTxt.force = Force.ToString();

        if (Types == ItemType.Bandeira)
        {
            gChest = transform.Find("Bau").gameObject;
        }
    }

    protected virtual void OnMouseDown()
    {
        if (!turn.isPlayerTurn) return;

        bool myTurn = tag == "Player" == turn.isPlayerTurn;
        if (myTurn && turn.Liberate == true)
        {
            turn.SetPiece(this);
            SendMessage("GetPiece", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SelectPeace()
    {
        turn.ChangeTurn();
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
            turn.ChangeTurn();
            return true;
        }

        return false;
    }

//@
    public string GetForceType()
    {
        string sforcetype = Force.ToString();

        if (Types == ItemType.Bandeira)
        {
            sforcetype = "F";
        }
        else if (Types == ItemType.Bomba)
        {
            sforcetype = "B";
        }
        else if (Types == ItemType.Espia)
        {
            sforcetype = "S";
        }

        return sforcetype;
    }

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

    public int[] HousesFree()
    {

        List<int> lhouses = new List<int>();

        GameField[] fcs = FindObjectsOfType<GameField>();

        int indexfield = 0;

        indexfield = IndexHouse(indexCurrentField);

        int iColumnCount = FindObjectOfType<BoardController>().ColumnLength()+1;

        int lenghtfields = fcs.Length;

        int indexfieldleft = (indexfield-1);
        int indexfieldright = (indexfield + 1);

        int indexfieldtop = (indexfield + iColumnCount);
        int indexfieldbottom = (indexfield - iColumnCount);

        if (indexfieldright < lenghtfields)
        {
            if (fcs[indexfieldright].Row == fcs[indexfield].Row)
            {
                if (fcs[indexfieldright].hasPiece)
                {
                    if (fcs[indexfieldright].piece.gameObject.tag == "Player")
                    {
                        lhouses.Add(fcs[indexfieldright].index);
                    }
                }
                else
                {
                    lhouses.Add(fcs[indexfieldright].index);
                }
            }
        }

        if (indexfieldleft >= 0)
        {
            if (fcs[indexfieldleft].Row == fcs[indexfield].Row)
            {
                if (fcs[indexfieldleft].hasPiece)
                {
                    if (fcs[indexfieldleft].piece.gameObject.tag == "Player")
                    {
                        lhouses.Add(fcs[indexfieldleft].index);
                    }
                }
                else
                {
                    lhouses.Add(fcs[indexfieldleft].index);
                }
            }
        }

        if (indexfieldtop < lenghtfields)
        {
            if (fcs[indexfieldtop].Column == fcs[indexfield].Column)
            {
                if (fcs[indexfieldtop].hasPiece)
                {
                    if (fcs[indexfieldtop].piece.gameObject.tag == "Player")
                    {
                        lhouses.Add(fcs[indexfieldtop].index);
                    }
                }
                else
                {
                    lhouses.Add(fcs[indexfieldtop].index);
                }
            }
        }

        if (indexfieldbottom >= 0)
        {
            if (fcs[indexfieldbottom].Column == fcs[indexfield].Column)
            {
                if (fcs[indexfieldbottom].hasPiece)
                {
                    if (fcs[indexfieldbottom].piece.gameObject.tag == "Player")
                    {
                        lhouses.Add(fcs[indexfieldbottom].index);
                    }
                }
                else
                {
                    lhouses.Add(fcs[indexfieldbottom].index);
                }
            }
        } 

        int[] ihouses = lhouses.ToArray();

        return ihouses;
    }

    public int[] CheckHouses()
    {

        List<int> lhouses = new List<int>();

        GameField[] fcs = FindObjectsOfType<GameField>();

        int indexfield = 0;

        indexfield = IndexHouse(indexCurrentField);

        int iColumnCount = FindObjectOfType<BoardController>().ColumnLength() + 1;

        int lenghtfields = fcs.Length;

        int indexfieldleft = (indexfield - 1);
        int indexfieldright = (indexfield + 1);

        int indexfieldtop = (indexfield + iColumnCount);
        int indexfieldbottom = (indexfield - iColumnCount);

        if (indexfieldright < lenghtfields)
        {
            if (fcs[indexfieldright].Row == fcs[indexfield].Row)
            {
                if (fcs[indexfieldright].hasPiece)
                {
                    if (fcs[indexfieldright].piece.gameObject.tag == "Enemy")
                    {
                        lhouses.Add(fcs[indexfieldright].index);
                    }
                }
                else
                {
                    lhouses.Add(fcs[indexfieldright].index);
                }
            }
        }

        if (indexfieldleft >= 0)
        {
            if (fcs[indexfieldleft].Row == fcs[indexfield].Row)
            {
                if (fcs[indexfieldleft].hasPiece)
                {
                    if (fcs[indexfieldleft].piece.gameObject.tag == "Enemy")
                    {
                        lhouses.Add(fcs[indexfieldleft].index);
                    }
                }
                else
                {
                    lhouses.Add(fcs[indexfieldleft].index);
                }
            }
        }

        if (indexfieldtop < lenghtfields)
        {
            if (fcs[indexfieldtop].Column == fcs[indexfield].Column)
            {
                if (fcs[indexfieldtop].hasPiece)
                {
                    if (fcs[indexfieldtop].piece.gameObject.tag == "Enemy")
                    {
                        lhouses.Add(fcs[indexfieldtop].index);
                    }
                }
                else
                {
                    lhouses.Add(fcs[indexfieldtop].index);
                }
            }
        }

        if (indexfieldbottom >= 0)
        {
            if (fcs[indexfieldbottom].Column == fcs[indexfield].Column)
            {
                if (fcs[indexfieldbottom].hasPiece)
                {
                    if (fcs[indexfieldbottom].piece.gameObject.tag == "Enemy")
                    {
                        lhouses.Add(fcs[indexfieldbottom].index);
                    }
                }
                else
                {
                    lhouses.Add(fcs[indexfieldbottom].index);
                }
            }
        }

        int[] ihouses = lhouses.ToArray();

        return ihouses;
    }

    public void OpenChest()
    {
        /*AnimatorStateInfo currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
        anim.SetBool("Open", true);*/

        gParticleChest.SetActive(true);

        soundController.VictoryConfirm();

        IEnumerator enumerator = IEWin(1.5f);
        StartCoroutine(enumerator);
    }

    private IEnumerator IEWin(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        matchController.WinGame();
    }

    public void SetVictory()
    {
        CelebrateVitory();
    }
    
    void CelebrateVitory()
    {
        if (!finished) return;

        //SetAnimation("Win", true);
        StartCoroutine(VictoryPlay(1.5f));
    }

    private IEnumerator VictoryPlay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        soundController.VictoryPeaple();
    } 

    public void SetTakeHome(int iGoField)
    {
        if (!finished)
        {
            IEnumerator enumerator = TakeHome(iGoField, 0.5f);
            StartCoroutine(enumerator);
        }
    }

    private IEnumerator TakeHome(int Field, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (!finished)
        {
            int iHomeField = Field;
            //MoveStart(gameFields[iHomeField]);
            IEnumerator enumerator = EndTurnAfterAttack(iHomeField, 1.5f);
            StartCoroutine(enumerator);
        }
    }

    private IEnumerator EndTurnAfterAttack(int Field, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (turn.currentePiece)
        {
            //cancela os movimentos
            //turn.currentePiece.GetComponent<Piece>().CancelMovement();

            //Reseta a casa
            turn.currentePiece.GetComponent<Piece>().ReleaseHouses();

            //Atualiza a casa atual após a peça andar
            //turn.currentePiece.GetComponent<Piece>().iFieldLive = Field;
        }
        sDebug = "EndTurnAfterAttack";
        Debug.Log(sDebug);
        //debugTotext.ShowDebug(sDebug);
    }

    private IEnumerator EndTurn(int Field, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //cancela os movimentos
        //turn.currentePiece.GetComponent<Piece>().CancelMovement();

        //Reseta a casa
        turn.currentePiece.GetComponent<Piece>().ReleaseHouses();

        //Atualiza a casa atual após a peça andar
        //turn.currentePiece.GetComponent<Piece>().iFieldLive = Field;

        //Troca o turno
        Debug.Log("EndTurn - turn.bChangeTurn = " + turn.bChangeTurn);
        if (!turn.bChangeTurn)
        {            
            turn.ChangeTurn();
        }

        sDebug = "Change Turn";
        Debug.Log(sDebug);
        //debugTotext.ShowDebug(sDebug);

    }

    bool bDieCounter = false;

    public void EndTurnEnemy()
    {
        IEnumerator enumerator = EndTurnCounter(2.5f);
        StartCoroutine(enumerator);
    }
    

    private IEnumerator EndTurnCounter(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        bDieCounter = true;
        //cancela os movimentos
        //CancelMovement();
        ReleaseHouses();

    } 

    public void SetDie()
    {
        //anim.SetBool("Die", true);
        bDie = true;

        IEnumerator enumerator1 = ShouPain(0.5f);
        StartCoroutine(enumerator1);

        IEnumerator enumerator2 = ManDown(2.5f);
        StartCoroutine(enumerator2);

        IEnumerator enumerator3 = IEDestroyAfterDying(3.5f);
        StartCoroutine(enumerator3);
    }

    public void SetSuicide()
    {
        //anim.SetBool("Die", true);
        //bDie = true;

        IEnumerator enumerator1 = ShouPain(0.5f);
        StartCoroutine(enumerator1);

        IEnumerator enumerator2 = ManDown(2.5f);
        StartCoroutine(enumerator2);

        IEnumerator enumerator3 = IEDestroyAfterSuicide(3.5f);
        StartCoroutine(enumerator3);
    }

    private IEnumerator IEDestroyAfterSuicide(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //CancelMovement();
        //ReleaseHouses();
        turn.ChangeTurn();
        Destroy(gameObject);
    
    }

    public bool isDie()
    {
        return bDie;
    }

    private IEnumerator IEDestroyAfterDying(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        sDebug = "DestroyAfterDie - Name = " + name + " - bDieCounter = " + bDieCounter;
        Debug.Log(sDebug);
        //debugTotext.ShowDebug(sDebug);

        if (Types != ItemType.Bomba && bDieCounter)
        {
            if (turn.currentePiece)
            {
                turn.currentePiece.GetComponent<Piece>().SetTakeHome(indexCurrentField);
            }
            sDebug = "DestroyAfterDie - Name = " + name + " - bDieCounter = " + bDieCounter;
            Debug.Log(sDebug);
            //debugTotext.ShowDebug(sDebug);
            //CancelMovement();
            ReleaseHouses();
            turn.ChangeTurn();
        }
        if (Types == ItemType.Bomba && !bDieCounter)
        {
            if (turn.currentePiece)
            {
                sDebug = "DestroyAfterDie - SetTakeHome iFieldLive = " + indexCurrentField;
                Debug.Log(sDebug);
                //debugTotext.ShowDebug(sDebug);
                turn.currentePiece.GetComponent<Piece>().SetTakeHome(indexCurrentField);
            }
            sDebug = "DestroyAfterDie - Name = " + name + " - bDieCounter = " + bDieCounter;
            Debug.Log(sDebug);
            //debugTotext.ShowDebug(sDebug);
            //CancelMovement();
            ReleaseHouses();
            //turn.ChangeTurn();
        }
        else if (Types != ItemType.Bomba && !bDieCounter)
        {
            if (turn.currentePiece)
            {
                turn.currentePiece.GetComponent<Piece>().SetTakeHome(indexCurrentField);
            }
            sDebug = "DestroyAfterDie - Name = " + name + " - bDieCounter = " + bDieCounter;
            Debug.Log(sDebug);
            //debugTotext.ShowDebug(sDebug);
            //CancelMovement();
            ReleaseHouses();
            turn.ChangeTurn();
        }
        if (Types == ItemType.Bomba && bDieCounter)
        {
            //if (turn.Piece)
            //{
            //    sDebug = "DestroyAfterDie - SetTakeHome iFieldLive = " + iFieldLive;
            //    Debug.Log(sDebug);
            //    debugTotext.ShowDebug(sDebug);
            //    turn.Piece.GetComponent<Player>().SetTakeHome(iFieldLive);
            //}
            sDebug = "DestroyAfterDie - Types == ItemType.Bomba = " +  ItemType.Bomba + " - bDieCounter = " + bDieCounter;
            Debug.Log(sDebug);
            //debugTotext.ShowDebug(sDebug);
            //CancelMovement();
            ReleaseHouses();
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
        IEnumerator enumerator = DieEffect(0.5f);
        StartCoroutine(enumerator);

    }
    private IEnumerator DieEffect(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //Instantiate(gDie, auDie.transform.position, auDie.transform.rotation);
        Instantiate(gDie, transform.position, gDie.transform.rotation);
        //Colar o player no chão
    }

    public void ReleaseHouses()
    { 
        foreach (GameField fd in gameFields)
        {
            if (fd.index == indexCurrentField)
            {
                fd.SetPiece(null);
            }
        }
    }
}
