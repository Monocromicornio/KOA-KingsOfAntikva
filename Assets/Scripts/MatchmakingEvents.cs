using UnityEngine;
using UnityEngine.Events;

namespace com.onlineobject.objectnet.integration
{
    public class MatchmakingEvents : MonoBehaviour
    {
        [System.Serializable]
        public class MatchmakingEvent : UnityEvent { }

        [System.Serializable]
        public class MatchmakingFloatEvent : UnityEvent<float> { }

        [Header("Events")]
        public MatchmakingEvent onSearchStarted;
        public MatchmakingEvent onSearchCancelled;
        public MatchmakingEvent onMatchFound;
        public MatchmakingEvent onLobbyCreated;
        public MatchmakingFloatEvent onSearchTimeUpdated;

        private UISteamLobbyList lobbyList;
        private bool wasSearching = false;
        private float searchTime = 0f;

        private void Awake()
        {
            lobbyList = GetComponent<UISteamLobbyList>();
        }

        private void Update()
        {
            if (lobbyList == null)
                return;

            bool isCurrentlySearching = IsSearching();

            if (isCurrentlySearching && !wasSearching)
            {
                searchTime = 0f;
                onSearchStarted?.Invoke();
            }
            else if (!isCurrentlySearching && wasSearching)
            {
                onSearchCancelled?.Invoke();
            }

            if (isCurrentlySearching)
            {
                searchTime += Time.deltaTime;
                onSearchTimeUpdated?.Invoke(searchTime);
            }

            wasSearching = isCurrentlySearching;
        }

        private bool IsSearching()
        {
            return lobbyList != null && 
                   lobbyList.searchingPanel != null && 
                   lobbyList.searchingPanel.activeSelf;
        }

        public void OnMatchFoundCallback()
        {
            onMatchFound?.Invoke();
        }

        public void OnLobbyCreatedCallback()
        {
            onLobbyCreated?.Invoke();
        }
    }
}
