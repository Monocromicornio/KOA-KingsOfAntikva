using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace KOA.Placement.Tests
{
    /// <summary>
    /// Guards against the offline freeze: the NPC being left with only movable pieces that are sealed in
    /// by bombs and the chest, so it never plays and the match never ends.
    /// </summary>
    public class EnemyPlacementTests
    {
        // Board of the Game scene: 8x8. The enemy formation is the last two rows (cells 48..63);
        // row 6 (cells 48..55) is the front row, facing the free cells of row 5.
        private const int Width = 8;
        private const int BoardCells = 64;
        private const int FirstFormationCell = 48;
        private const int PinnedPiece = 0;
        private const int PinnedSlot = 6;

        // Composition of EnemySquad.defaultPieces in Game.unity:
        // 2 Soldier, 2 Antibomb, Sergeant, Lieutenant, Captain, Major, Colonel, General, Minister,
        // 3 Bomb, 1 Chest, Spy  ->  12 movable pieces and 4 immobile ones (indices 11..14).
        private static bool[] RealComposition()
        {
            bool[] immobile = new bool[16];
            for (int i = 11; i <= 14; i++) immobile[i] = true;
            return immobile;
        }

        private static int[] RealFormation()
        {
            int[] cells = new int[16];
            for (int i = 0; i < cells.Length; i++) cells[i] = FirstFormationCell + i;
            return cells;
        }

        private static CellState[] Board(params (int cell, CellState state)[] pieces)
        {
            CellState[] cells = new CellState[BoardCells];
            foreach ((int cell, CellState state) in pieces) cells[cell] = state;
            return cells;
        }

        // ------------------------------------------------------------------ validator

        [Test]
        public void Validator_FlagsMobilePieceSealedInCorner()
        {
            // Back-row corner (63) with an immobile piece beside it (62) and in front of it (55)
            CellState[] cells = Board(
                (63, CellState.Mobile), (62, CellState.Immobile), (55, CellState.Immobile));

            List<int> trapped = PlacementValidator.FindTrappedCells(cells, Width);

            Assert.AreEqual(new List<int> { 63 }, trapped);
            Assert.IsFalse(PlacementValidator.IsPlayable(cells, Width));
        }

        [Test]
        public void Validator_FlagsGroupOfMobilePiecesSealedTogether()
        {
            // Two movable pieces block each other in the corner, everything around them is immobile
            CellState[] cells = Board(
                (63, CellState.Mobile), (62, CellState.Mobile),
                (61, CellState.Immobile), (54, CellState.Immobile), (55, CellState.Immobile));

            List<int> trapped = PlacementValidator.FindTrappedCells(cells, Width);
            trapped.Sort();

            Assert.AreEqual(new List<int> { 62, 63 }, trapped);
        }

        [Test]
        public void Validator_AcceptsCornerWhenAnotherMobilePieceCanLeave()
        {
            // 63 is boxed in by 62 (immobile) but 55 is movable and touches the free row 5
            CellState[] cells = Board(
                (63, CellState.Mobile), (62, CellState.Immobile), (55, CellState.Mobile));

            Assert.IsTrue(PlacementValidator.IsPlayable(cells, Width));
        }

        [Test]
        public void Validator_IgnoresImmobilePiecesOnTheirOwn()
        {
            // Walls with nothing movable behind them are not a problem
            CellState[] cells = Board(
                (63, CellState.Immobile), (62, CellState.Immobile), (55, CellState.Immobile));

            Assert.IsTrue(PlacementValidator.IsPlayable(cells, Width));
        }

        [Test]
        public void Validator_BoardEdgeCountsAsAWall()
        {
            // Top-left corner of the board (0): only 1 and 8 lead out, both immobile
            CellState[] cells = Board(
                (0, CellState.Mobile), (1, CellState.Immobile), (8, CellState.Immobile));

            Assert.IsFalse(PlacementValidator.IsPlayable(cells, Width));
        }

        [Test]
        public void Validator_AgreesWithBruteForceOracle()
        {
            bool[] immobile = RealComposition();
            int[] formation = RealFormation();
            Random random = new Random(12345);
            int playable = 0;
            int trapped = 0;

            for (int i = 0; i < 3000; i++)
            {
                int[] layout = EnemyPlacementGenerator.GenerateRandom(immobile, formation, random);
                CellState[] cells = EnemyPlacementGenerator.BuildCells(immobile, layout, BoardCells);

                bool expected = !CanBeLeftWithoutMoves(cells);
                bool actual = PlacementValidator.IsPlayable(cells, Width);

                Assert.AreEqual(expected, actual, $"Validator and oracle disagree on layout {string.Join(",", layout)}");
                if (actual) playable++; else trapped++;
            }

            // The comparison only means something if both outcomes were exercised
            Assert.Greater(playable, 0);
            Assert.Greater(trapped, 0);
        }

        // ------------------------------------------------------------------ generator

        [Test]
        public void Generator_NeverProducesASealedPiece()
        {
            bool[] immobile = RealComposition();
            int[] formation = RealFormation();
            const int runs = 20000;
            int maxAttempts = 0;

            for (int seed = 0; seed < runs; seed++)
            {
                bool found = EnemyPlacementGenerator.TryGenerate(
                    immobile, formation, BoardCells, Width, new Random(seed),
                    out int[] layout, out int attempts, PinnedPiece, PinnedSlot);

                Assert.IsTrue(found, $"No playable layout for seed {seed}");
                maxAttempts = Math.Max(maxAttempts, attempts);

                AssertIsValidPermutation(layout, formation, seed);
                Assert.AreEqual(formation[PinnedSlot], layout[PinnedPiece], $"Pinned piece moved (seed {seed})");

                CellState[] cells = EnemyPlacementGenerator.BuildCells(immobile, layout, BoardCells);
                Assert.IsTrue(PlacementValidator.IsPlayable(cells, Width), $"Sealed piece (seed {seed})");
                Assert.IsTrue(PlacementValidator.HasLegalMove(cells, Width), $"NPC has no first move (seed {seed})");

                // Cross-check with the independent oracle on a sample (it is much slower)
                if (seed % 40 == 0)
                {
                    Assert.IsFalse(CanBeLeftWithoutMoves(cells), $"Oracle found a stuck set (seed {seed})");
                }
            }

            // Rejection sampling should converge quickly, far from the attempt limit
            Assert.Less(maxAttempts, EnemyPlacementGenerator.DefaultMaxAttempts / 4,
                $"Generator needed {maxAttempts} attempts in the worst case");
        }

        [Test]
        public void Generator_IsDeterministicForTheSameSeed()
        {
            bool[] immobile = RealComposition();
            int[] formation = RealFormation();

            EnemyPlacementGenerator.TryGenerate(immobile, formation, BoardCells, Width, new Random(7),
                out int[] first, out _, PinnedPiece, PinnedSlot);
            EnemyPlacementGenerator.TryGenerate(immobile, formation, BoardCells, Width, new Random(7),
                out int[] second, out _, PinnedPiece, PinnedSlot);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void Generator_FailsCleanlyWhenNoPlayableLayoutExists()
        {
            // 3x1 board completely filled: the movable piece can never have a free neighbour
            bool[] immobile = { false, true, true };
            int[] formation = { 0, 1, 2 };

            bool found = EnemyPlacementGenerator.TryGenerate(
                immobile, formation, 3, 3, new Random(1),
                out int[] layout, out int attempts, maxAttempts: 50);

            Assert.IsFalse(found);
            Assert.IsNull(layout);
            Assert.AreEqual(50, attempts);
        }

        [Test]
        public void PreviousRandomPlacement_DidProduceSealedPieces()
        {
            // Proves the checks above can actually catch the bug: plain shuffling (the old behaviour) seals pieces.
            bool[] immobile = RealComposition();
            int[] formation = RealFormation();
            Random random = new Random(2024);
            const int runs = 50000;
            int sealedLayouts = 0;

            for (int i = 0; i < runs; i++)
            {
                int[] layout = EnemyPlacementGenerator.GenerateRandom(immobile, formation, random, PinnedPiece, PinnedSlot);
                CellState[] cells = EnemyPlacementGenerator.BuildCells(immobile, layout, BoardCells);
                if (!PlacementValidator.IsPlayable(cells, Width)) sealedLayouts++;
            }

            Assert.Greater(sealedLayouts, 0, "Plain shuffling never sealed a piece, so these tests would not catch the bug.");
            TestContext.WriteLine($"Plain shuffle sealed a movable piece in {sealedLayouts} of {runs} layouts ({100.0 * sealedLayouts / runs:F2}%).");
        }

        // ------------------------------------------------------------------ helpers

        private static void AssertIsValidPermutation(int[] layout, int[] formation, int seed)
        {
            HashSet<int> allowed = new HashSet<int>(formation);
            HashSet<int> used = new HashSet<int>();
            foreach (int cell in layout)
            {
                Assert.IsTrue(allowed.Contains(cell), $"Cell {cell} is outside the formation (seed {seed})");
                Assert.IsTrue(used.Add(cell), $"Cell {cell} used twice (seed {seed})");
            }
        }

        /// <summary>
        /// Brute force, written independently of the validator: is there a non-empty set of movable pieces
        /// whose every neighbour is the board edge, an immobile piece or another piece of the set?
        /// If the rest of the army dies, those pieces could never move again.
        /// </summary>
        private static bool CanBeLeftWithoutMoves(CellState[] cells)
        {
            List<int> mobile = new List<int>();
            for (int i = 0; i < cells.Length; i++)
                if (cells[i] == CellState.Mobile) mobile.Add(i);

            int subsets = 1 << mobile.Count;
            for (int mask = 1; mask < subsets; mask++)
            {
                HashSet<int> alive = new HashSet<int>();
                for (int b = 0; b < mobile.Count; b++)
                    if ((mask & (1 << b)) != 0) alive.Add(mobile[b]);

                bool allStuck = true;
                foreach (int cell in alive)
                {
                    foreach (int neighbour in NeighboursOf(cell, cells.Length))
                    {
                        bool blocked = cells[neighbour] == CellState.Immobile || alive.Contains(neighbour);
                        if (!blocked) { allStuck = false; break; }
                    }

                    if (!allStuck) break;
                }

                if (allStuck) return true;
            }

            return false;
        }

        private static IEnumerable<int> NeighboursOf(int cell, int length)
        {
            int row = cell / Width;
            int column = cell % Width;

            if (column > 0) yield return cell - 1;
            if (column < Width - 1) yield return cell + 1;
            if (row > 0) yield return cell - Width;
            if (cell + Width < length) yield return cell + Width;
        }
    }
}
