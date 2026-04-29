using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OfflinePiece))]
public class OfflineInteractivePiece : MonoBehaviour
{
    private const float DEFAULT_TIME_TO_DESTROY = 3.5f;

    private OfflinePiece _piece;
    public OfflinePiece piece 
    { 
        get 
        {
            if (_piece == null)
            {
                _piece = GetComponent<OfflinePiece>();
            }
            return _piece;
        }
        private set => _piece = value;
    }

    [SerializeField]
    protected OfflineAnimPiece anim;

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
        piece = GetComponent<OfflinePiece>();
    }

    /// <summary>
    /// Called when this piece wins a combat (attacker that won).
    /// Runs WinSequence on this piece (the winner), so the coroutine is never killed.
    /// </summary>
    public virtual void Notify(bool success, OfflineInteractivePiece target)
    {
        if (success)
        {
            StartCoroutine(WinSequence(target));
        }
    }

    private IEnumerator WinSequence(OfflineInteractivePiece loser)
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

        yield return new WaitForSeconds(cachedDeathDuration);
        SendMessage("Success", SendMessageOptions.DontRequireReceiver);
    }

    protected virtual void ForceChallenge(OfflineInteractivePiece target)
    {
        if (force >= target.force)
        {
            Notify(true, target);
            return;
        }

        target.CounterAttack(this);
    }

    /// <summary>
    /// Plays pre-attack delay, triggers the animation, then invokes the combat action.
    /// </summary>
    protected IEnumerator FeedbackAttack(UnityAction action, string animName = "Attack")
    {
        yield return new WaitForSeconds(0.5f);
        
        if (anim != null)
        {
            anim.PlayAttackSound();
            anim.SetAnimation(animName);
        }
        
        action.Invoke();
    }

    protected virtual void Attack(OfflineInteractivePiece target)
    {
        Debug.Log($"[OfflineInteractivePiece] Attack called! Attacker: {piece.name}, Target: {(target != null ? target.name : "null")}");
        
        if (target == null) return;
        
        TutorialEvents.TriggerPieceAttacked(piece, target.piece);
        
        UnityAction action = () => ForceChallenge(target);
        StartCoroutine(FeedbackAttack(action));
    }

    /// <summary>
    /// Counter-attack when this piece is attacked and wins the force comparison.
    /// All death handling runs on THIS (the winner) so the coroutine is never killed.
    /// </summary>
    protected virtual void CounterAttack(OfflineInteractivePiece target)
    {
        if (target == null) return;

        OfflineFakePiece fakePiece = GetComponent<OfflineFakePiece>();
        if (fakePiece != null)
        {
            StartCoroutine(FakeCounterAttackSequence(target));
            return;
        }

        // Standard counter-attack: play Attack, then handle loser death
        UnityAction action = () => StartCoroutine(HandleLoserDeath(target));
        StartCoroutine(FeedbackAttack(action));
    }

    /// <summary>
    /// Handles the loser's death entirely from the winner's coroutine.
    /// </summary>
    private IEnumerator HandleLoserDeath(OfflineInteractivePiece loser)
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

        yield return new WaitForSeconds(cachedDeathDuration);

        SendMessage("Failed", SendMessageOptions.DontRequireReceiver);
    }

    private IEnumerator FakeCounterAttackSequence(OfflineInteractivePiece attacker)
    {
        // Cache all values before any yield
        float cachedAttackAnimDuration = attacker != null ? attacker.attackAnimationDuration : 1f;
        float cachedDeathAnimDelay = deathAnimationDelay;
        float cachedDeathDuration = GetSafeTimeToDestroy(attacker);

        if (anim != null) anim.SetAnimation("Block");

        yield return new WaitForSeconds(cachedAttackAnimDuration);

        // Counter-attack animation
        yield return new WaitForSeconds(0.5f);
        if (anim != null)
        {
            anim.PlayAttackSound();
            anim.SetAnimation("CounterAttack");
        }

        // Handle loser death from THIS (winner) coroutine
        if (attacker != null && attacker.piece != null)
        {
            attacker.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(cachedDeathAnimDelay);

        if (attacker != null && attacker.piece != null)
        {
            attacker.piece.SetLose();
        }

        yield return new WaitForSeconds(cachedDeathDuration);

        SendMessage("Failed", SendMessageOptions.DontRequireReceiver);
    }

    protected virtual void InstaKillAttack(OfflineInteractivePiece target)
    {
        Debug.Log($"[OfflineInteractivePiece] InstaKillAttack called! Attacker: {piece.name}, Target: {(target != null ? target.name : "null")}");
        
        if (target == null) return;
        
        TutorialEvents.TriggerPieceAttacked(piece, target.piece);
        
        UnityAction action = () => Notify(true, target);
        StartCoroutine(FeedbackAttack(action));
    }

    /// <summary>
    /// Safely reads timeToDestroy from a piece, returning a default if the piece was already destroyed.
    /// </summary>
    protected static float GetSafeTimeToDestroy(OfflineInteractivePiece interactivePiece)
    {
        if (interactivePiece != null && interactivePiece.piece != null)
        {
            return interactivePiece.piece.timeToDestroy;
        }
        return DEFAULT_TIME_TO_DESTROY;
    }
}
