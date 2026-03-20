using System.Collections;
using UnityEngine;

public class BombPiece : InteractivePiece
{
    public GameObject effect;
    private bool effectSpawned;

    protected override void Awake()
    {
        base.Awake();
        force = int.MaxValue;
    }

    /// <summary>
    /// Called via SendMessage("Destroy") from Piece.OnLose().
    /// Since SetLose uses NetworkExecute, this runs on ALL clients, so the effect is visible everywhere.
    /// The effectSpawned flag prevents double-spawning when OnLose is called multiple times.
    /// </summary>
    private void Destroy()
    {
        if (effect != null && !effectSpawned)
        {
            effectSpawned = true;
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
    /// Bomb explosion sequence:
    /// 1. Wait for target's death animation delay
    /// 2. Bomb explodes (SetLose triggers Destroy → effect) and kills attacker simultaneously
    /// 3. Wait for death animations to complete
    /// 4. Change turn
    /// Runs on MatchController so the coroutine survives piece destruction.
    /// </summary>
    private IEnumerator BombCounterAttackSequence(InteractivePiece target)
    {
        // Cache all values before any yield to avoid MissingReferenceException
        float cachedTargetDeathDelay = target != null ? target.DeathAnimationDelay : 1f;
        float cachedDeathDuration = GetSafeTimeToDestroy(target);

        yield return new WaitForSeconds(cachedTargetDeathDelay);

        // Bomb explodes NOW — kill bomb first so the effect spawns at the right time
        if (this != null && piece != null)
        {
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

        // Wait for death animations to complete
        yield return new WaitForSeconds(cachedDeathDuration);

        // Change turn after everything is resolved
        matchController.ChangeTurn();
    }
}