#if STEAMWORKS_NET
using Steamworks;
using System.Collections;
using System.Collections.Generic;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace com.onlineobject.objectnet.integration
{
    public class UISteamLobbyList : MonoBehaviour
    {
        public Button CreateLobbyButton;

        public Button RefreshLobbyButton;

        public Button QuickPlayButton;

        public TMP_InputField LobbyName;

        public GameObject LobbyItemsRoot;

        public GameObject LobbyItem;

        public string LobbyKey = "MyObjectNetGameName";

        [Header("Auto Matchmaking")]
        public GameObject searchingPanel;
        public TextMeshProUGUI searchTimerText;
        public Button cancelSearchButton;
        public float lobbyCheckInterval = 1.5f;
        public int maxSearchRetries = 5;

        [Header("Player Wait System")]
        public LobbyPlayerWaitController playerWaitController;
        public SteamLobbyWaitManager steamLobbyWaitManager;
        public SavePieceOrder savePieceOrder;

        [Header("Lobby Settings")]
        public int maxPlayersPerLobby = 2;

        private bool disableAutoSceneLoad = false;

        private bool isSearching = false;
        private float searchStartTime;
        private int currentRetry = 0;

#if STEAMWORKS_NET
        public ELobbyDistanceFilter[] FilterTypes = { ELobbyDistanceFilter.k_ELobbyDistanceFilterClose };
#endif

        public const string MY_LOBBY_FILTER_KEY = "MyLobbyKey";

#if STEAMWORKS_NET        
        private Dictionary<SteamLobby, GameObject> Lobbies = new Dictionary<SteamLobby, GameObject>();

        void Awake()
        {
            SetMaxPlayers(maxPlayersPerLobby);
        }

        void Start()
        {
            CreateLobbyButton.onClick.AddListener(CreateSteamLobby);
            RefreshLobbyButton.onClick.AddListener(RefreshLobby);

            if (QuickPlayButton != null)
            {
               QuickPlayButton.onClick.AddListener(StartQuickPlay);
            }

            cancelSearchButton.onClick.AddListener(CancelMatchmaking);

            if (searchingPanel != null)
            {
                this.searchingPanel.SetActive(false);
            }

            RefreshLobby();
        }
        private void SetMaxPlayers(int maxPlayers)
        {
            var networkSteamManager = NetworkSteamManager.Instance();
            if (networkSteamManager != null)
            {
                var field = networkSteamManager.GetType().GetField("maximumOfPlayers",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(networkSteamManager, maxPlayers);
                    Debug.Log($"[UISteamLobbyList] Limite de jogadores configurado para: {maxPlayers}");
                }
            }
        }

        private IEnumerator EnforceLobbyMemberLimit(int maxPlayers)
        {
            yield return new WaitForSeconds(0.3f);

            var networkSteamManager = NetworkSteamManager.Instance();
            if (networkSteamManager != null)
            {
                var field = networkSteamManager.GetType().GetField("currentLobbyID",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    ulong currentLobbyID = (ulong)field.GetValue(networkSteamManager);
                    if (currentLobbyID != 0)
                    {
                        CSteamID lobbyID = new CSteamID(currentLobbyID);
                        bool success = SteamMatchmaking.SetLobbyMemberLimit(lobbyID, maxPlayers);

                        int actualLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
                        Debug.Log($"[UISteamLobbyList] ✓ SetLobbyMemberLimit({maxPlayers}) aplicado! Limite real: {actualLimit}");
                    }
                    else
                    {
                        Debug.LogWarning("[UISteamLobbyList] Tentando novamente configurar limite...");
                        yield return new WaitForSeconds(0.3f);

                        currentLobbyID = (ulong)field.GetValue(networkSteamManager);
                        if (currentLobbyID != 0)
                        {
                            CSteamID lobbyID = new CSteamID(currentLobbyID);
                            SteamMatchmaking.SetLobbyMemberLimit(lobbyID, maxPlayers);
                            Debug.Log($"[UISteamLobbyList] ✓ Limite configurado na 2ª tentativa: {maxPlayers}");
                        }
                    }
                }
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

        private void CreateSteamLobby()
        {
            SetMaxPlayers(maxPlayersPerLobby);

            NetworkAutoLoadController.DisableAutoLoadForMatchmaking();

            if (savePieceOrder != null)
            {
                savePieceOrder.enabled = false;
                Debug.Log("[UISteamLobbyList] SavePieceOrder desabilitado para criação manual");
            }

            if (string.IsNullOrEmpty(this.LobbyKey))
            {
                NetworkSteamManager.Instance().CreateLobby(this.LobbyName.text, ("ranked", "no"));
            }
            else
            {
                NetworkSteamManager.Instance().CreateLobby(this.LobbyName.text, (MY_LOBBY_FILTER_KEY, this.LobbyKey), ("ranked" , "no"));
            }

            Debug.Log($"[UISteamLobbyList] Criando lobby manual com limite de {maxPlayersPerLobby} jogadores");
            MatchEvents.SetRankedMatch(false);
            StartCoroutine(EnforceLobbyMemberLimit(maxPlayersPerLobby));
            StartCoroutine(NotifyPlayerWaitController());
        }

        private void StartQuickPlay()
        {
            isSearching = true;
            searchStartTime = Time.time;
            currentRetry = 0;
            disableAutoSceneLoad = true;

            SetMaxPlayers(maxPlayersPerLobby);

            NetworkAutoLoadController.DisableAutoLoadForMatchmaking();

            if (savePieceOrder != null)
            {
                savePieceOrder.enabled = false;
                Debug.Log("[UISteamLobbyList] SavePieceOrder desabilitado para matchmaking");
            }

            if (searchingPanel != null)
            {
                searchingPanel.SetActive(true);
            }

            Debug.Log($"[UISteamLobbyList] Iniciando busca por lobbies com {maxPlayersPerLobby} jogadores...");
            SearchForLobbies();
        }

        private void SearchForLobbies()
        {
            if (string.IsNullOrEmpty(this.LobbyKey))
            {
                NetworkSteamManager.Instance().RequestLobbyList();
            }
            else
            {
                NetworkSteamManager.Instance().RequestLobbyList(() =>
                {
                    SteamMatchmaking.AddRequestLobbyListStringFilter(MY_LOBBY_FILTER_KEY, this.LobbyKey, ELobbyComparison.k_ELobbyComparisonEqual);
                    SteamMatchmaking.AddRequestLobbyListStringFilter("ranked", "yes", ELobbyComparison.k_ELobbyComparisonEqual);
                    SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
                    foreach (ELobbyDistanceFilter filter in this.FilterTypes)
                    {
                        SteamMatchmaking.AddRequestLobbyListDistanceFilter(filter);
                    }
                });
            }

            MatchEvents.SetRankedMatch(true);
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
                bool foundAvailableLobby = false;

                foreach (SteamLobby lobby in lobbies)
                {
                    CSteamID lobbyID = new CSteamID((ulong)lobby.SteamId);
                    int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
                    int memberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);

                    Debug.Log($"[UISteamLobbyList] Lobby {lobby.SteamId}: {numMembers}/{memberLimit} jogadores");

                    if (numMembers < memberLimit || memberLimit == 0)
                    {
                        foundAvailableLobby = true;
                        Debug.Log($"[UISteamLobbyList] Tentando entrar no lobby {lobby.SteamId}...");

                        bool callbackReceived = false;
                        bool joinSucceeded = false;

                        NetworkSteamManager.Instance().RequestToJoin(lobby.SteamId, (bool joined) =>
                        {
                            callbackReceived = true;
                            joinSucceeded = joined;

                            if (joined)
                            {
                                Debug.Log($"[UISteamLobbyList] ✓ Entrou no lobby {lobby.SteamId}!");
                                OnMatchFound();
                            }
                            else
                            {
                                Debug.LogWarning($"[UISteamLobbyList] ✗ Falha ao entrar no lobby {lobby.SteamId}");
                            }
                        });

                        float timeout = 3f;
                        float elapsed = 0f;

                        while (!callbackReceived && elapsed < timeout)
                        {
                            elapsed += Time.deltaTime;
                            yield return null;
                        }

                        if (!callbackReceived)
                        {
                            Debug.LogWarning($"[UISteamLobbyList] Timeout ao tentar entrar no lobby {lobby.SteamId}");
                        }

                        if (joinSucceeded)
                        {
                            yield break;
                        }

                        break;
                    }
                    else
                    {
                        Debug.Log($"[UISteamLobbyList] Lobby {lobby.SteamId} está cheio ({numMembers}/{memberLimit}), pulando...");
                    }
                }

                if (!foundAvailableLobby)
                {
                    Debug.Log("[UISteamLobbyList] Nenhum lobby disponível encontrado");
                    RetrySearch();
                }
                else if (!isSearching)
                {
                    yield break;
                }
                else
                {
                    RetrySearch();
                }
            }
            else
            {
                currentRetry++;

                if (currentRetry >= maxSearchRetries)
                {
                    Debug.Log($"[UISteamLobbyList] Nenhum lobby encontrado após {currentRetry} tentativas. Criando novo lobby...");
                    CreateQuickPlayLobby();
                }
                else
                {
                    Debug.Log($"[UISteamLobbyList] Nenhum lobby encontrado. Tentativa {currentRetry}/{maxSearchRetries}");
                    SearchForLobbies();
                }
            }
        }

        private void CreateQuickPlayLobby()
        {
            SetMaxPlayers(maxPlayersPerLobby);

            string lobbyName = "Sala_" + Random.Range(1000, 9999);

            if (string.IsNullOrEmpty(this.LobbyKey))
            {
                NetworkSteamManager.Instance().CreateLobby(lobbyName, ("ranked", "yes"));
            }
            else
            {
                NetworkSteamManager.Instance().CreateLobby(lobbyName, (MY_LOBBY_FILTER_KEY, this.LobbyKey),("ranked", "yes"));
            }

            MatchEvents.SetRankedMatch(true);
            Debug.Log($"[UISteamLobbyList] Criando lobby Quick Play '{lobbyName}' com limite de {maxPlayersPerLobby} jogadores");
            StartCoroutine(EnforceLobbyMemberLimit(maxPlayersPerLobby));
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

            if (playerWaitController != null || steamLobbyWaitManager != null)
            {
                StartCoroutine(NotifyPlayerWaitController());
            }
            else
            {
               // this.gameObject.SetActive(false);
            }
        }

        private IEnumerator NotifyPlayerWaitController()
        {
            yield return new WaitForSeconds(0.5f);

            if (NetworkSteamManager.Instance() != null)
            {
                System.Reflection.FieldInfo field = NetworkSteamManager.Instance().GetType()
                    .GetField("currentLobbyID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    ulong currentLobbyID = (ulong)field.GetValue(NetworkSteamManager.Instance());

                    if (currentLobbyID != 0)
                    {
                        if (steamLobbyWaitManager != null)
                        {
                            steamLobbyWaitManager.StartWaitingForPlayers(currentLobbyID);
                            Debug.Log("[UISteamLobbyList] SteamLobbyWaitManager notificado com lobby: " + currentLobbyID);
                        }
                        else if (playerWaitController != null)
                        {
                            if (NetworkManager.Instance().IsServerConnection())
                            {
                                playerWaitController.OnLobbyCreated(currentLobbyID);
                            }
                            else
                            {
                                playerWaitController.OnLobbyJoined(currentLobbyID);
                            }
                        }
                    }
                }
            }

          //  this.gameObject.SetActive(false);
        }

        private void RetrySearch()
        {
            if (!isSearching)
            {
                Debug.Log("[UISteamLobbyList] Busca cancelada pelo usuário");
                return;
            }

            currentRetry++;

            if (currentRetry >= maxSearchRetries)
            {
                Debug.Log($"[UISteamLobbyList] Limite de tentativas atingido ({currentRetry}). Criando novo lobby...");
                CreateQuickPlayLobby();
            }
            else
            {
                Debug.Log($"[UISteamLobbyList] Tentando novamente... ({currentRetry}/{maxSearchRetries})");
                SearchForLobbies();
            }
        }

        public void CancelMatchmaking()
        {
            Debug.Log("Cancelling matchmaking");
            isSearching = false;
            currentRetry = 0;

            if (searchingPanel != null)
            {
                searchingPanel.SetActive(false);
            }

            StopAllCoroutines();
        }

        private void RefreshLobby()
        {
            if (string.IsNullOrEmpty(this.LobbyKey))
            {
                NetworkSteamManager.Instance().RequestLobbyList();
            }
            else
            {
                NetworkSteamManager.Instance().RequestLobbyList(() => {
                    SteamMatchmaking.AddRequestLobbyListStringFilter(MY_LOBBY_FILTER_KEY, this.LobbyKey, ELobbyComparison.k_ELobbyComparisonEqual);
                    SteamMatchmaking.AddRequestLobbyListStringFilter("ranked", "no", ELobbyComparison.k_ELobbyComparisonEqual);
                    SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
                    foreach (ELobbyDistanceFilter filter in this.FilterTypes)
                    {
                        SteamMatchmaking.AddRequestLobbyListDistanceFilter(filter);
                    }
                });
            }
            MatchEvents.SetRankedMatch(false);
        }

        private void LateUpdate()
        {

            if (NetworkManager.Instance().InEmbeddedMode())
            {
                foreach (SteamLobby lobby in NetworkSteamManager.Instance().GetLobbies())
                {
                    if (!this.Lobbies.ContainsKey(lobby))
                    {
                        GameObject newItem = Instantiate(this.LobbyItem);
                        UILobbyItem lobbyItem = newItem.GetComponent<UILobbyItem>();
                        lobbyItem.label.text = lobby["LobbyName"];
                        lobbyItem.button.onClick.AddListener(() =>
                        {
                            NetworkSteamManager.Instance().RequestToJoin(lobby.SteamId, (bool joined) =>
                            {
                                if (joined)
                                {
                                    NetworkAutoLoadController.DisableAutoLoadForMatchmaking();

                                    if (savePieceOrder != null)
                                    {
                                        savePieceOrder.enabled = false;
                                        Debug.Log("[UISteamLobbyList] SavePieceOrder desabilitado para join manual");
                                    }

                                    StartCoroutine(NotifyPlayerWaitController());
                                }
                                else
                                {
                                    Debug.LogWarning("[UISteamLobbyList] Falha ao entrar no lobby");
                                   // this.gameObject.SetActive(false);
                                }
                            });
                        });
                        newItem.transform.SetParent(this.LobbyItemsRoot.transform, false);
                        this.Lobbies.Add(lobby, newItem);
                    }
                }

                List<SteamLobby> removedLobbies = new List<SteamLobby>();
                foreach (SteamLobby lobby in this.Lobbies.Keys)
                {
                    bool found = false;
                    foreach (SteamLobby lobbyData in NetworkSteamManager.Instance().GetLobbies())
                    {
                        found |= (lobby.Equals(lobbyData));
                    }
                    if (!found)
                    {
                        removedLobbies.Add(lobby);
                    }
                }
                while (removedLobbies.Count > 0)
                {
                    GameObject objToRemove = this.Lobbies[removedLobbies[0]];
                    this.Lobbies.Remove(removedLobbies[0]);
                    removedLobbies.RemoveAt(0);
                    objToRemove.transform.SetParent(null);
                    Destroy(objToRemove);
                }
            }
        }
#endif
    }

}
