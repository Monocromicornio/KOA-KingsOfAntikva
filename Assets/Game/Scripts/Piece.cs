using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private MatchController matchController => MatchController.instance;
    private SoundController soundController => matchController.soundController;

    private BoardController board => matchController.boardController;
    private GameField[] gameFields => board.GetGameFieldFromFields();

    protected GameMode.GameType gameType => matchController.gameType;

    private Turn turn => matchController.turn;

    [SerializeField]
    protected Animator anim;

    private bool finished => matchController.finished;

    public bool selected { get; private set; }
    private GameField currentField;

    public GameField GetCurrentField()
    {
        return currentField;
    }

    private void SetCurrentField(GameField value)
    {
        currentField?.SetPiece(null);
        
        currentField = value;
        iFieldLive = value.index;

        currentField.SetPiece(this);
    }

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
    public int Rule;
    public int iFieldLive;
    public float MoveSpeed;
    public bool Attacked = false;

    public bool Attack = false;

    int iTargetField;

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
        forceTxt.force = Force.ToString();

        if (Types == ItemType.Bandeira)
        {
            gChest = transform.Find("Bau").gameObject;
        }
    }

    void Update()
    {
        StopAttack();
    }

    private void OnMouseDown()
    {
        if (!turn.isPlayerTurn) return;

        SelectPeace();
    }

    public void SelectPeace()
    {
        if (finished) return;

        bool myTurn = tag == "Player" == turn.isPlayerTurn;
        if (myTurn && turn.Liberate == true)
        {
            if (!selected)
            {
                SendMessage("Select", SendMessageOptions.DontRequireReceiver);
                selected = true;
                turn.SetPiece(this);
                soundController.Select();
            }
            else
            {
                SendMessage("Deselect", SendMessageOptions.DontRequireReceiver);
                selected = false;
                turn.SetPiece(null);
                soundController.Cancel();
            }
        }

        if (Attacked == true)
        {
            SetAttack();
        }
    }

    GameObject gEnemyPieace;

    void SetAttack()
    {       
        
        Piece gpieace = turn.currentePiece;

        if (gpieace)
        {
            turn.Liberate = false;
            gpieace.AttackRules(gameObject);
        }
    }    

    public void AttackRules(GameObject pieace)
    {
        iTargetField = pieace.GetComponent<Piece>().iFieldLive;
        gEnemyPieace = pieace;
        LookEnemy(pieace.transform);

        if (pieace.GetComponent<Piece>().Types != ItemType.Bomba)
        {
            if (pieace.GetComponent<Piece>().Types != ItemType.Bandeira)
            {
                pieace.GetComponent<Piece>().LookEnemy(transform);
            }
        }

        if (pieace.GetComponent<Piece>().Types == ItemType.Bandeira)
        {
            if (Types != ItemType.Soldado)
            {
                sDebug = "Attack pieace.GetComponent<Player>().Types = " + pieace.GetComponent<Piece>().Types;
                Debug.Log(sDebug);
                //debugTotext.ShowDebug(sDebug);

                soundController.PreAttack();

                IEnumerator enumerator = IEattack(pieace, 1.0f);
                StartCoroutine(enumerator);

            }
            //else if (pieace.GetComponent<Player>().Types == ItemType.Soldado)
            else if (Types == ItemType.Soldado)
            {
                SetCurrentField(gameFields[iTargetField]);

                PlayStep();

                IEnumerator enumerator = MovetoAttack(pieace, true);
                StartCoroutine(enumerator);

            }
        }
        else if (Types == ItemType.Antibomba && pieace.GetComponent<Piece>().Types == ItemType.Bomba)
        {

            if (pieace.GetComponent<Piece>().Types != ItemType.Soldado)
            {

                sDebug = "Pieace " + pieace.name + " is dead";
                Debug.Log(sDebug);
                //debugTotext.ShowDebug(sDebug);

                soundController.PreAttack();

                IEnumerator enumerator = IEattack(pieace, 1.0f);
                StartCoroutine(enumerator);
            }
            else if (pieace.GetComponent<Piece>().Types == ItemType.Soldado)
            {
                sDebug = "Move to " + pieace.GetComponent<Piece>().iFieldLive + " - " + "Name of house " + gameFields[iTargetField].GetComponent<GameField>().gameObject.name;

                Debug.Log(sDebug);
                //debugTotext.ShowDebug(sDebug);

                SetCurrentField(gameFields[iTargetField]);

                PlayStep();

                IEnumerator enumerator = MovetoAttack(pieace, true);
                StartCoroutine(enumerator);
                
            }

        }
        else if (Types != ItemType.Antibomba && pieace.GetComponent<Piece>().Types == ItemType.Bomba)
        {
            if (Types != ItemType.Soldado)
            {
                soundController.PreAttack();
                pieace.GetComponent<Piece>().CounterAttack(gameObject);
            }
            else if (Types == ItemType.Soldado)
            {
                sDebug = "Move to " + pieace.GetComponent<Piece>().iFieldLive + " - " + "Name of house " + gameFields[iTargetField].GetComponent<GameField>().gameObject.name;

                Debug.Log(sDebug);
                //debugTotext.ShowDebug(sDebug);

                SetCurrentField(gameFields[iTargetField]);

                PlayStep();

                IEnumerator enumerator = MovetoAttack(pieace, false);
                StartCoroutine(enumerator);
            }


        }
        else if (Types == ItemType.Espia && pieace.GetComponent<Piece>().Force == 9)
        {

            soundController.PreAttack();

            IEnumerator enumerator = IEattack(pieace, 1.0f);
            StartCoroutine(enumerator);


        } 
        else if (Force >= pieace.GetComponent<Piece>().Force && pieace.GetComponent<Piece>().Types != ItemType.Bomba)
        {
            //if (pieace.GetComponent<Player>().Types != ItemType.Soldado)
            if (Types != ItemType.Soldado)
            {
                sDebug = "Attack pieace.GetComponent<Player>().Types = " + pieace.GetComponent<Piece>().Types;
                Debug.Log(sDebug);
                //debugTotext.ShowDebug(sDebug);

                soundController.PreAttack();

                IEnumerator enumerator = IEattack(pieace, 1.0f);
                StartCoroutine(enumerator);
               
            }
            //else if (pieace.GetComponent<Player>().Types == ItemType.Soldado)
            else if (Types == ItemType.Soldado)
            {
                sDebug = "Move to " + pieace.GetComponent<Piece>().iFieldLive + " - " + "Name of house " + gameFields[iTargetField].GetComponent<GameField>().gameObject.name;

                Debug.Log(sDebug);
                //debugTotext.ShowDebug(sDebug);

                SetCurrentField(gameFields[iTargetField]);

                PlayStep();

                IEnumerator enumerator = MovetoAttack(pieace,true);
                StartCoroutine(enumerator);
                
            }
        }
        else if (Force < pieace.GetComponent<Piece>().Force)
        {            
            if (Types != ItemType.Soldado)
            {
                soundController.PreAttack();
                pieace.GetComponent<Piece>().CounterAttack(gameObject);
            }
            else if (Types == ItemType.Soldado)
            {
                SetCurrentField(gameFields[iTargetField]);

                PlayStep();

                IEnumerator enumerator = MovetoAttack(pieace,false);
                StartCoroutine(enumerator);
            }

         }
    }

    enum Rules
    {
        BandeiraBomba,
        Outros,
        Soldado
    }

    Rules GetRule()
    {
        if (Types == ItemType.Bandeira || Types == ItemType.Bomba)
        {
            return Rules.BandeiraBomba;
        }
        else if (Types == ItemType.Soldado)
        {
            return Rules.Soldado;
        }
        else
        {
            return Rules.Outros;
        }
    }

    private void PlayStep()
    {
        if (Types == ItemType.Soldado
        || (gameType != GameMode.GameType.Training && tag == "Enemy"))
        {
            soundController.Run();
        }

        soundController.Steps();
    }

    private void StopStep()
    {
        if (Types == ItemType.Soldado
        || (gameType != GameMode.GameType.Training && tag == "Enemy"))
        {
            soundController.StopRun();
        }

        soundController.StopSteps();
    }

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
        GameField[] fcs = board.GetGameFieldFromFields();

        int indexloop = 0;

        foreach (GameField fc in fcs)
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

        indexfield = IndexHouse(iFieldLive);

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

        indexfield = IndexHouse(iFieldLive);

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

    public virtual void GoToField()
    {
        if (currentField == null) return;

        transform.position = currentField.transform.position;
        transform.Rotate(0, 0, 0, Space.Self);
    }

    public virtual void GoToField(GameField field)
    {
        SetCurrentField(field);
        transform.position = currentField.transform.position;
        transform.Rotate(0, 0, 0, Space.Self);
    }

    public void MoveStart(GameField target)
    {
        if (finished) return;

        SetCurrentField(target);

        transform.LookAt(target.transform);

        PlayStep();

        StartCoroutine(Moveto());
    }

    IEnumerator Moveto()
    {
        float SpeedPlus = 0;

        if (CanRun(currentField.transform))
        {
            SpeedPlus = 1.0f;
        }

        while (!DistanceTarget(currentField.transform))
        {
            print("B");
            SetAnimation("Walk", true);
            transform.Translate(Vector3.forward * Time.deltaTime * (MoveSpeed + SpeedPlus));
            yield return null;
        }

        if (!turn.bChangeTurn)
        {
            turn.ChangeTurn();
        }
        StopStep();
        SetAnimation("Walk", false);
        
        yield return 0;
    }

    private bool CanRun(Transform target)
    {
        bool bRun = false;
        float dist;

        float MaxDist = board.GetDistance() * 2;

        if (target)
        {
            dist = Vector3.Distance(target.position, transform.position);           

            if (dist >= MaxDist)
            {
                print("CanRun - Distance: " + dist);
                bRun = true;
            }
        }

        return bRun;

    }

    IEnumerator MovetoAttack(GameObject pieace,bool attack)
    {
        if (finished) yield return null;
        
        turn.Liberate = false;
        //CancelMovement();

        Debug.Log("MovetoAttack pieace = " + pieace.name + " - attack = " + attack);

        float SpeedPlus = 0;

        if(CanRun(currentField.transform))
        {
            SpeedPlus = 1.0f;
        }

        while (!DistanceAttack(currentField.transform))
        {
            SetAnimation("Walk", true);
            transform.Translate(Vector3.forward * Time.deltaTime * (MoveSpeed + SpeedPlus));
            yield return null;
        }
        if (attack)
        {
            soundController.PreAttack();
            IEnumerator enumerator = IEattack(pieace, 1.0f);
            StartCoroutine(enumerator);
        }
        else
        {
            soundController.PreAttack();
            pieace.GetComponent<Piece>().CounterAttack(gameObject);
        }

        StopStep();
        SetAnimation("Walk", false);
        
        yield return 0;
    }

    private bool DistanceTarget(Transform target)
    {
        bool bdistance = false;
        float dist;

        if (target)
        {
            dist = Vector3.Distance(target.position, transform.position);
            if (dist <= 0.1f)            
            {
                bdistance = true;                
            }
        }

        return bdistance;

    }

    private bool DistanceAttack(Transform target)
    {
        bool bdistance = false;
        float dist;

        if (target)
        {
            dist = Vector3.Distance(target.position, transform.position);
            //print("Distance to other: " + dist);

            if (dist <= 1.5f)
            {
                bdistance = true;                
            }
        }

        return bdistance;

    }

    public void OpenChest()
    {
        AnimatorStateInfo currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
        anim.SetBool("Open", true);

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

        SetAnimation("Win", true);
        StartCoroutine(VictoryPlay(1.5f));
    }

    private IEnumerator VictoryPlay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        soundController.VictoryPeaple();
    }

    void SetAnimation(string AnimName, bool bstatus)
    {
        anim.SetBool(AnimName, bstatus);
    }

    /// <summary>
    /// Arrumar Anim para remover essa função
    /// </summary>
    void StopAttack()
    {
        if (anim)
        {
            if (anim.GetBool("Attack"))
            {
                AnimatorStateInfo currentBaseState = anim.GetCurrentAnimatorStateInfo(0);

                if (currentBaseState.IsName("Attack"))
                {
                    anim.SetBool("Attack", false);
                }
            }
        }
    }

    bool bAttack = false;

    private bool isWalk()
    {
        bool bWalk = false;

        if (anim)
        {
            AnimatorStateInfo currentBaseState = anim.GetCurrentAnimatorStateInfo(0);

            if (currentBaseState.IsName("Walk"))
            {
                bWalk = true;
            }
        }

        return bWalk;
    }

    private IEnumerator IEattack(GameObject pieace, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        turn.Liberate = false;
        //Ativa animação de ataque
        SetAnimation("Attack", true);
        bAttack = true;

        if (gameType == GameMode.GameType.Normal || gameType == GameMode.GameType.Hard)
        {
            if (tag == "Enemy")
            {
                soundController.AttackSoldier();
            }
            else
            {
                if (auAttackYell)
                {
                    auAttackYell.Play();
                }
            }
        }
        else
        {
            if (auAttackYell)
            {
                auAttackYell.Play();
            }
        }
        IEnumerator coKillEnemy = KillEnemy(pieace, 0.5f);
        StartCoroutine(coKillEnemy);
    }

    private IEnumerator KillEnemy(GameObject pieace,  float waitTime)
    {
        while (bAttack)
        {            
            print("KillEnemy = " + waitTime);            
            yield return new WaitForSeconds(waitTime);
            StartEffectAttack();
            
            if (pieace.GetComponent<Piece>().Types == ItemType.Bandeira)
            {
                pieace.GetComponent<Piece>().OpenChest();
            }
            else
            {
                gEnemyPieace.GetComponent<Piece>().SetDie();
            }
            bAttack = false;                        
        }
    }

    void StartEffectAttack()
    {
        if(AttackEffect)
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

        }
    }

    bool bTakeHome = false;    

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
            MoveStart(gameFields[iHomeField]);
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
            turn.currentePiece.GetComponent<Piece>().iFieldLive = Field;
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
        turn.currentePiece.GetComponent<Piece>().iFieldLive = Field;

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

    public void CounterAttack(GameObject pieace)
    {
        IEnumerator enumerator = IECounterAttack(pieace, 1.0f);
        StartCoroutine(enumerator);
    }

    private IEnumerator IECounterAttack(GameObject pieace, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        turn.Liberate = false;
        //Ativa animação de ataque
        SetAnimation("Attack", true);
        bAttack = true;

        if (gameType == GameMode.GameType.Normal || gameType == GameMode.GameType.Hard)
        {
            if (tag == "Enemy")
            {
                soundController.AttackSoldier();
            }
            else
            {
                if (auAttackYell)
                {
                    auAttackYell.Play();
                }
            }
        }
        else
        {
            if (auAttackYell)
            {
                auAttackYell.Play();
            }
        }
        IEnumerator coKillEnemy = CounterKillEnemy(pieace, 0.5f);
        StartCoroutine(coKillEnemy);
    }

    bool bDieCounter = false;

    private IEnumerator CounterKillEnemy(GameObject pieace, float waitTime)
    {
        while (bAttack)
        {
            gEnemyPieace = pieace;
            print("KillEnemy = " + waitTime);
            yield return new WaitForSeconds(waitTime);
            StartEffectAttack();
            gEnemyPieace.GetComponent<Piece>().SetDie();
            bAttack = false;
            gEnemyPieace.GetComponent<Piece>().EndTurnEnemy();

            if(Types == ItemType.Bomba)
            {
                bDieCounter = true;
                SetDie();
            }

            //IEnumerator enumerator = EndTurn(3.0f);
            //StartCoroutine(enumerator);
        }
    }

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

    public void LookEnemy(Transform Target)
    {
        transform.LookAt(Target);
    }    

    public void SetDie()
    {
        anim.SetBool("Die", true);        
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
        anim.SetBool("Die", true);
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
                turn.currentePiece.GetComponent<Piece>().SetTakeHome(iFieldLive);
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
                sDebug = "DestroyAfterDie - SetTakeHome iFieldLive = " + iFieldLive;
                Debug.Log(sDebug);
                //debugTotext.ShowDebug(sDebug);
                turn.currentePiece.GetComponent<Piece>().SetTakeHome(iFieldLive);
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
                turn.currentePiece.GetComponent<Piece>().SetTakeHome(iFieldLive);
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
        GameField[] fcs = board.GetGameFieldFromFields();   
        foreach (GameField fd in fcs)
        {
            if (fd.index == iFieldLive)
            {
                fd.SetPiece(null);
            }
        }
    }
}
