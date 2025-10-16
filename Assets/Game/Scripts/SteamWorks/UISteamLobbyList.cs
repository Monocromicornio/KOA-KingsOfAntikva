#if STEAMWORKS_NET
using Steamworks;
using System.Collections;
using System.Collections.Generic;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace com.onlineobject.objectnet.integration {
    /// <summary>
    /// The UILobbyList class manages the lobby UI elements and interactions in a multiplayer game.
    /// </summary>
    public class UISteamLobbyList : MonoBehaviour {

        // UI button to create a new lobby.
        public Button CreateLobbyButton;

        // UI button to refresh the list of available lobbies.
        public Button RefreshLobbyButton;

        // UI button to start auto matchmaking.
        public Button QuickPlayButton;

        // Input field for entering the name of a new lobby.
        public InputField LobbyName;

        // The root GameObject where lobby items will be instantiated.
        public GameObject LobbyItemsRoot;

        // The prefab for an individual lobby item in the list.
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

        private bool disableAutoSceneLoad = false;

        private bool isSearching = false;
        private float searchStartTime;
        private int currentRetry = 0;

#if STEAMWORKS_NET
        public ELobbyDistanceFilter[] FilterTypes = { ELobbyDistanceFilter.k_ELobbyDistanceFilterClose };
#endif

        public const string MY_LOBBY_FILTER_KEY = "MyLobbyKey";

#if STEAMWORKS_NET
        // A dictionary to keep track of the current lobbies and their associated GameObjects.
        private Dictionary<SteamLobby, GameObject> Lobbies = new Dictionary<SteamLobby, GameObject>();

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Here we are setting up listeners for the Create and Refresh lobby buttons.
        /// </summary>
        void Awake() {
            this.CreateLobbyButton.onClick.AddListener(CreateSteamLobby);
            this.RefreshLobbyButton.onClick.AddListener(RefreshLobby);

            if (this.QuickPlayButton != null)
            {
                this.QuickPlayButton.onClick.AddListener(StartQuickPlay);
            }

            if (this.cancelSearchButton != null)
            {
                this.cancelSearchButton.onClick.AddListener(CancelMatchmaking);
            }

            if (this.searchingPanel != null)
            {
                this.searchingPanel.SetActive(false);
            }
        }

        void OnEnable()
        {
            RefreshLobby();
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

        /// <summary>
        /// StartServerMode is called when the Start Server button is clicked.
        /// It configures the network manager to server mode, sets the server address, and starts the network.
        /// </summary>
        private void CreateSteamLobby()
        {
            if (string.IsNullOrEmpty(this.LobbyKey))
            {
                NetworkSteamManager.Instance().CreateLobby(this.LobbyName.text);
            }
            else
            {
                NetworkSteamManager.Instance().CreateLobby(this.LobbyName.text, (MY_LOBBY_FILTER_KEY, this.LobbyKey));
            }

            this.gameObject.SetActive(false);
        }

        private void StartQuickPlay()
        {
            isSearching = true;
            searchStartTime = Time.time;
            currentRetry = 0;
            disableAutoSceneLoad = true;

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
                    SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
                    foreach (ELobbyDistanceFilter filter in this.FilterTypes)
                    {
                        SteamMatchmaking.AddRequestLobbyListDistanceFilter(filter);
                    }
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

                if (currentRetry >= maxSearchRetries)
                {
                    CreateQuickPlayLobby();
                }
                else
                {
                    SearchForLobbies();
                }
            }
        }

        private void CreateQuickPlayLobby()
        {
            string lobbyName = "Sala_" + Random.Range(1000, 9999);

            if (string.IsNullOrEmpty(this.LobbyKey))
            {
                NetworkSteamManager.Instance().CreateLobby(lobbyName);
            }
            else
            {
                NetworkSteamManager.Instance().CreateLobby(lobbyName, (MY_LOBBY_FILTER_KEY, this.LobbyKey));
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

            if (playerWaitController != null || steamLobbyWaitManager != null)
            {
                StartCoroutine(NotifyPlayerWaitController());
            }
            else
            {
                this.gameObject.SetActive(false);
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

            this.gameObject.SetActive(false);
        }

        private void RetrySearch()
        {
            currentRetry++;

            if (currentRetry >= maxSearchRetries)
            {
                CreateQuickPlayLobby();
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

        /// <summary>
        /// Sends a request to refresh the list of available lobbies.
        /// </summary>
        private void RefreshLobby() {
            if (string.IsNullOrEmpty(this.LobbyKey)) {
                NetworkSteamManager.Instance().RequestLobbyList();
            } else {
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // To filter lobbies you can use this instead
                // Note : Using this filter the value defined on "Lobby Distance" into SteamManager will be ignored 
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////            
                NetworkSteamManager.Instance().RequestLobbyList(() => {
                    /*
                    // This is an example about how you can add multiple filter on your lobby search
                    // Use one of those of multiple to filter the listed lobbies                    
                    SteamMatchmaking.AddRequestLobbyListStringFilter("KeyToMatch", "ValueToMatch", ELobbyComparison.k_ELobbyComparisonEqual);
                    SteamMatchmaking.AddRequestLobbyListNearValueFilter("KeyToMatch", 100);
                    SteamMatchmaking.AddRequestLobbyListNumericalFilter("KeyToMatch", 1, ELobbyComparison.k_ELobbyComparisonEqual);
                    SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(4); // At least 4 slots avaiable
                    */
                    // I'm going to filter by my lobby to9 list only lobbyes relatred to by game ( because i'm using public STEAM ID )
                    SteamMatchmaking.AddRequestLobbyListStringFilter(MY_LOBBY_FILTER_KEY, this.LobbyKey, ELobbyComparison.k_ELobbyComparisonEqual);
                    SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1); // At least 1 slots avaiable
                    foreach (ELobbyDistanceFilter filter in this.FilterTypes) {
                        SteamMatchmaking.AddRequestLobbyListDistanceFilter(filter);
                    }                    
                });
            }
        }

        /// <summary>
        /// LateUpdate is called every frame, if the MonoBehaviour is enabled.
        /// It is used here to update the lobby list UI based on the latest lobby information.
        /// </summary>
        private void LateUpdate() {
            // Only update the lobby list if not in relay mode.
            if (NetworkManager.Instance().InEmbeddedMode()) {
                // Add new lobbies to the UI.
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
                                // Deactivate the current game object (likely the UI panel).
                                this.gameObject.SetActive(false);
                                //SceneManager.LoadScene("Game");
                            }); // Send lobby join request.
                        });
                        newItem.transform.SetParent(this.LobbyItemsRoot.transform, false);
                        this.Lobbies.Add(lobby, newItem);
                    }
                }
                // Remove lobbies that are no longer available.
                List<SteamLobby> removedLobbies = new List<SteamLobby>();
                foreach (SteamLobby lobby in this.Lobbies.Keys) {
                    bool found = false;
                    foreach (SteamLobby lobbyData in NetworkSteamManager.Instance().GetLobbies()) {
                        found |= (lobby.Equals(lobbyData));
                    }
                    if (!found) {
                        removedLobbies.Add(lobby);
                    }
                }
                while (removedLobbies.Count > 0) {
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