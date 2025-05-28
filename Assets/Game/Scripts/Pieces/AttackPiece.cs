using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SelectField))]
public class AttackPiece : InteractivePiece
{
    private SelectField selectField;

    private GameField fieldAtk;
    private Piece target;

    IEnumerator posToAtk;

    protected override void Awake()
    {
        base.Awake();
        selectField = GetComponent<SelectField>();
    }

    private void NewTarget()
    {
        if (finished) return;

        GameField fieldPiece = piece.targetField;
        if (fieldPiece == null || !fieldPiece.hasPiece) return;

        target = fieldPiece.piece;
        fieldAtk = selectField.GetEmptyFieldFromActive(fieldPiece);

        if (posToAtk != null) StopCoroutine(posToAtk);
        posToAtk = PositionToAttack();
        StartCoroutine(posToAtk);
    }

    private void Sucess()
    {
        StartCoroutine(WaitToAttack());
    }

    private IEnumerator WaitToAttack()
    {
        yield return new WaitForSeconds(3.5f);
        SendMessage("NewTarget");
    }

    private IEnumerator PositionToAttack()
    {
        while (transform.position != fieldAtk.transform.position)
        {
            yield return new WaitForEndOfFrame();
        }

        ReadyToAttack();
        EndAttack();
    }

    protected InteractivePiece GetCombatPiece()
    {
        InteractivePiece combatTarget = target.GetComponent<InteractivePiece>();
        if (combatTarget == null)
        {
            piece.SelectedAField(fieldAtk);
            return null;
        }
        return combatTarget;
    }

    protected virtual void ReadyToAttack()
    {
        InteractivePiece combatTarget = GetCombatPiece();
        Attack(combatTarget);
    }

    private void EndAttack()
    {
        target = null;
        fieldAtk = null;
    }
}