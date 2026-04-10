using System.Collections;
using UnityEngine;

public class BombPiece : InteractivePiece
{
    public GameObject effect;
    private bool hasExploded;

    protected override void Awake()
    {
        base.Awake();
        force = int.MaxValue;
    }

    /// <summary>
    /// Called via SendMessage("Explode") from Piece.TriggerExplosion().
    /// Only runs during counter-attack (TriggerExplosion is only called from BombCounterAttackSequence).
    /// Uses hasExploded flag to prevent double-spawning from NetworkExecute + NetworkExecuteOnClient.
    /// </summary>
    private void Explode()
    {
        if (effect != null && !hasExploded)
        {
            hasExploded = true;
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
    /// Bomb counter-attack explosion sequence:
    /// 1. Wait for target's death animation delay
    /// 2. Trigger explosion effect on all clients via Piece.TriggerExplosion (network synced)
    /// 3. Both pieces die simultaneously
    /// 4. Wait for death animations to complete
    /// 5. Change turn
    /// Runs on MatchController so the coroutine survives piece destruction.
    /// </summary>
    private IEnumerator BombCounterAttackSequence(InteractivePiece target)
    {
        float cachedTargetDeathDelay = target != null ? target.DeathAnimationDelay : 1f;
        float cachedDeathDuration = GetSafeTimeToDestroy(target);

        yield return new WaitForSeconds(cachedTargetDeathDelay);

        // Trigger explosion on ALL clients via network before killing the bomb
        if (this != null && piece != null)
        {
            piece.TriggerExplosion();
            piece.SetLose();
        }

        // Kill the attacker
        if (target != null && target.piece != null)
        {
            target.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
            target.piece.SetLose();
        }
        else
        {
            Debug.LogWarning("[BombPiece] BombCounterAttack: target already destroyed before SetLose.");
        }

        yield return new WaitForSeconds(cachedDeathDuration);

        Debug.Log($"[BombPiece:{name}] BombCounterAttackSequence — calling matchController.ChangeTurn().");
        matchController.ChangeTurn();
    }
}