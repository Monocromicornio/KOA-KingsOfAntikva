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
        matchController.StartCoroutine(BombCounterAttackSequence(target));
    }

    /// <summary>
    /// Bomb counter-attack explosion sequence:
    /// 1. Wait for target's death animation delay
    /// 2. Spawn explosion effect (ONLY during counter-attack, not when killed by Desarmador)
    /// 3. Both pieces die simultaneously
    /// 4. Wait for death animations to complete
    /// 5. Change turn
    /// Runs on MatchController so the coroutine survives piece destruction.
    /// </summary>
    private IEnumerator BombCounterAttackSequence(InteractivePiece target)
    {
        // Cache all values before any yield to avoid MissingReferenceException
        float cachedTargetDeathDelay = target != null ? target.DeathAnimationDelay : 1f;
        float cachedDeathDuration = GetSafeTimeToDestroy(target);
        GameObject cachedEffect = effect;
        Vector3 cachedPosition = transform.position;
        Quaternion cachedEffectRotation = cachedEffect != null ? cachedEffect.transform.rotation : Quaternion.identity;

        yield return new WaitForSeconds(cachedTargetDeathDelay);

        // Spawn explosion effect at the right moment
        if (cachedEffect != null)
        {
            Instantiate(cachedEffect, cachedPosition, cachedEffectRotation);
        }

        // Both pieces die simultaneously
        if (this != null && piece != null)
        {
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

        // Wait for death animations to complete
        yield return new WaitForSeconds(cachedDeathDuration);

        // Change turn after everything is resolved
        matchController.ChangeTurn();
    }
}