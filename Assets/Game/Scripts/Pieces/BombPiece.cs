using UnityEngine;
using UnityEngine.Events;

public class BombPiece : InteractivePiece
{
    protected override void Awake()
    {
        base.Awake();
        force = int.MaxValue;
    }

    protected override void CounterAttack(InteractivePiece target)
    {
        if (target == null) return;
        SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        UnityAction action = () => ActionsAfterAttack(target);
        StartCoroutine(FeedbackAttack(action));
    }

    private void ActionsAfterAttack(InteractivePiece target)
    {
        target.Notify(false, this);
        piece.SetLose();
    }
}