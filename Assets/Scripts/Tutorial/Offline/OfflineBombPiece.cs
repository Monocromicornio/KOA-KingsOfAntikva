using System.Collections;
using UnityEngine;

public class OfflineBombPiece : OfflineInteractivePiece
{
    protected override void Awake()
    {
        base.Awake();
        force = int.MaxValue;
    }

    protected override void CounterAttack(OfflineInteractivePiece target)
    {
        Debug.Log($"[OfflineBombPiece] CounterAttack called! Target: {(target != null ? target.name : "null")}");
        
        if (target == null) return;
        
        Debug.Log($"[OfflineBombPiece] Triggering piece attacked event - Attacker: {piece.name}, Target: {target.piece.name}");
        TutorialEvents.TriggerPieceAttacked(piece, target.piece);
        
        SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        StartCoroutine(BombCounterAttackSequence(target));
    }

    /// <summary>
    /// Bomb explosion: kill attacker, wait for death, send Failed, then kill the bomb.
    /// The bomb kills itself LAST so the coroutine survives.
    /// </summary>
    private IEnumerator BombCounterAttackSequence(OfflineInteractivePiece target)
    {
        // Cache values before any yield
        float cachedTargetDeathDelay = target != null ? target.DeathAnimationDelay : 1f;
        float cachedDeathDuration = GetSafeTimeToDestroy(target);

        yield return new WaitForSeconds(cachedTargetDeathDelay);

        // Kill the attacker first
        if (target != null && target.piece != null)
        {
            target.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
            target.piece.SetLose();
        }

        // Wait for attacker's death animation to complete
        yield return new WaitForSeconds(cachedDeathDuration);

        // Signal turn change before killing the bomb
        SendMessage("Failed", SendMessageOptions.DontRequireReceiver);

        // Now the bomb can safely die
        piece.SetLose();
    }
}
