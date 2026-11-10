using System;
using System.Collections.Generic;

namespace KOA.Placement
{
    /// <summary>
    /// State of a board cell from the point of view of the side being validated.
    /// </summary>
    public enum CellState : byte
    {
        /// <summary>Empty cell, or a cell held by the opponent (a piece can move or attack there).</summary>
        Free = 0,

        /// <summary>A piece of this side that can move.</summary>
        Mobile = 1,

        /// <summary>A piece of this side that never moves (bomb, chest).</summary>
        Immobile = 2,
    }

    /// <summary>
    /// Pure-C# rules that decide whether an arrangement of pieces can leave a side without any legal move.
    /// Movement follows SelectablePiece: orthogonal steps, blocked by the side's own pieces and by the board edge.
    /// </summary>
    public static class PlacementValidator
    {
        /// <summary>
        /// A mobile piece is permanently stuck when every cell it could ever reach is held by its own side.
        /// Walls are immobile pieces (they never leave their cell) and the board edge. So take each connected
        /// region of non-immobile cells: if it has no Free cell, its mobile pieces can never move,
        /// no matter how the other pieces move or die.
        /// </summary>
        /// <param name="cells">Board cells, row after row, <paramref name="width"/> cells per row.</param>
        /// <param name="width">Number of cells in one row of the board.</param>
        /// <returns>The cells that hold permanently stuck mobile pieces (empty when the arrangement is playable).</returns>
        public static List<int> FindTrappedCells(CellState[] cells, int width)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            if (width <= 0 || cells.Length % width != 0)
                throw new ArgumentException("cells.Length must be a multiple of width.", nameof(width));

            List<int> trapped = new List<int>();
            bool[] visited = new bool[cells.Length];
            Stack<int> pending = new Stack<int>();
            List<int> region = new List<int>();
            int[] neighbours = new int[4];

            for (int start = 0; start < cells.Length; start++)
            {
                if (visited[start] || cells[start] == CellState.Immobile) continue;

                bool hasFreeCell = false;
                region.Clear();
                visited[start] = true;
                pending.Push(start);

                while (pending.Count > 0)
                {
                    int cell = pending.Pop();
                    region.Add(cell);
                    if (cells[cell] == CellState.Free) hasFreeCell = true;

                    int count = GetNeighbours(cell, width, cells.Length, neighbours);
                    for (int i = 0; i < count; i++)
                    {
                        int next = neighbours[i];
                        if (visited[next] || cells[next] == CellState.Immobile) continue;
                        visited[next] = true;
                        pending.Push(next);
                    }
                }

                // A region without a Free cell only holds mobile pieces, all of them sealed in.
                if (!hasFreeCell) trapped.AddRange(region);
            }

            return trapped;
        }

        /// <summary>
        /// True when no mobile piece can end up permanently stuck.
        /// </summary>
        public static bool IsPlayable(CellState[] cells, int width)
        {
            return FindTrappedCells(cells, width).Count == 0;
        }

        /// <summary>
        /// True when at least one mobile piece has a Free neighbour right now (the side can make a move this turn).
        /// </summary>
        public static bool HasLegalMove(CellState[] cells, int width)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            if (width <= 0 || cells.Length % width != 0)
                throw new ArgumentException("cells.Length must be a multiple of width.", nameof(width));

            int[] neighbours = new int[4];
            for (int cell = 0; cell < cells.Length; cell++)
            {
                if (cells[cell] != CellState.Mobile) continue;

                int count = GetNeighbours(cell, width, cells.Length, neighbours);
                for (int i = 0; i < count; i++)
                {
                    if (cells[neighbours[i]] == CellState.Free) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Writes the orthogonal neighbours of a cell (same row for left/right, same column for up/down)
        /// into <paramref name="buffer"/> and returns how many there are.
        /// </summary>
        internal static int GetNeighbours(int cell, int width, int length, int[] buffer)
        {
            int count = 0;
            int column = cell % width;

            if (column > 0) buffer[count++] = cell - 1;
            if (column < width - 1) buffer[count++] = cell + 1;
            if (cell - width >= 0) buffer[count++] = cell - width;
            if (cell + width < length) buffer[count++] = cell + width;

            return count;
        }
    }
}
