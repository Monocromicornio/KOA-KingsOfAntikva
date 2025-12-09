using UnityEngine;

[System.Serializable]
public class TutorialSpawnData
{
    public GameObject piecePrefab;
    public int fieldIndex;
    public bool isPlayerPiece = true;
    
    [HideInInspector]
    public MonoBehaviour spawnedPiece;
}
