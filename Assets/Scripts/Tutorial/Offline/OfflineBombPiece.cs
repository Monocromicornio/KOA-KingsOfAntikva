using UnityEngine;
using UnityEngine.Events;

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
        UnityAction action = () => ActionsAfterAttack(target);
        StartCoroutine(FeedbackAttack(action));
    }

    private void ActionsAfterAttack(OfflineInteractivePiece target)
    {
        target.Notify(false, this);
        piece.SetLose();
    }
}
