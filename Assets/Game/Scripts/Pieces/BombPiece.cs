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

    protected override void CounterAttack(InteractivePiece target)
    {
        if (target == null) return;
        SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        StartCoroutine(BombCounterAttackSequence(target));
    }

    private IEnumerator BombCounterAttackSequence(InteractivePiece target)
    {
        // Wait for the configured delay before exploding
        yield return new WaitForSeconds(target.DeathAnimationDelay);

        if (effect != null)
        {
            Instantiate(effect, transform.position, effect.transform.rotation);
        }

        target.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        target.piece.SetLose();
        piece.SetLose();

        // Wait for death animations to complete before changing turn
        yield return new WaitForSeconds(piece.timeToDestroy);
        SendMessage("Failed");
    }
}