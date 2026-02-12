using System.Collections;
using UnityEngine;

[RequireComponent(typeof(OfflineSelectablePiece))]
public class OfflineAttackPiece : OfflineInteractivePiece
{
    private OfflineSelectablePiece selectField;

    private GameField fieldAtk;
    private OfflinePiece target;

    IEnumerator posToAtk;

    [Header("Effects")]
    [SerializeField]
    protected GameObject AttackEffect;

    [SerializeField]
    protected Transform AttackEffectPos;

    protected override void Awake()
    {
        base.Awake();
        selectField = GetComponent<OfflineSelectablePiece>();
    }

    public void NewTarget()
    {
        Debug.Log($"[OfflineAttackPiece] NewTarget called on {piece.name}");
        
        GameField fieldPiece = piece.targetField;
        if (fieldPiece == null)
        {
            Debug.LogWarning("[OfflineAttackPiece] targetField is null!");
            return;
        }
        
        Debug.Log($"[OfflineAttackPiece] Target field index: {fieldPiece.index}, hasPiece: {fieldPiece.hasPiece}");
        
        if (!fieldPiece.hasPiece)
        {
            Debug.LogWarning($"[OfflineAttackPiece] Field {fieldPiece.index} has no piece!");
            return;
        }

        Debug.Log($"[OfflineAttackPiece] Field has - piece: {fieldPiece.piece?.name ?? "null"}, offlinePiece: {fieldPiece.offlinePiece?.name ?? "null"}");

        target = fieldPiece.offlinePiece != null ? fieldPiece.offlinePiece : fieldPiece.piece?.GetComponent<OfflinePiece>();
        if (target == null)
        {
            Debug.LogWarning($"[OfflineAttackPiece] Target doesn't have OfflinePiece! Field has piece: {fieldPiece.piece != null}, offlinePiece: {fieldPiece.offlinePiece != null}");
            return;
        }
        
        Debug.Log($"[OfflineAttackPiece] Target found: {target.name}, color: {target.pieceColor}");
        
        fieldAtk = selectField.GetEmptyFieldFromActive(fieldPiece);

        if (fieldAtk == null) fieldAtk = piece.field;

        Debug.Log($"[OfflineAttackPiece] Attack field: {fieldAtk.index}, Starting position coroutine");

        if (posToAtk != null) StopCoroutine(posToAtk);
        posToAtk = PositionToAttack();
        StartCoroutine(posToAtk);
    }

    private void Success()
    {
        GameField targetFieldToOccupy = piece.targetField;
        if (targetFieldToOccupy != null)
        {
            piece.ForceSelectField(targetFieldToOccupy);
        }
        piece.ResetTurnAction();
        EndAttack();
    }

    private void Failed()
    {
        piece.ResetTurnAction();
        CancelAttack();
    }

    private IEnumerator PositionToAttack()
    {
        Debug.Log($"[OfflineAttackPiece] PositionToAttack started. Moving to field {fieldAtk.index}");
        
        const float distanceThreshold = 0.1f;
        while (Vector3.Distance(transform.position, fieldAtk.transform.position) > distanceThreshold)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log($"[OfflineAttackPiece] Reached attack position. Looking at target and initiating combat");
        
        transform.LookAt(target.transform);
        
        Debug.Log($"[OfflineAttackPiece] Getting combat piece component from target: {target?.name ?? "null"}");
        OfflineInteractivePiece combatTarget = GetCombatPiece();
        
        Debug.Log($"[OfflineAttackPiece] Combat target obtained: {combatTarget?.name ?? "null"}. Calling ReadyToAttack");
        ReadyToAttack(combatTarget);
        
        Debug.Log($"[OfflineAttackPiece] ReadyToAttack completed");
    }

    protected OfflineInteractivePiece GetCombatPiece()
    {
        OfflineInteractivePiece combatTarget = target.GetComponent<OfflineInteractivePiece>();
        if (combatTarget == null)
        {
            CancelAttack();
            return null;
        }
        return combatTarget;
    }

    protected virtual void ReadyToAttack(OfflineInteractivePiece combatTarget)
    {
        Debug.Log($"[OfflineAttackPiece] ReadyToAttack - Target: {combatTarget?.name ?? "null"}");
        
        if (combatTarget != null)
        {
            Debug.Log($"[OfflineAttackPiece] Calling Attack on target");
            Attack(combatTarget);
        }
        else
        {
            Debug.LogWarning("[OfflineAttackPiece] Combat target is null, cannot attack!");
        }
    }

    private void CancelAttack()
    {
        piece.ForceSelectField(fieldAtk);
        piece.ResetTurnAction();
        EndAttack();
    }

    private void EndAttack()
    {
        target = null;
        fieldAtk = null;
    }
}
