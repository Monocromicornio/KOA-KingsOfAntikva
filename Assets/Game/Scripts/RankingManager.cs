using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RankingManager : MonoBehaviour
{
    private const string API_KEY = "Koa2025SecureKey!";
    private const string GET_RANKING_URL = "https://fromzerogamestudio.com/get_ranking.php";

    public static RankingData rankingData { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static async Task GetRankingData()
    {
        rankingData = Resources.Load<RankingData>("RankingData");

        if(rankingData != null)
        {
            string url = $"{GET_RANKING_URL}?api_key={API_KEY}";

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                UnityWebRequestAsyncOperation operation = www.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Erro ao carregar ranking: " + www.error);
                    return;
                }

                string json = www.downloadHandler.text;
                rankingData.UptadeRankingData(JsonHelper.FromJson<RankingEntry>(JsonHelper.FixJson(json)));
            }
        }
        else
        {
            Debug.LogError("Erro: Falha ao identificar arquivo RankingData");
        }
    }
}
