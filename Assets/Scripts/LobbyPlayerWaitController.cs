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

            // Atualiza status com o nome da sala
            if (statusText != null)
            {
                string lobbyName = SteamMatchmaking.GetLobbyData(currentLobbyID, "LobbyName");
                if (!string.IsNullOrEmpty(lobbyName))
                {
                    statusText.text = string.Format("Sala ({0})", lobbyName);
                }
                else
                {
                    statusText.text = "Aguardando jogadores...";
                }
            }

            UpdatePlayerCount();
        }

        public void OnLobbyJoined(ulong lobbyID)
        {
            currentLobbyID = new CSteamID(lobbyID);
            isInLobby = true;
            hasStartedGame = false;

            Debug.Log("[LobbyPlayerWait] Entrou no lobby: " + lobbyID);

            // Atualiza status com o nome da sala
            if (statusText != null)
            {
                string lobbyName = SteamMatchmaking.GetLobbyData(currentLobbyID, "LobbyName");
                if (!string.IsNullOrEmpty(lobbyName))
                {
                    statusText.text = string.Format("Sala ({0})", lobbyName);
                }
                else
                {
                    statusText.text = "Aguardando jogadores...";
                }
            }

            UpdatePlayerCount();
        }

        private void UpdatePlayerCount()
        {
            if (!isInLobby || currentLobbyID.m_SteamID == 0)
                return;

            currentPlayerCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);

            Debug.Log(string.Format("[LobbyPlayerWait] Jogadores no lobby: {0}/{1}", currentPlayerCount, minPlayersToStart));

            if (currentPlayerCount >= minPlayersToStart && !hasStartedGame)
            {
                if (statusText != null)
                {
                    statusText.text = "Iniciando partida...";
                }
                StartGame();
            }
        }

        private void StartGame()
        {
            hasStartedGame = true;

            Debug.Log("[LobbyPlayerWait] Iniciando jogo! Jogadores: " + currentPlayerCount);

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
