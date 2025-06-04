using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPieceData", menuName = "KOA/PieceData")]
public class PieceData : ScriptableObject
{
    [System.Serializable]
    public struct PieceConfig
    {
        public string name;
        public Piece prefab;
        public FakePiece fakePrefab;
    }

    public GameObject fakePiece;
    public List<PieceConfig> pieces;
}