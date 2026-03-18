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

    private IEnumerator BombCounterAttackSequence(OfflineInteractivePiece target)
    {
        // Wait for the configured delay before exploding
        yield return new WaitForSeconds(target.DeathAnimationDelay);

        target.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        target.piece.SetLose();
        piece.SetLose();

        // Wait for death animations to complete before changing turn
        yield return new WaitForSeconds(piece.timeToDestroy);
        SendMessage("Failed", SendMessageOptions.DontRequireReceiver);
    }
}
