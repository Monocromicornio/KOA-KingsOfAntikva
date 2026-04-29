using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Debug utility that logs adjacency and directional info for selected pieces.
/// Attach to any GameObject in the scene. Does NOT alter existing game flow.
/// </summary>
public class PieceDebugInfo : MonoBehaviour
{
    private const string LOG_PREFIX = "[PieceDebug]";

    private BoardController board;
    private Piece lastActivePiece;
    private GameField lastHoveredField;

    private void Update()
    {
        if (board == null)
        {
            board = BoardController.instance;
            if (board == null) return;
        }

        Piece currentActive = Piece.activePiece;

        if (currentActive != null && currentActive != lastActivePiece)
        {
            lastActivePiece = currentActive;
            LogAdjacentEnemies(currentActive);
        }

        if (currentActive == null && lastActivePiece != null)
        {
            lastActivePiece = null;
        }

        if (currentActive != null)
        {
            DetectHoveredFieldDirection(currentActive);
        }
    }

    /// <summary>
    /// Checks all 4 adjacent fields (up, down, left, right) of the given piece for enemies.
    /// </summary>
    private void LogAdjacentEnemies(Piece piece)
    {
        int pieceIndex = piece.indexCurrentField;
        if (pieceIndex < 0) return;

        GameField[] gameFields = board.gameFields;
        if (gameFields == null || gameFields.Length == 0) return;

        int columnLength = board.ColumnLength();

        Dictionary<string, int> adjacentOffsets = new Dictionary<string, int>
        {
            { "Frente (column+)", columnLength },
            { "Tras (column-)",  -columnLength },
            { "Direita (row+)",   1 },
            { "Esquerda (row-)", -1 }
        };

        bool foundEnemy = false;

        foreach (KeyValuePair<string, int> entry in adjacentOffsets)
        {
            int targetIndex = pieceIndex + entry.Value;

            if (!IsValidAdjacentField(pieceIndex, targetIndex, entry.Value, columnLength, gameFields.Length))
                continue;

            GameField adjacentField = board.GetGameField(targetIndex);
            if (adjacentField == null) continue;

            if (adjacentField.hasPiece && adjacentField.piece != null)
            {
                bool isEnemy = adjacentField.piece.pieceColor != piece.pieceColor;
                if (isEnemy)
                {
                    foundEnemy = true;
                    Debug.Log($"{LOG_PREFIX} INIMIGO detectado ao lado [{entry.Key}] da peca '{piece.name}' " +
                              $"| Inimigo: '{adjacentField.piece.name}' no field index {targetIndex}");
                }
            }
        }

        if (!foundEnemy)
        {
            Debug.Log($"{LOG_PREFIX} Nenhum inimigo adjacente a peca '{piece.name}' (field index {pieceIndex})");
        }
    }

    /// <summary>
    /// Detects which GameField the mouse is hovering over and logs its direction relative to the active piece.
    /// </summary>
    private void DetectHoveredFieldDirection(Piece piece)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        GameField hoveredField = hit.collider.GetComponent<GameField>();
        if (hoveredField == null)
        {
            hoveredField = hit.collider.GetComponentInParent<GameField>();
        }

        if (hoveredField == null || hoveredField == lastHoveredField) return;

        lastHoveredField = hoveredField;

        string direction = GetDirectionLabel(piece, hoveredField);
        Debug.Log($"{LOG_PREFIX} Mouse sobre field index {hoveredField.index} (row {hoveredField.row}, col {hoveredField.column}) " +
                  $"| Direcao relativa a '{piece.name}': {direction}");
    }

    /// <summary>
    /// Returns a human-readable direction label for the hovered field relative to the piece.
    /// </summary>
    private string GetDirectionLabel(Piece piece, GameField hoveredField)
    {
        GameField pieceField = board.GetGameField(piece.indexCurrentField);
        if (pieceField == null) return "DESCONHECIDA";

        int pieceRow = pieceField.row;
        int pieceCol = pieceField.column;
        int hoveredRow = hoveredField.row;
        int hoveredCol = hoveredField.column;

        int deltaRow = hoveredRow - pieceRow;
        int deltaCol = hoveredCol - pieceCol;

        // Determine primary direction based on grid position
        if (deltaRow == 0 && deltaCol == 0) return "MESMA CASA";

        List<string> directions = new List<string>();

        if (deltaRow > 0) directions.Add("FRENTE");
        else if (deltaRow < 0) directions.Add("TRAS");

        if (deltaCol > 0) directions.Add("DIREITA");
        else if (deltaCol < 0) directions.Add("ESQUERDA");

        return string.Join(" + ", directions);
    }

    /// <summary>
    /// Validates that the target index is a valid adjacent field (same row or same column, within bounds).
    /// </summary>
    private bool IsValidAdjacentField(int currentIndex, int targetIndex, int offset, int columnLength, int totalFields)
    {
        if (targetIndex < 0 || targetIndex >= totalFields) return false;

        // Row movement (offset = +1 or -1): must stay on the same row
        if (offset == 1 || offset == -1)
        {
            int currentRow = currentIndex / columnLength;
            int targetRow = targetIndex / columnLength;
            return currentRow == targetRow;
        }

        // Column movement (offset = +/- columnLength): always valid if within bounds
        return true;
    }
}
