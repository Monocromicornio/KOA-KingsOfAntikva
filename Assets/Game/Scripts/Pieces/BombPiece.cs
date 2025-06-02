using UnityEngine.Events;

public class BombPiece : InteractivePiece
{
    void Start()
    {
        force = int.MaxValue;
    }

    protected override void CounterAttack(InteractivePiece target)
    {
        if (target == null) return;
        UnityAction action = () => ActionsAfterAttack(target);
        StartCoroutine(FeedbackAttack(action));
    }

    private void ActionsAfterAttack(InteractivePiece target)
    {
        target.Notify(false, this);
        SendMessage("Destroy");
    }
}