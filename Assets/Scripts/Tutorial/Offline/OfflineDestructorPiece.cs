using System.Collections.Generic;
using UnityEngine;

public class OfflineDestructorPiece : OfflineAttackPiece
{
    [SerializeField]
    private List<PieceType> toDestroy;

    protected override void ReadyToAttack(OfflineInteractivePiece combatTarget)
    {
        Debug.Log($"[OfflineDestructorPiece] ReadyToAttack called. Target: {combatTarget?.piece.name}, Type: {combatTarget?.piece.type}");
        
        if (toDestroy.Contains(combatTarget.piece.type))
        {
            Debug.Log($"[OfflineDestructorPiece] Target type {combatTarget.piece.type} is in destroy list. Calling InstaKillAttack");
            InstaKillAttack(combatTarget);
            return;
        }

        Debug.Log($"[OfflineDestructorPiece] Calling normal Attack");
        Attack(combatTarget);
    }
}
