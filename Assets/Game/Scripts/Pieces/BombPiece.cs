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

    private IEnumerator BombCounterAttackSequence(InteractivePiece target)
    {
        float cachedTargetDeathDelay = target != null ? target.DeathAnimationDelay : 1f;
        float cachedDeathDuration = GetSafeTimeToDestroy(target);

        yield return new WaitForSeconds(cachedTargetDeathDelay);
                
        if (this != null && piece != null)
        {
            piece.TriggerExplosion();
            piece.SetLose();
        }
      
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