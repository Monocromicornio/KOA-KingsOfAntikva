using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Steamworks;

public class PlayerProfileManager : MonoBehaviour
{
    public static PlayerProfileManager Instance;

    
    private const string API_KEY = "Koa2025SecureKey!";
    private const string UPDATE_URL = "https://fromzerogamestudio.com/update_player.php";
    private const string GET_URL = "https://fromzerogamestudio.com/get_player.php";

    
    public ulong playerID;
    public string nickname;
    public int pontuation;
    public int rankingPosition;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        playerID = SteamUser.GetSteamID().m_SteamID;
        nickname = SteamFriends.GetPersonaName();
    }

    IEnumerator Start()
    {
       
        while (!SteamInitializer.Initialized) yield return null;

        

        yield return StartCoroutine(GetPlayerData());
    }

    
    public IEnumerator UpdatePlayerData()
    {
        WWWForm form = new WWWForm();
        form.AddField("api_key", API_KEY);
        form.AddField("player_id", playerID.ToString());
        form.AddField("nickname", nickname);
        form.AddField("pontuation", pontuation);
        form.AddField("rankingPosition", rankingPosition);

        using (UnityWebRequest www = UnityWebRequest.Post(UPDATE_URL, form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Erro ao enviar dados: " + www.error);
            else
                Debug.Log("Servidor: " + www.downloadHandler.text);
        }
    }

    
    public IEnumerator GetPlayerData()
    {
        string url = $"{GET_URL}?api_key={API_KEY}&player_id={playerID}";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao buscar dados: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            if (json.Contains("not_found"))
            {
                pontuation = 0;
                rankingPosition = 0;
                yield return StartCoroutine(UpdatePlayerData());
                yield break;
            }

            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            pontuation = data.pontuation;
            rankingPosition = data.rankingPosition;
            Debug.Log($"Perfil: {nickname} | Pontos: {pontuation} | Rank: {rankingPosition}");
        }
    }

    
    public void AddPoints(int delta)
    {
        pontuation += delta;
        if (pontuation < 0) pontuation = 0;
        StartCoroutine(UpdatePlayerData());
    }

    
    public void UpdateRankingPosition(int newRank)
    {
        rankingPosition = newRank;
        StartCoroutine(UpdatePlayerData());
    }

    [System.Serializable]
    private class PlayerData
    {
        public ulong player_id;
        public string nickname;
        public int pontuation;
        public int rankingPosition;
    }
}
