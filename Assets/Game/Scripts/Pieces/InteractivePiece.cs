using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public virtual void Hitted(InteractivePiece target, int force)
    {
        if (this.force < force)
        {
            var options = SendMessageOptions.DontRequireReceiver;
            SendMessage("Destroy", options);
            target.SendMessage("Sucess", options);
            return;
        }

        Kill(target);
    }

    private IEnumerator FeedbackAttack(InteractivePiece target, int force)
    {
        yield return new WaitForSeconds(1);
        anim.SetAnimation("Attack");
        target.Hitted(this, force);
    }

    protected void Attack(InteractivePiece target)
    {
        if (target == null) return;
        soundController.PreAttack();
        StartCoroutine(FeedbackAttack(target, force));
    }

    protected void Kill(InteractivePiece target)
    {
        if (target == null) return;
        soundController.PreAttack();
        StartCoroutine(FeedbackAttack(target, int.MaxValue));
    }
}