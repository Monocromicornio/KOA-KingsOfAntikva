using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SelectablePiece))]
public class AttackPiece : InteractivePiece
{
    private SelectablePiece selectField;

    private GameField fieldAtk;
    private Piece target;

    IEnumerator posToAtk;

    [Header("Effects")]
    [SerializeField]
    protected GameObject AttackEffect;

    [SerializeField]
    protected Transform AttackEffectPos;

    protected override void Awake()
    {
        base.Awake();
        selectField = GetComponent<SelectablePiece>();
    }

    private void NewTarget()
    {
        if (finished) return;

        GameField fieldPiece = piece.targetField;
        Debug.Log($"[AttackPiece] NewTarget - targetField: {fieldPiece?.name}, hasPiece: {fieldPiece?.hasPiece}");
        if (fieldPiece == null || !fieldPiece.hasPiece)
        {
            Debug.LogWarning("[AttackPiece] Abortou: targetField nulo ou sem pe�a.");
            return;
        }

        target = fieldPiece.piece;
        fieldAtk = selectField.GetEmptyFieldFromActive(fieldPiece);
        Debug.Log($"[AttackPiece] fieldAtk: {fieldAtk?.name ?? "NULL"}, piece.field: {piece.field?.name ?? "NULL"}");
        if (fieldAtk == null) fieldAtk = piece.field;



        if (posToAtk != null) StopCoroutine(posToAtk);
        posToAtk = PositionToAttack();
        StartCoroutine(posToAtk);
    }

    private void Sucess()
    {
        GameField targetFieldToOccupy = piece.targetField;
        if (targetFieldToOccupy != null)
        {
            piece.ForceSelectField(targetFieldToOccupy);
        }
        EndAttack();
    }

    private void Failed()
    {
        CancelAttack();
    }

    private IEnumerator PositionToAttack()
    {
        const float timeout = 10f;
        float elapsed = 0f;
        const float distanceThreshold = 0.1f;

        Debug.Log($"[AttackPiece:{name}] PositionToAttack started. Moving to fieldAtk: {fieldAtk?.name ?? "NULL"}, target: {target?.name ?? "NULL"}");

        while (Vector3.Distance(transform.position, fieldAtk.transform.position) > distanceThreshold)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogError($"[AttackPiece:{name}] Timeout: piece did not reach fieldAtk after {timeout}s. Forcing turn change as fallback.");
                matchController.ChangeTurn();
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }

        Debug.Log($"[AttackPiece:{name}] Reached fieldAtk after {elapsed:F2}s. Starting attack on {target?.name ?? "NULL"}.");
        transform.LookAt(target.transform);
        InteractivePiece combatTarget = GetCombatPiece();
        ReadyToAttack(combatTarget);
    }

    protected InteractivePiece GetCombatPiece()
    {
        InteractivePiece combatTarget = target.GetComponent<InteractivePiece>();
        if (combatTarget == null)
        {
            CancelAttack();
            return null;
        }
        return combatTarget;
    }

    protected virtual void ReadyToAttack(InteractivePiece combatTarget)
    {
        Attack(combatTarget);
    }

    private void CancelAttack()
    {
        piece.ForceSelectField(fieldAtk);
        EndAttack();
    }

    private void EndAttack()
    {
        target = null;
        fieldAtk = null;
    }
}