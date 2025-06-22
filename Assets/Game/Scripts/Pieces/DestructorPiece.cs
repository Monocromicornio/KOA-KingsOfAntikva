using System.Collections.Generic;
using UnityEngine;

public class DestructorPiece : AttackPiece
{
    [SerializeField]
    List<PieceType> toDestroy;

    protected override void ReadyToAttack(InteractivePiece combatTarget)
    {
        if (toDestroy.Contains(combatTarget.piece.type))
        {
            InstaKillAttack(combatTarget);
            return;
        }

        Attack(combatTarget);
    }
}