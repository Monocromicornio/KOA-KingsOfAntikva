using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class RankingKOA : MonoBehaviour
{
    private RankingData _rankingData;

    private async void OnEnable()
    {
        if(_rankingData == null)
        {
            GetComponent<TMP_Text>().text = "Loading Ranking...";

            while (_rankingData == null)
            {
                _rankingData = RankingManager.rankingData;
                await Task.Yield();
            }
        }

        GetComponent<TMP_Text>().text = _rankingData.rankingInfo[0].nickname;
    }
}