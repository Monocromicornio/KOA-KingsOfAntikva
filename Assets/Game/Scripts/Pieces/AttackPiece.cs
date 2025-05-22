using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Piece))]
[RequireComponent(typeof(SelectField))]
public class AttackPiece : MonoBehaviour
{
    private MatchController matchController => MatchController.instance;
    private SoundController soundController => matchController.soundController;
    private bool finished => matchController.finished;

    private Piece piece;
    private SelectField selectField;

    private GameField fieldAtk;
    private Piece target;

    [SerializeField]
    GameObject AttackEffect;

    [SerializeField]
    GameObject AttackEffectSoldier;

    [SerializeField]
    Transform AttackEffectPos;

    void Awake()
    {
        piece = GetComponent<Piece>();
        selectField = GetComponent<SelectField>();
    }

    void Update()
    {
        StopAttack();
    }

    public void NewTarget()
    {
        if (finished) return;

        GameField fieldPiece = piece.targetField;
        if (!fieldPiece.hasPiece) return;

        fieldAtk = selectField.GetEmptyFieldFromActive(fieldPiece);
        target = fieldPiece.piece;
    }

    //gpieace = atacante - piece = alvo
    public void AttackRules(Piece target)
    {
        transform.LookAt(target.transform);

        //if (target.Types != ItemType.Bomba || target.Types != ItemType.Bandeira)
        {
            transform.LookAt(target.transform);
        }

        //if (target.Types == ItemType.Bandeira)
        {
            soundController.PreAttack();
            StartCoroutine(FeedbackAtk(target, 1.0f));
        }
        //else if (this.Types == ItemType.Antibomba && target.Types == ItemType.Bomba)
        {
            soundController.PreAttack();
            StartCoroutine(FeedbackAtk(target, 1.0f));
        }
        //else if (this.Types != ItemType.Antibomba && target.Types == ItemType.Bomba)
        {
            soundController.PreAttack();
            target.GetComponent<AttackPiece>().CounterAttack(piece);
        }
        //else if (this.Types == ItemType.Espia && target.Force == 9)
        {
            soundController.PreAttack();
            StartCoroutine(FeedbackAtk(target, 1.0f));
        }
        //else if (this.Force >= target.Force && target.Types != ItemType.Bomba)
        {
            soundController.PreAttack();
            StartCoroutine(FeedbackAtk(target, 1.0f));
        }
        //else if (this.Force < target.Force)
        {
            soundController.PreAttack();
            target.GetComponent<AttackPiece>().CounterAttack(piece);
        }
    }

    private IEnumerator FeedbackAtk(Piece pieace, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //turn.Liberate = false;
        pieace.SetAnimation("Attack", true);

        /*if (gameType == GameMode.GameType.Normal || gameType == GameMode.GameType.Hard)
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
        }*/
        StartCoroutine(KillEnemy(pieace, 0.5f));
    }

    private IEnumerator KillEnemy(Piece pieace, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartEffectAttack();

        //if (pieace.Types == ItemType.Bandeira)
        {
            pieace.OpenChest();
        }
        //else
        {
            target.SetDie();
        }
    }

    void StartEffectAttack()
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
    }

    void StopAttack()
    {
        /*if (anim)
        {
            if (anim.GetBool("Attack"))
            {
                AnimatorStateInfo currentBaseState = anim.GetCurrentAnimatorStateInfo(0);

                if (currentBaseState.IsName("Attack"))
                {
                    anim.SetBool("Attack", false);
                }
            }
        }*/
    }

    public void CounterAttack(Piece pieace)
    {
        StartCoroutine(IECounterAttack(pieace, 1.0f));
    }

    private IEnumerator IECounterAttack(Piece pieace, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //turn.Liberate = false;

        pieace.SetAnimation("Attack", true);

        /*if (gameType == GameMode.GameType.Normal || gameType == GameMode.GameType.Hard)
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
        }*/
        StartCoroutine(CounterKillEnemy(pieace, 0.5f));
    }
    
    private IEnumerator CounterKillEnemy(Piece pieace, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartEffectAttack();
        pieace.SetDie();
        pieace.EndTurnEnemy();

        /*if(Types == ItemType.Bomba)
        {
            SetDie();
        }*/
    }
}