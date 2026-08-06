using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

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

    private RankingData rankingData;
    private CancellationTokenSource _cts;

    private void OnEnable()
    {
        rankingData = RankingManager.rankingData;
        _cts = new CancellationTokenSource();
        _ = PopulateRanking(_cts.Token);
    }

    private async Task PopulateRanking(CancellationToken token)
    {
        if (RankingManager.rankingData != null)
        {

            foreach (Transform child in contentParent)
            {
                token.ThrowIfCancellationRequested();

                Destroy(child.gameObject);
                await Task.Yield();
            }

            foreach (RankingEntry entry in rankingData.rankingInfo)
            {
                token.ThrowIfCancellationRequested();

                GameObject go = Instantiate(entryPrefab, contentParent);
                TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();

                // Ordem: 0 = posição, 1 = nome, 2 = pontos
                texts[0].text = entry.rankingPosition.ToString();
                texts[1].text = entry.nickname;
                texts[2].text = entry.pontuation.ToString();
                await Task.Yield();
            }
        }
        else
        {
            Debug.LogError("RankingData não identificado");
        }
    }

    private void LimpaToken()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void OnDestroy()
    {
        LimpaToken();
    }

    private void OnDisable()
    {
        LimpaToken();
    }
}