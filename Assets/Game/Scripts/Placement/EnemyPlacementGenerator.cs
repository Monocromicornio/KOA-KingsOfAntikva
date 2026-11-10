using System;
using System.Collections.Generic;

namespace KOA.Placement
{
    /// <summary>
    /// Builds the NPC's starting arrangement. Draws random layouts and keeps the first one in which no mobile
    /// piece is sealed in by immobile pieces (see <see cref="PlacementValidator"/>).
    /// </summary>
    public static class EnemyPlacementGenerator
    {
        public const int DefaultMaxAttempts = 1000;

        /// <summary>
        /// Draws random layouts until one is playable.
        /// </summary>
        /// <param name="pieceIsImmobile">One entry per piece: true for pieces that never move (bombs, chest).</param>
        /// <param name="formationCells">Board indices where the pieces may be placed.</param>
        /// <param name="boardCellCount">Total number of cells on the board.</param>
        /// <param name="boardWidth">Number of cells in one row of the board.</param>
        /// <param name="random">Source of randomness (pass a seeded instance to reproduce a layout).</param>
        /// <param name="cellOfPiece">For each piece, the board index it was placed on. Null when no playable layout was found.</param>
        /// <param name="attempts">How many layouts were drawn.</param>
        /// <param name="pinnedPiece">Piece that must go to <paramref name="pinnedSlot"/>, or -1 for none.</param>
        /// <param name="pinnedSlot">Position inside <paramref name="formationCells"/> reserved for the pinned piece.</param>
        /// <param name="maxAttempts">Give up after this many draws.</param>
        /// <returns>True when a playable layout was found.</returns>
        public static bool TryGenerate(
            bool[] pieceIsImmobile,
            int[] formationCells,
            int boardCellCount,
            int boardWidth,
            Random random,
            out int[] cellOfPiece,
            out int attempts,
            int pinnedPiece = -1,
            int pinnedSlot = -1,
            int maxAttempts = DefaultMaxAttempts)
        {
            for (attempts = 1; attempts <= maxAttempts; attempts++)
            {
                int[] candidate = GenerateRandom(pieceIsImmobile, formationCells, random, pinnedPiece, pinnedSlot);
                CellState[] cells = BuildCells(pieceIsImmobile, candidate, boardCellCount);

                if (PlacementValidator.IsPlayable(cells, boardWidth))
                {
                    cellOfPiece = candidate;
                    return true;
                }
            }

            attempts = maxAttempts;
            cellOfPiece = null;
            return false;
        }

        /// <summary>
        /// One uniformly random layout, with no validation. This is how the NPC was arranged before the fix.
        /// </summary>
        public static int[] GenerateRandom(
            bool[] pieceIsImmobile,
            int[] formationCells,
            Random random,
            int pinnedPiece = -1,
            int pinnedSlot = -1)
        {
            if (pieceIsImmobile == null) throw new ArgumentNullException(nameof(pieceIsImmobile));
            if (formationCells == null) throw new ArgumentNullException(nameof(formationCells));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (formationCells.Length < pieceIsImmobile.Length)
                throw new ArgumentException("The formation has fewer cells than there are pieces.", nameof(formationCells));

            bool hasPin = pinnedPiece >= 0 && pinnedPiece < pieceIsImmobile.Length
                          && pinnedSlot >= 0 && pinnedSlot < formationCells.Length;

            List<int> freeSlots = new List<int>(formationCells.Length);
            for (int slot = 0; slot < formationCells.Length; slot++)
            {
                if (hasPin && slot == pinnedSlot) continue;
                freeSlots.Add(slot);
            }

            // Fisher-Yates shuffle
            for (int i = freeSlots.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = freeSlots[i];
                freeSlots[i] = freeSlots[j];
                freeSlots[j] = temp;
            }

            int[] cellOfPiece = new int[pieceIsImmobile.Length];
            int next = 0;
            for (int piece = 0; piece < pieceIsImmobile.Length; piece++)
            {
                if (hasPin && piece == pinnedPiece)
                {
                    cellOfPiece[piece] = formationCells[pinnedSlot];
                    continue;
                }

                cellOfPiece[piece] = formationCells[freeSlots[next++]];
            }

            return cellOfPiece;
        }

        /// <summary>
        /// Converts a layout into the cell grid the validator works on.
        /// </summary>
        public static CellState[] BuildCells(bool[] pieceIsImmobile, int[] cellOfPiece, int boardCellCount)
        {
            CellState[] cells = new CellState[boardCellCount];
            for (int piece = 0; piece < cellOfPiece.Length; piece++)
            {
                cells[cellOfPiece[piece]] = pieceIsImmobile[piece] ? CellState.Immobile : CellState.Mobile;
            }

            return cells;
        }
    }
}
