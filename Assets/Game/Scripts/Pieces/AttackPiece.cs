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
        if (!fieldPiece.hasPiece) return;

        fieldAtk = selectField.GetEmptyFieldFromActive(fieldPiece);
        target = fieldPiece.piece;

        if (posToAtk != null) StopCoroutine(posToAtk);
        posToAtk = PositionToAttack();
        StartCoroutine(posToAtk);
    }

    private IEnumerator PositionToAttack()
    {
        while (transform.position != fieldAtk.transform.position)
        {
            yield return new WaitForEndOfFrame();
        }

        ReadyToAttack();
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
}