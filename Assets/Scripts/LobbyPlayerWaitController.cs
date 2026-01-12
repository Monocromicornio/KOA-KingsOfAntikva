using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace com.onlineobject.objectnet.integration
{
    public class LobbyPlayerWaitController : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject waitingForPlayersPanel;
        public TextMeshProUGUI playerCountText;
        public TextMeshProUGUI statusText;
        public Button leaveLobbyButton;

        [Header("Settings")]
        public int minPlayersToStart = 2;
        public string gameSceneName = "Game";
        public float checkInterval = 0.5f;

        private bool isInLobby = false;
        private int currentPlayerCount = 0;
        private bool hasStartedGame = false;
        private float nextCheckTime = 0f;

#if STEAMWORKS_NET
        private CSteamID currentLobbyID;

        private void Awake()
        {
            if (waitingForPlayersPanel != null)
            {
                waitingForPlayersPanel.SetActive(false);
            }

            if (leaveLobbyButton != null)
            {
                leaveLobbyButton.onClick.AddListener(OnLeaveLobby);
            }
        }

        private void Update()
        {
            if (!isInLobby || hasStartedGame)
                return;

            if (Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + checkInterval;
                UpdatePlayerCount();
            }
        }

        public void OnLobbyCreated(ulong lobbyID)
        {
            currentLobbyID = new CSteamID(lobbyID);
            isInLobby = true;
            hasStartedGame = false;

            Debug.Log("[LobbyPlayerWait] Lobby criado: " + lobbyID);

            if (waitingForPlayersPanel != null)
            {
                waitingForPlayersPanel.SetActive(true);
            }

            UpdatePlayerCount();
        }

        public void OnLobbyJoined(ulong lobbyID)
        {
            currentLobbyID = new CSteamID(lobbyID);
            isInLobby = true;
            hasStartedGame = false;

            Debug.Log("[LobbyPlayerWait] Entrou no lobby: " + lobbyID);

            if (waitingForPlayersPanel != null)
            {
                waitingForPlayersPanel.SetActive(true);
            }

            UpdatePlayerCount();
        }

        private void UpdatePlayerCount()
        {
            if (!isInLobby || currentLobbyID.m_SteamID == 0)
                return;

            currentPlayerCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);

            Debug.Log(string.Format("[LobbyPlayerWait] Jogadores no lobby: {0}/{1}", currentPlayerCount, minPlayersToStart));

            if (playerCountText != null)
            {
                playerCountText.text = string.Format("Jogadores: {0}/{1}", currentPlayerCount, minPlayersToStart);
            }

            if (statusText != null)
            {
                if (currentPlayerCount >= minPlayersToStart)
                {
                    statusText.text = "Iniciando partida...";
                }
                else
                {
                    int playersNeeded = minPlayersToStart - currentPlayerCount;
                    statusText.text = string.Format("Aguardando {0} jogador{1}...", 
                        playersNeeded, 
                        playersNeeded > 1 ? "es" : "");
                }
            }

            if (currentPlayerCount >= minPlayersToStart && !hasStartedGame)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            hasStartedGame = true;

            Debug.Log("[LobbyPlayerWait] Iniciando jogo! Jogadores: " + currentPlayerCount);

            if (waitingForPlayersPanel != null)
            {
                waitingForPlayersPanel.SetActive(false);
            }

            if (NetworkManager.Instance().IsServerConnection() && !NetworkManager.Instance().IsConnected())
            {
                NetworkManager.Instance().StartNetwork();
            }

            Debug.Log("Loading game scen via Lobby Player Wait Controller");
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnLeaveLobby()
        {
            Debug.Log("[LobbyPlayerWait] Saindo do lobby...");

            if (NetworkSteamManager.Instance() != null)
            {
                NetworkSteamManager.Instance().LeaveLobby();
            }

            isInLobby = false;
            hasStartedGame = false;
            currentLobbyID = default(CSteamID);

            if (waitingForPlayersPanel != null)
            {
                waitingForPlayersPanel.SetActive(false);
            }
        }

        public void ForceStartGame()
        {
            if (isInLobby && !hasStartedGame)
            {
                Debug.Log("[LobbyPlayerWait] Forçando início do jogo...");
                StartGame();
            }
        }
#endif
    }
}
