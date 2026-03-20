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
    /// Bomb explosion: wait for deathAnimationDelay, explode, kill attacker,
    /// wait for death animation, change turn, then kill the bomb.
    /// Runs on MatchController so the coroutine survives even if both pieces are destroyed.
    /// </summary>
    private IEnumerator BombCounterAttackSequence(InteractivePiece target)
    {
        // Cache all values before any yield to avoid MissingReferenceException
        BombPiece bomb = this;
        float cachedTargetDeathDelay = target != null ? target.DeathAnimationDelay : 1f;
        float cachedDeathDuration = GetSafeTimeToDestroy(target);
        Vector3 cachedBombPosition = transform.position;
        Quaternion cachedEffectRotation = (effect != null) ? effect.transform.rotation : Quaternion.identity;
        GameObject cachedEffect = effect;

        yield return new WaitForSeconds(cachedTargetDeathDelay);

        if (cachedEffect != null)
        {
            Instantiate(cachedEffect, cachedBombPosition, cachedEffectRotation);
        }

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

        // Now the bomb can safely die
        if (bomb != null && bomb.piece != null)
        {
            bomb.piece.SetLose();
        }
    }
}