using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "RankingData", menuName = "KOA/NewRankingData")]
public class RankingData : ScriptableObject
{
    public RankingEntry[] rankingInfo { get; private set; }

    public void UptadeRankingData(RankingEntry[] _NewRanking)
    {
        rankingInfo = _NewRanking;
    }
}