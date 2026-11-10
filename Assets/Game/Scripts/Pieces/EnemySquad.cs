using System.Collections.Generic;
using System.Threading.Tasks;
using KOA.Placement;
using UnityEngine;

public class EnemySquad : Squad
{
    private List<FakePiece> _fakePieces;
    public List<FakePiece> fakePieces
    {
        get
        {
            _fakePieces ??= new List<FakePiece>();
            return _fakePieces;
        }
    }

    [SerializeField]
    Piece[] defaultPieces;

    // The first default piece always starts on this slot of the formation (a front-row cell).
    private const int PinnedPieceIndex = 0;
    private const int PinnedSlot = 6;

    public override Task LoadPieces()
    {
        int pieceCount = Mathf.Min(defaultPieces.Length, gameFields.Length);
        int lastIndex = gameFields.Length - 1;
        int firstIndex = lastIndex - pieceCount + 1;

        // Board cells that make up the enemy formation (the last rows of the board)
        int[] formationCells = new int[pieceCount];
        for (int i = 0; i < pieceCount; i++)
            formationCells[i] = firstIndex + i;

        // Pieces without a SelectablePiece (bombs, chest) never move
        bool[] pieceIsImmobile = new bool[pieceCount];
        for (int i = 0; i < pieceCount; i++)
            pieceIsImmobile[i] = defaultPieces[i].GetComponent<SelectablePiece>() == null;

        // Arrange the pieces so that no movable piece is sealed in by immobile ones,
        // otherwise the NPC could end up with no legal move and the match would freeze.
        System.Random random = new System.Random(Random.Range(int.MinValue, int.MaxValue));
        bool found = EnemyPlacementGenerator.TryGenerate(
            pieceIsImmobile, formationCells, gameFields.Length, board.ColumnLength(), random,
            out int[] cellOfPiece, out int attempts, PinnedPieceIndex, PinnedSlot);

        if (!found)
        {
            Debug.LogError($"[EnemySquad] No playable arrangement found after {attempts} attempts. Using an unchecked random one.");
            cellOfPiece = EnemyPlacementGenerator.GenerateRandom(
                pieceIsImmobile, formationCells, random, PinnedPieceIndex, PinnedSlot);
        }

        for (int i = 0; i < pieceCount; i++)
        {
            Vector3 pos = gameFields[cellOfPiece[i]].transform.position;
            Quaternion rot = Quaternion.Euler(0, 180, 0);
            Piece piece = Instantiate(defaultPieces[i], pos, rot);

            //Set as fake
            piece.TurnRedPiece();
        }

        return Task.CompletedTask;
    }
}