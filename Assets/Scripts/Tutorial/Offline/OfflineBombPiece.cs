using System.Collections;
using UnityEngine;

public class OfflineBombPiece : OfflineInteractivePiece
{
    [Header("Bomb")]
    [Tooltip("Explosion VFX prefab instantiated when the bomb detonates.")]
    public GameObject effect;

    private bool hasExploded;

    protected override void Awake()
    {
        base.Awake();
        force = int.MaxValue;
    }

    /// <summary>
    /// Instantiates the explosion effect at the bomb's position. Called once per bomb lifecycle.
    /// </summary>
    private void Explode()
    {
        if (effect != null && !hasExploded)
        {
            hasExploded = true;
            Instantiate(effect, transform.position, effect.transform.rotation);
        }
    }

    protected override void CounterAttack(OfflineInteractivePiece target)
    {
        Debug.Log($"[OfflineBombPiece] CounterAttack called! Target: {(target != null ? target.name : "null")}");
        
        if (target == null) return;
        
        TutorialEvents.TriggerPieceAttacked(piece, target.piece);
        
        SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
        StartCoroutine(BombCounterAttackSequence(target));
    }

    /// <summary>
    /// Bomb explosion sequence: explode, kill attacker, wait for death, then kill the bomb.
    /// Mirrors the online BombPiece flow.
    /// </summary>
    private IEnumerator BombCounterAttackSequence(OfflineInteractivePiece target)
    {
        float cachedTargetDeathDelay = target != null ? target.DeathAnimationDelay : 1f;
        float cachedDeathDuration = GetSafeTimeToDestroy(target);

        yield return new WaitForSeconds(cachedTargetDeathDelay);

        // Trigger explosion VFX and sound
        Explode();

        // Kill the bomb first (mirrors online order)
        piece.SetLose();

        // Kill the attacker
        if (target != null && target.piece != null)
        {
            target.piece.SendMessage("Reveal", SendMessageOptions.DontRequireReceiver);
            target.piece.SetLose();
        }

        yield return new WaitForSeconds(cachedDeathDuration);

        SendMessage("Failed", SendMessageOptions.DontRequireReceiver);
    }
}
