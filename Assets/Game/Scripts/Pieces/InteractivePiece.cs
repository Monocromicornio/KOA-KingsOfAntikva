using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Piece))]
public class InteractivePiece : MonoBehaviour
{
    protected MatchController matchController => MatchController.instance;
    protected SoundController soundController => matchController.soundController;
    protected bool finished => matchController.finished;

    private Piece _piece;
    public Piece piece 
    { 
        get 
        {
            if (_piece == null)
            {
                _piece = GetComponent<Piece>();
            }
            return _piece;
        }
        private set => _piece = value;
    }

    [SerializeField]
    protected AnimPiece anim;

    [SerializeField]
    public int force;

    [Header("Attack Timing")]
    [SerializeField]
    [Tooltip("Delay (seconds) from when the Attack trigger fires to when the losing piece starts its death sequence. Configure per piece to sync with each attack animation.")]
    protected float deathAnimationDelay = 1f;

    [SerializeField]
    [Tooltip("Total duration (seconds) of this piece's Attack animation clip. Used by FakePiece to hold Block pose during the attacker's animation before counter-attacking.")]
    protected float attackAnimationDuration = 1f;

    /// <summary>
    /// Public accessor so other pieces can read the configured delay during combat resolution.
    /// </summary>
    public float DeathAnimationDelay => deathAnimationDelay;

    protected virtual void Awake()
    {
        piece = GetComponent<Piece>();
    }

    /// <summary>
    /// Resolves combat outcome with proper sequencing.
    /// Win: reveal loser instantly, start death at deathAnimationDelay, wait for death to complete, then move.
    /// Lose: reveal self instantly, start death at winner's deathAnimationDelay, wait for death to complete, then cancel.
    /// In both cases, the turn only changes AFTER the death animation finishes.
    /// </summary>
    public virtual void Notify(bool sucess, InteractivePiece target)
    {
        if (sucess)
        {
            StartCoroutine(WinSequence(target));
        }
        else
        {
            StartCoroutine(LoseSequence(target));
        }
    }

    private IEnumerator WinSequence(InteractivePiece loser)
    {
        // Reveal enemy immediately (shows real model if disguised)
        loser.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);

        // Wait for the configured moment in the attack animation to start death
        yield return new WaitForSeconds(deathAnimationDelay);
        loser.piece.SetLose();

        // Wait for death animation to complete before moving and changing turn
        yield return new WaitForSeconds(loser.piece.timeToDestroy);
        SendMessage("Sucess");
    }

    private IEnumerator LoseSequence(InteractivePiece winner)
    {
        // Reveal self if disguised (FakePiece shows real model)
        piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);

        // Cache delay before yielding (winner's GO may be destroyed in edge cases like BombPiece)
        float winnerDelay = winner.DeathAnimationDelay;

        // Winner's delay determines when during their attack/counter-attack this piece starts dying
        yield return new WaitForSeconds(winnerDelay);
        piece.SetLose();

        // Wait for death animation to complete before canceling and changing turn
        yield return new WaitForSeconds(piece.timeToDestroy);
        SendMessage("Failed");
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

    /// <summary>
    /// Plays the pre-attack sound, waits briefly, triggers the animation, then invokes the combat action.
    /// </summary>
    protected IEnumerator FeedbackAttack(UnityAction action, string animName = "Attack")
    {
        soundController.PreAttack();
        yield return new WaitForSeconds(0.5f);
        anim.SetAnimation(animName);
        action.Invoke();
    }

    protected virtual void Attack(InteractivePiece target)
    {
        if (target == null) return;
        
        TutorialEvents.TriggerPieceAttacked(piece, target.piece);
        
        UnityAction action = () => ForceChallenge(target);
        StartCoroutine(FeedbackAttack(action));
    }

    /// <summary>
    /// Counter-attack when this piece is attacked and wins the force comparison.
    /// FakePiece: Block during enemy attack, then CounterAttack after it finishes.
    /// All other pieces: standard Attack trigger for counter-attack.
    /// </summary>
    protected virtual void CounterAttack(InteractivePiece target)
    {
        if (target == null) return;

        FakePiece fakePiece = GetComponent<FakePiece>();
        if (fakePiece != null)
        {
            StartCoroutine(FakeCounterAttackSequence(target));
            return;
        }

        // Standard counter-attack: play Attack and resolve
        UnityAction action = () => target.Notify(false, this);
        StartCoroutine(FeedbackAttack(action));
    }

    private IEnumerator FakeCounterAttackSequence(InteractivePiece attacker)
    {
        // Step 1: hold Block pose while the attacker's attack animation plays
        anim.SetAnimation("Block");

        // Step 2: wait for the attacker's attack animation to finish
        yield return new WaitForSeconds(attacker.attackAnimationDuration);

        // Step 3: launch counter-attack animation and resolve
        UnityAction action = () => attacker.Notify(false, this);
        StartCoroutine(FeedbackAttack(action, "CounterAttack"));
    }

    protected virtual void InstaKillAttack(InteractivePiece target)
    {
        if (target == null) return;
        UnityAction action = () => Notify(true, target);
        StartCoroutine(FeedbackAttack(action));
    }
}