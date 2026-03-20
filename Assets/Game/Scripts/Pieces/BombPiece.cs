using System.Collections;
using UnityEngine;

public class BombPiece : InteractivePiece
{
    public GameObject effect;

    protected override void Awake()
    {
        base.Awake();
        force = int.MaxValue;
    }

    /// <summary>
    /// Called via SendMessage("Destroy") from Piece.OnLose().
    /// Since SetLose uses NetworkExecute, this runs on ALL clients, so the effect is visible everywhere.
    /// </summary>
    private void Destroy()
    {
        if (effect != null)
        {
            Instantiate(effect, transform.position, effect.transform.rotation);
        }
    }

    protected override void CounterAttack(InteractivePiece target)
    {
        if (target == null) return;
        SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        matchController.StartCoroutine(BombCounterAttackSequence(target));
    }

    /// <summary>
    /// Bomb explosion: wait for deathAnimationDelay, kill attacker,
    /// wait for death animation, change turn, then kill the bomb.
    /// Runs on MatchController so the coroutine survives even if both pieces are destroyed.
    /// The explosion effect is handled by Destroy() via network-synced SendMessage.
    /// </summary>
    private IEnumerator BombCounterAttackSequence(InteractivePiece target)
    {
        // Cache all values before any yield to avoid MissingReferenceException
        float cachedTargetDeathDelay = target != null ? target.DeathAnimationDelay : 1f;
        float cachedDeathDuration = GetSafeTimeToDestroy(target);

        yield return new WaitForSeconds(cachedTargetDeathDelay);

        // Kill the attacker first
        if (target != null && target.piece != null)
        {
            target.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
            target.piece.SetLose();
        }
        else
        {
            Debug.LogWarning("[BombPiece] BombCounterAttack: target already destroyed before SetLose.");
        }

        // Wait for attacker's death animation to complete
        yield return new WaitForSeconds(cachedDeathDuration);

        // Change turn BEFORE killing the bomb
        matchController.ChangeTurn();

        // Now the bomb can safely die — Destroy() will spawn the effect on all clients
        if (this != null && piece != null)
        {
            piece.SetLose();
        }
    }
}