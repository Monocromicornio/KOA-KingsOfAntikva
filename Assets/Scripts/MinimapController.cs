using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.onlineobject.objectnet;

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
    private Dictionary<Piece, string> pieceMarkings = new Dictionary<Piece, string>();
    private string activeMarking;
    private bool isClientPerspective = false;

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

        if (MatchController.instance != null && MatchController.instance.hasConnection)
            StartCoroutine(WaitForConnectionAndInitialize());
        else
            InitializeMinimapImmediate();
    }

    /// <summary>
    /// Waits until both host and client are fully connected and the first turn has been
    /// assigned before initializing the minimap. This guarantees the network perspective
    /// (host vs client) is stable when InitializePerspective runs.
    /// </summary>
    private System.Collections.IEnumerator WaitForConnectionAndInitialize()
    {
        yield return new WaitUntil(() =>
            MatchController.instance != null &&
            MatchController.instance.hasConnection &&
            MatchController.instance.turn != TurnState.undefined);

        Debug.Log("Initialized game for minimap");

        yield return new WaitForSeconds(3f);
        InitializeMinimapImmediate();
    }

    private void InitializeMinimapImmediate()
    {
        Debug.Log("Initialize game for minimap");

        InitializePerspective();
        InitializeGrid();
        StartCoroutine(WaitAndSyncWithBoard());
    }

    private void InitializePerspective()
    {
        if (MatchController.instance != null)
        {
            NetworkManager networkManager = MatchController.instance.networkManager;
            if (networkManager != null && networkManager.IsClientConnection())
            {
                isClientPerspective = true;
            }
        }
    }

    private System.Collections.IEnumerator WaitAndSyncWithBoard()
    {
        while (boardController == null || !boardController.isFinished() || MatchController.instance.hasStarted == false)
        {
            yield return new WaitForSeconds(0.1f);
            if (boardController == null)
                boardController = BoardController.instance;
        }

        Debug.Log("Initialize board for minimap");
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

        for (int boardIndex = 0; boardIndex < boardController.gameFields.Length && boardIndex < cells.Length; boardIndex++)
        {
            GameField field = boardController.gameFields[boardIndex];
            if (field != null && field.piece != null)
            {
                int minimapIndex = ConvertBoardIndexToMinimapIndex(boardIndex);
                UpdateCellAtIndex(minimapIndex, minimapIndex, field.piece);
            }
        }
    }

    private int ConvertBoardIndexToMinimapIndex(int boardIndex)
    {
        if (!isClientPerspective)
            return boardIndex;

        int row = boardIndex / gridSize;
        int col = boardIndex % gridSize;
        
        int flippedRow = (gridSize - 1) - row;
        int flippedCol = (gridSize - 1) - col;
        
        return flippedRow * gridSize + flippedCol;
    }

    public void RegisterPiece(Piece piece)
    {
        if (piece == null || cells == null)
            return;

        int boardIndex = piece.indexCurrentField;
        int minimapIndex = ConvertBoardIndexToMinimapIndex(boardIndex);
        
        if (minimapIndex >= 0 && minimapIndex < cells.Length)
        {
            piecePositions[boardIndex] = piece;
            UpdateCellAtIndex(minimapIndex, minimapIndex, piece);
        }
    }

    public void UnregisterPiece(Piece piece)
    {
        if (piece == null || cells == null)
            return;

        int boardIndex = piece.indexCurrentField;
        if (piecePositions.ContainsKey(boardIndex) && piecePositions[boardIndex] == piece)
        {
            piecePositions.Remove(boardIndex);
            pieceMarkings.Remove(piece);
        }

        Debug.Log("Unregistering piece from minimap at board index " + boardIndex);

        int minimapIndex = ConvertBoardIndexToMinimapIndex(boardIndex);
        if (minimapIndex >= 0 && minimapIndex < cells.Length)
            cells[minimapIndex].Clear();
    }

    public void UpdatePiecePosition(Piece piece, int oldBoardIndex, int newBoardIndex)
    {
        if (cells == null)
            return;
        

        int oldMinimapIndex = ConvertBoardIndexToMinimapIndex(oldBoardIndex);
        int newMinimapIndex = ConvertBoardIndexToMinimapIndex(newBoardIndex);

        if (newMinimapIndex >= 0 && newMinimapIndex < cells.Length && oldMinimapIndex >= 0 && oldMinimapIndex < cells.Length)
        {
            if (piecePositions.ContainsKey(oldBoardIndex))
                piecePositions.Remove(oldBoardIndex);

            if (piece != null)
            {
                piecePositions[newBoardIndex] = piece;
            }

            UpdateCellAtIndex(newMinimapIndex, oldMinimapIndex, piece);
        }
    }

    private void UpdateCellAtIndex(int index, int oldIndex, Piece piece)
    {
        if (index < 0 || index >= cells.Length)
            return;

        string marking = cells[oldIndex].GetCurrentMarking();

        cells[index].UpdateCell(piece);

        cells[index].RestoreMarking(marking);

        if(oldIndex != index)
            cells[oldIndex].Clear();
    }

    private void OnCellClicked(int cellIndex)
    {
        if (string.IsNullOrEmpty(activeMarking))
            return;

        if (cellIndex < 0 || cellIndex >= cells.Length)
            return;

        string resultMarking = cells[cellIndex].SetMarking(activeMarking);

        Piece piece = GetPieceAtMinimapIndex(cellIndex);
        if (piece != null)
        {
            if (string.IsNullOrEmpty(resultMarking))
                pieceMarkings.Remove(piece);
            else
                pieceMarkings[piece] = resultMarking;
        }
    }

    /// <summary>Finds the piece currently occupying the given minimap cell index.</summary>
    private Piece GetPieceAtMinimapIndex(int minimapIndex)
    {
        foreach (var kvp in piecePositions)
        {
            if (ConvertBoardIndexToMinimapIndex(kvp.Key) == minimapIndex)
                return kvp.Value;
        }
        return null;
    }

    public void SetActiveMarking(string marking)
    {
        activeMarking = marking;
    }

    public void ClearAllMarkings()
    {
        activeMarking = null;
        pieceMarkings.Clear();
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
