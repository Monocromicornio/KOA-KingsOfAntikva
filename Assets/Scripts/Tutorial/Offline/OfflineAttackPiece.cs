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
        GameField fieldPiece = piece.targetField;
        if (fieldPiece == null || !fieldPiece.hasPiece) return;

        target = fieldPiece.offlinePiece != null ? fieldPiece.offlinePiece : fieldPiece.piece?.GetComponent<OfflinePiece>();
        if (target == null)
        {
            Debug.LogWarning("Target não tem OfflinePiece!");
            return;
        }
        
        fieldAtk = selectField.GetEmptyFieldFromActive(fieldPiece);

        if (fieldAtk == null) fieldAtk = piece.field;

        if (posToAtk != null) StopCoroutine(posToAtk);
        posToAtk = PositionToAttack();
        StartCoroutine(posToAtk);
    }

    private void Success()
    {
        GameField targetFieldToOccupy = piece.targetField;
        if (targetFieldToOccupy != null)
        {
            piece.SelectedAField(targetFieldToOccupy);
        }
        EndAttack();
    }

    private void Failed()
    {
        CancelAttack();
    }

    private IEnumerator PositionToAttack()
    {
        const float distanceThreshold = 0.1f;
        while (Vector3.Distance(transform.position, fieldAtk.transform.position) > distanceThreshold)
        {
            yield return new WaitForEndOfFrame();
        }

        transform.LookAt(target.transform);
        OfflineInteractivePiece combatTarget = GetCombatPiece();
        ReadyToAttack(combatTarget);
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
        if (combatTarget != null)
        {
            Attack(combatTarget);
        }
    }

    private void CancelAttack()
    {
        piece.SelectedAField(fieldAtk);
        EndAttack();
    }

    private void EndAttack()
    {
        target = null;
        fieldAtk = null;
    }
}
