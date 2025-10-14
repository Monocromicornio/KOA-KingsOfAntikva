using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RankingEntry
{
    public string nickname;
    public int pontuation;
    public int rankingPosition;
}

public class RankingHUDManager : MonoBehaviour
{
    [Header("UI")]
    public Transform contentParent; // onde os itens serão instanciados
    public GameObject entryPrefab;  // prefab com textos de posição, nome e pontos

    private const string API_KEY = "Koa2025SecureKey!";
    private const string GET_RANKING_URL = "https://fromzerogamestudio.com/get_ranking.php";

    private void Start()
    {
        StartCoroutine(LoadRanking());
    }

    private IEnumerator LoadRanking()
    {
        string url = $"{GET_RANKING_URL}?api_key={API_KEY}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao carregar ranking: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            RankingEntry[] ranking = JsonHelper.FromJson<RankingEntry>(JsonHelper.FixJson(json));

            PopulateRanking(ranking);
        }
    }

    private void PopulateRanking(RankingEntry[] ranking)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (RankingEntry entry in ranking)
        {
            GameObject go = Instantiate(entryPrefab, contentParent);
            TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();

            // Ordem: 0 = posição, 1 = nome, 2 = pontos
            texts[0].text = entry.rankingPosition.ToString();
            texts[1].text = entry.nickname;
            texts[2].text = entry.pontuation.ToString();
        }
    }
}
