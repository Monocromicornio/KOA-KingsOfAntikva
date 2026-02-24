using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public static MinimapController instance;

    [Header("References")]
    [SerializeField] private GameObject gridContainer;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private BoardController boardController;

    [Header("Settings")]
    [SerializeField] private int gridSize = 8;

    private MinimapCell[] cells;
    private Dictionary<int, Piece> piecePositions = new Dictionary<int, Piece>();
    private string activeMarking;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (boardController == null)
            boardController = BoardController.instance;

        InitializeGrid();
        StartCoroutine(WaitAndSyncWithBoard());
    }

    private System.Collections.IEnumerator WaitAndSyncWithBoard()
    {
        while (boardController == null || !boardController.isFinished())
        {
            yield return new WaitForSeconds(0.1f);
            if (boardController == null)
                boardController = BoardController.instance;
        }

        SyncWithBoard();
    }

    private void InitializeGrid()
    {
        if (gridContainer == null)
        {
            Debug.LogError("GridContainer não está atribuído no MinimapController!");
            return;
        }

        if (cellPrefab == null)
        {
            Debug.LogError("CellPrefab não está atribuído no MinimapController!");
            return;
        }

        int totalCells = gridSize * gridSize;
        cells = new MinimapCell[totalCells];

        for (int i = 0; i < totalCells; i++)
        {
            GameObject cellObj = Instantiate(cellPrefab, gridContainer.transform);
            cellObj.name = $"Cell_{i}";
            cellObj.SetActive(true);
            
            MinimapCell cell = cellObj.GetComponent<MinimapCell>();
            if (cell == null)
                cell = cellObj.AddComponent<MinimapCell>();

            cell.Initialize(i);
            cells[i] = cell;

            Button cellButton = cellObj.GetComponent<Button>();
            if (cellButton == null)
                cellButton = cellObj.AddComponent<Button>();

            int cellIndex = i;
            cellButton.onClick.AddListener(() => OnCellClicked(cellIndex));
        }

        if (cellPrefab.activeSelf)
            cellPrefab.SetActive(false);
    }

    private void SyncWithBoard()
    {
        if (boardController == null || boardController.gameFields == null)
            return;

        for (int i = 0; i < boardController.gameFields.Length && i < cells.Length; i++)
        {
            GameField field = boardController.gameFields[i];
            if (field != null && field.piece != null)
            {
                UpdateCellAtIndex(i, field.piece);
            }
        }
    }

    public void RegisterPiece(Piece piece)
    {
        if (piece == null)
            return;

        int index = piece.indexCurrentField;
        if (index >= 0 && index < cells.Length)
        {
            piecePositions[index] = piece;
            UpdateCellAtIndex(index, piece);
        }
    }

    public void UnregisterPiece(Piece piece)
    {
        if (piece == null)
            return;

        int index = piece.indexCurrentField;
        if (piecePositions.ContainsKey(index) && piecePositions[index] == piece)
        {
            piecePositions.Remove(index);
            if (index >= 0 && index < cells.Length)
                cells[index].Clear();
        }
    }

    public void UpdatePiecePosition(Piece piece, int oldIndex, int newIndex)
    {
        if (piece == null)
            return;

        if (oldIndex >= 0 && oldIndex < cells.Length)
        {
            if (piecePositions.ContainsKey(oldIndex))
                piecePositions.Remove(oldIndex);
            cells[oldIndex].Clear();
        }

        if (newIndex >= 0 && newIndex < cells.Length)
        {
            piecePositions[newIndex] = piece;
            UpdateCellAtIndex(newIndex, piece);
        }
    }

    private void UpdateCellAtIndex(int index, Piece piece)
    {
        if (index >= 0 && index < cells.Length)
        {
            cells[index].UpdateCell(piece);
        }
    }

    private void OnCellClicked(int cellIndex)
    {
        if (string.IsNullOrEmpty(activeMarking))
            return;

        if (cellIndex >= 0 && cellIndex < cells.Length)
        {
            cells[cellIndex].SetMarking(activeMarking);
        }
    }

    public void SetActiveMarking(string marking)
    {
        activeMarking = marking;
    }

    public void ClearAllMarkings()
    {
        activeMarking = null;
        foreach (MinimapCell cell in cells)
        {
            cell.ClearMarking();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
