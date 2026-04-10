using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Piece))]
public class InteractivePiece : MonoBehaviour
{
    private const float DEFAULT_TIME_TO_DESTROY = 3.5f;

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
    /// Called when this piece wins a combat (attacker that won).
    /// Runs WinSequence on MatchController so the coroutine survives even if both pieces are destroyed.
    /// </summary>
    public virtual void Notify(bool sucess, InteractivePiece target)
    {
        if (sucess)
        {
            matchController.StartCoroutine(WinSequence(target));
        }
    }

    /// <summary>
    /// Attacker won: reveal loser, wait deathAnimationDelay, kill loser,
    /// wait for death animation, then move to the target field.
    /// Runs on MatchController so the coroutine is never killed by piece destruction.
    /// </summary>
    private IEnumerator WinSequence(InteractivePiece loser)
    {
        // Cache references and values before any yield to avoid MissingReferenceException
        InteractivePiece winner = this;
        float cachedDeathAnimDelay = deathAnimationDelay;
        float cachedDeathDuration = GetSafeTimeToDestroy(loser);

        if (loser != null && loser.piece != null)
        {
            loser.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(cachedDeathAnimDelay);

        if (loser != null && loser.piece != null)
        {
            loser.piece.SetLose();
        }
        else
        {
            Debug.LogWarning("[InteractivePiece] WinSequence: loser already destroyed before SetLose.");
        }

        yield return new WaitForSeconds(cachedDeathDuration);

        if (winner != null)
        {
            winner.SendMessage("Sucess", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("[InteractivePiece] WinSequence: winner destroyed — calling ChangeTurn as fallback.");
            matchController.ChangeTurn();
        }
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
    /// FakePiece: Block during enemy attack, then CounterAttack.
    /// All other pieces: standard Attack trigger.
    /// Combat coroutines run on MatchController so they survive piece destruction.
    /// </summary>
    protected virtual void CounterAttack(InteractivePiece target)
    {
        if (target == null) return;

        FakePiece fakePiece = GetComponent<FakePiece>();
        if (fakePiece != null)
        {
            Debug.Log("[InteractivePiece] COUNTER ATTACK! FakePiece detected, calling fake counter attack sequence");
            matchController.StartCoroutine(FakeCounterAttackSequence(target));
            return;
        }

        // Standard counter-attack: play Attack on this piece, then handle loser death on MatchController
        Debug.Log("[InteractivePiece] COUNTER ATTACK! Reveled Piece detected, calling default attack sequence");
        UnityAction action = () => matchController.StartCoroutine(HandleLoserDeath(target));
        StartCoroutine(FeedbackAttack(action));
    }

    /// <summary>
    /// Handles the loser's death from a coroutine on MatchController.
    /// Reveal, wait, kill, wait for death animation, then change turn.
    /// </summary>
    private IEnumerator HandleLoserDeath(InteractivePiece loser)
    {
        // Cache values before any yield
        float cachedDeathAnimDelay = deathAnimationDelay;
        float cachedDeathDuration = GetSafeTimeToDestroy(loser);

        if (loser != null && loser.piece != null)
        {
            loser.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(cachedDeathAnimDelay);

        if (loser != null && loser.piece != null)
        {
            loser.piece.SetLose();
        }
        else
        {
            Debug.LogWarning("[InteractivePiece] HandleLoserDeath: loser already destroyed before SetLose.");
        }

        yield return new WaitForSeconds(cachedDeathDuration);

        Debug.Log($"[InteractivePiece:{name}] HandleLoserDeath — calling matchController.ChangeTurn().");
        matchController.ChangeTurn();
    }

    private IEnumerator FakeCounterAttackSequence(InteractivePiece attacker)
    {
        // Cache all values before any yield
        InteractivePiece defender = this;
        float cachedAttackAnimDuration = attacker != null ? attacker.attackAnimationDuration : 1f;
        float cachedDeathAnimDelay = deathAnimationDelay;
        float cachedDeathDuration = GetSafeTimeToDestroy(attacker);

        // Step 1: hold Block pose while the attacker's attack animation plays
        if (defender != null && anim != null) anim.SetAnimation("Block");

        // Step 2: wait for the attacker's attack animation to finish
        yield return new WaitForSeconds(cachedAttackAnimDuration);

        // Step 3: launch counter-attack animation
        if (defender != null)
        {
            soundController.PreAttack();
            yield return new WaitForSeconds(0.5f);
            Debug.Log("[InteractivePiece] COUNTER ATTACK! Inside coroutine on piece " + gameObject.name);
            if (anim != null) anim.SetAnimation("CounterAttack");
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Step 4: handle loser death
        if (attacker != null && attacker.piece != null)
        {
            attacker.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(cachedDeathAnimDelay);

        if (attacker != null && attacker.piece != null)
        {
            attacker.piece.SetLose();
        }
        else
        {
            Debug.LogWarning("[InteractivePiece] FakeCounterAttack: attacker already destroyed before SetLose.");
        }

        yield return new WaitForSeconds(cachedDeathDuration);

        Debug.Log($"[InteractivePiece:{name}] FakeCounterAttackSequence — calling matchController.ChangeTurn().");
        matchController.ChangeTurn();
    }

    protected virtual void InstaKillAttack(InteractivePiece target)
    {
        if (target == null) return;
        UnityAction action = () => Notify(true, target);
        StartCoroutine(FeedbackAttack(action));
    }

    /// <summary>
    /// Safely reads timeToDestroy from a piece, returning a default if the piece was already destroyed.
    /// </summary>
    protected static float GetSafeTimeToDestroy(InteractivePiece interactivePiece)
    {
        if (interactivePiece != null && interactivePiece.piece != null)
        {
            return interactivePiece.piece.timeToDestroy;
        }
        return DEFAULT_TIME_TO_DESTROY;
    }
}