using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace com.onlineobject.objectnet.integration
{
    public class AutoMatchmaking : MonoBehaviour
    {
        [Header("UI References")]
        public Button playButton;
        public GameObject searchingPanel;
        public Text searchTimerText;
        public Button cancelButton;

        [Header("Settings")]
        public string lobbyKey = "MyObjectNetGameName";
        public float lobbyCheckInterval = 1.5f;
        public int maxRetries = 5;

        public const string MY_LOBBY_FILTER_KEY = "MyLobbyKey";

        private bool isSearching = false;
        private float searchStartTime;
        private int currentRetry = 0;

#if STEAMWORKS_NET
        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelMatchmaking);
            }

            if (searchingPanel != null)
            {
                searchingPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (isSearching && searchTimerText != null)
            {
                float elapsedTime = Time.time - searchStartTime;
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                searchTimerText.text = string.Format("Buscando partida... {0:00}:{1:00}", minutes, seconds);
            }
        }

        private void OnPlayButtonClicked()
        {
            if (isSearching)
            {
                Debug.Log("[AutoMatchmaking] Já estamos buscando uma partida");
                return;
            }
            
            var networkManager = NetworkManager.Instance();
            
            if (networkManager != null && networkManager.HasConnection())
            {
                Debug.Log("[AutoMatchmaking] Já existe uma conexão ativa. Desconectando antes de buscar nova partida...");
                
                try
                {
                    NetworkSteamManager.Instance().LeaveLobby();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AutoMatchmaking] Erro ao sair do lobby: {e.Message}");
                }
                
                try
                {
                    networkManager.StopNetwork();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AutoMatchmaking] Erro ao parar rede: {e.Message}");
                }
                
                StartCoroutine(WaitAndStartMatchmaking(0.5f));
            }
            else
            {
                SyncronizeTable.ResetAll();
                StartMatchmaking();
            }
        }
        
        private IEnumerator WaitAndStartMatchmaking(float delay)
        {
            yield return new WaitForSeconds(delay);
            SyncronizeTable.ResetAll();
            StartMatchmaking();
        }

        private void StartMatchmaking()
        {
            isSearching = true;
            searchStartTime = Time.time;
            currentRetry = 0;

            if (searchingPanel != null)
            {
                searchingPanel.SetActive(true);
            }

            SearchForLobbies();
        }

        private void SearchForLobbies()
        {
            if (string.IsNullOrEmpty(lobbyKey))
            {
                NetworkSteamManager.Instance().RequestLobbyList();
            }
            else
            {
                NetworkSteamManager.Instance().RequestLobbyList(() =>
                {
                    SteamMatchmaking.AddRequestLobbyListStringFilter(MY_LOBBY_FILTER_KEY, lobbyKey, ELobbyComparison.k_ELobbyComparisonEqual);
                    SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
                });
            }

            StartCoroutine(CheckForAvailableLobbies());
        }

        private IEnumerator CheckForAvailableLobbies()
        {
            yield return new WaitForSeconds(lobbyCheckInterval);

            if (!isSearching)
                yield break;

            SteamLobby[] lobbies = NetworkSteamManager.Instance().GetLobbies();

            if (lobbies != null && lobbies.Length > 0)
            {
                NetworkSteamManager.Instance().RequestToJoin(lobbies[0].SteamId, (bool joined) =>
                {
                    if (joined)
                    {
                        OnMatchFound();
                    }
                    else
                    {
                        RetrySearch();
                    }
                });
            }
            else
            {
                currentRetry++;
                
                if (currentRetry >= maxRetries)
                {
                    CreateNewLobby();
                }
                else
                {
                    SearchForLobbies();
                }
            }
        }

        private void CreateNewLobby()
        {
            string lobbyName = "Sala_" + Random.Range(1000, 9999);

            if (string.IsNullOrEmpty(lobbyKey))
            {
                NetworkSteamManager.Instance().CreateLobby(lobbyName);
            }
            else
            {
                NetworkSteamManager.Instance().CreateLobby(lobbyName, (MY_LOBBY_FILTER_KEY, lobbyKey));
            }

            OnMatchFound();
        }

        private void OnMatchFound()
        {
            isSearching = false;

            if (searchingPanel != null)
            {
                searchingPanel.SetActive(false);
            }

            StopAllCoroutines();
        }

        private void RetrySearch()
        {
            currentRetry++;
            
            if (currentRetry >= maxRetries)
            {
                CreateNewLobby();
            }
            else
            {
                SearchForLobbies();
            }
        }

        public void CancelMatchmaking()
        {
            isSearching = false;
            currentRetry = 0;

            if (searchingPanel != null)
            {
                searchingPanel.SetActive(false);
            }

            StopAllCoroutines();
        }
#endif
    }
}
