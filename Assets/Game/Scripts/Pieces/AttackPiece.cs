using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SelectablePiece))]
public class AttackPiece : InteractivePiece
{
    private SelectablePiece selectField;

    private GameField fieldAtk;
    private Piece target;

    IEnumerator posToAtk;

    protected override void Awake()
    {
        base.Awake();
        selectField = GetComponent<SelectablePiece>();
    }

    private void NewTarget()
    {
        if (finished) return;

        GameField fieldPiece = piece.targetField;
        if (fieldPiece == null || !fieldPiece.hasPiece) return;

        target = fieldPiece.piece;
        fieldAtk = selectField.GetEmptyFieldFromActive(fieldPiece);

        if (fieldAtk == null) fieldAtk = piece.field;

        if (posToAtk != null) StopCoroutine(posToAtk);
        posToAtk = PositionToAttack();
        StartCoroutine(posToAtk);
    }

    private void Sucess()
    {
        StartCoroutine(WaitToAttack());
    }

    private void Failed()
    {
        CancelAttack();
    }

    private IEnumerator WaitToAttack()
    {
        yield return new WaitForSeconds(3.5f);
        EndAttack();
        SendMessage("NewTarget");
    }

    private IEnumerator PositionToAttack()
    {
        while (transform.position != fieldAtk.transform.position)
        {
            yield return new WaitForEndOfFrame();
        }

        transform.LookAt(target.transform);
        ReadyToAttack();
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

    protected virtual void ReadyToAttack()
    {
        InteractivePiece combatTarget = GetCombatPiece();
        Attack(combatTarget);
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