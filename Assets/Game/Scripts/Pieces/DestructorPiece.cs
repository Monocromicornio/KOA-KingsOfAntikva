using System.Collections.Generic;
using UnityEngine;

public class DestructorPiece : AttackPiece
{
    [SerializeField]
    List<PieceType> toDestroy;

    protected override void ReadyToAttack()
    {
        InteractivePiece combatTarget = GetCombatPiece();
        Piece targetPiece = combatTarget.piece;
        if (toDestroy.Contains(targetPiece.type))
        {
            CounterAttack(combatTarget);
        }

        Attack(combatTarget);
    }
}