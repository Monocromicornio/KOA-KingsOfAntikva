using UnityEngine;

[CreateAssetMenu(fileName = "NewBoardData", menuName = "Board/BoardData")]
public class BoardData : ScriptableObject
{
    [Header("Board Settings")]
    [Tooltip("Number of fields on the board.")]
    [Min(1)]
    public int column;

    [Tooltip("Number of rows on the board.")]
    [Min(1)]
    public int row;

    [Tooltip("Distance between fields.")]
    public float distance;

    public string[] alphabet { get; private set; }

    [HideInInspector]
    public bool isFinished;

    /// <summary>
    /// Initializes the board data with default values.
    /// </summary>
    public void Initialize(int fields, int rows, float distance)
    {
        column = fields;
        row = rows;
        this.distance = distance;
        alphabet = AlphabetHelper.GetAlphabet();
        isFinished = false;
    }
}