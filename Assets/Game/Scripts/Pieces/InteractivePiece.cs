using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Piece))]
public class InteractivePiece : MonoBehaviour
{
    protected MatchController matchController => MatchController.instance;
    protected SoundController soundController => matchController.soundController;
    protected bool finished => matchController.finished;

    public Piece piece { get; private set; }

    //Inspector
    [SerializeField]
    protected AnimPiece anim;

    [SerializeField]
    public int force;

    [Header("Effects")]
    [SerializeField]
    protected GameObject AttackEffect;

    [SerializeField]
    protected Transform AttackEffectPos;

    protected virtual void Awake()
    {
        piece = GetComponent<Piece>();
    }

    protected virtual void Notify(bool sucess, InteractivePiece target)
    {
        SendMessage(sucess? "Sucess": "Failed");
        GameObject toDestroy = sucess ? target.gameObject : gameObject;
        toDestroy.SendMessage("Destroy", SendMessageOptions.DontRequireReceiver);
    }

    protected virtual void ForceChallenge(InteractivePiece target)
    {
        if (force >= target.force)
        {
            Notify(true, target);
            return;
        }

        target.CounterAttack(this);
    }

    private IEnumerator FeedbackAttack(UnityAction action)
    {
        soundController.PreAttack();
        yield return new WaitForSeconds(1);
        anim.SetAnimation("Attack");
        action.Invoke();
    }

    protected void Attack(InteractivePiece target)
    {
        if (target == null) return;
        UnityAction action = () => ForceChallenge(target);
        StartCoroutine(FeedbackAttack(action));
    }

    protected void CounterAttack(InteractivePiece target)
    {
        if (target == null) return;
        UnityAction action = () => target.Notify(false, this);
        StartCoroutine(FeedbackAttack(action));
    }

    protected void InstaKillAttack(InteractivePiece target)
    {
        if (target == null) return;
        UnityAction action = () => Notify(true, target);
        StartCoroutine(FeedbackAttack(action));
    }
}