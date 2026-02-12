using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace com.onlineobject.objectnet.integration
{
    public class SteamLobbyWaitManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject waitingPanel;
        public TextMeshProUGUI playerCountText;
        public TextMeshProUGUI statusText;
        public Button leaveLobbyButton;

        [Header("Settings")]
        public int minPlayersToStart = 2;
        public string gameSceneName = "Game";

        [Header("Optional")]
        public SavePieceOrder savePieceOrder;

        private bool isWaitingForPlayers = false;
        private bool hasStartedGame = false;

#if STEAMWORKS_NET
        private Callback<LobbyChatUpdate_t> lobbyChatUpdate;
        private CSteamID currentLobbyID;

        private void Start()
        {
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

            if (waitingPanel != null)
            {
               // waitingPanel.SetActive(false);
            }

            if (leaveLobbyButton != null)
            {
                leaveLobbyButton.onClick.AddListener(OnLeaveLobby);
            }
        }

        private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            if (!isWaitingForPlayers || hasStartedGame)
                return;

            if (callback.m_ulSteamIDLobby != currentLobbyID.m_SteamID)
                return;

            EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

            if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered ||
                stateChange == EChatMemberStateChange.k_EChatMemberStateChangeLeft ||
                stateChange == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
            {
                UpdatePlayerCount();
            }
        }

        public void StartWaitingForPlayers(ulong lobbyID)
        {
            currentLobbyID = new CSteamID(lobbyID);
            isWaitingForPlayers = true;
            hasStartedGame = false;

            Debug.Log("[SteamLobbyWait] Aguardando jogadores no lobby: " + lobbyID);

            if (waitingPanel != null)
            {
                waitingPanel.SetActive(true);
            }

            UpdatePlayerCount();
        }

        private void UpdatePlayerCount()
        {
            if (!isWaitingForPlayers || currentLobbyID.m_SteamID == 0)
                return;

            int playerCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);

            Debug.Log(string.Format("[SteamLobbyWait] Jogadores: {0}/{1}", playerCount, minPlayersToStart));

            if (playerCountText != null)
            {
                playerCountText.text = string.Format("Jogadores: {0}/{1}", playerCount, minPlayersToStart);
            }

            if (statusText != null)
            {
                if (playerCount >= minPlayersToStart)
                {
                    statusText.text = "Iniciando partida...";
                }
                else
                {
                    int needed = minPlayersToStart - playerCount;
                    statusText.text = string.Format("Aguardando {0} jogador{1}...", 
                        needed, 
                        needed > 1 ? "es" : "");
                }
            }

            if (playerCount >= minPlayersToStart && !hasStartedGame)
            {
                Invoke("StartGame", 1f);
            }
        }

        private void StartGame()
        {
            if (hasStartedGame)
                return;

            hasStartedGame = true;
            isWaitingForPlayers = false;

            Debug.Log("[SteamLobbyWait] Iniciando jogo!");

            if (waitingPanel != null)
            {
                waitingPanel.SetActive(false);
            }

            NetworkAutoLoadController.EnableAutoLoadForGame();

            if (savePieceOrder != null)
            {
                savePieceOrder.enabled = true;
                Debug.Log("[SteamLobbyWait] SavePieceOrder reabilitado");
            }

            Debug.Log("Loading game scen via Steam Lobby Wait Manager");
            SceneLoadingHandler.LoadSceneWithLoading(gameSceneName, "Iniciando partida...");
        }

        private void OnLeaveLobby()
        {
            Debug.Log("[SteamLobbyWait] Saindo do lobby...");

            if (NetworkSteamManager.Instance() != null)
            {
                NetworkSteamManager.Instance().LeaveLobby();
            }

            isWaitingForPlayers = false;
            hasStartedGame = false;
            currentLobbyID = default(CSteamID);

            if (waitingPanel != null)
            {
                waitingPanel.SetActive(false);
            }
        }
#endif
    }
}
