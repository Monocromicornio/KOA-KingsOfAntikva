using System.Collections.Generic;

/// <summary>
/// Translatable texts written from code (status labels, loading messages, result screen...).
/// Keep every new runtime text here so the localization editor can register it in the table.
/// </summary>
public static class GameTexts
{
    // ---------- Lobby / matchmaking ----------

    /// <summary>Format with the lobby name as {0}.</summary>
    public static readonly LocalizedString LobbyRoomLabelFormat = new LocalizedString("lobby.room_label_format", "Room ({0})");
    public static readonly LocalizedString LobbyWaitingForPlayers = new LocalizedString("lobby.waiting_for_players", "Waiting for players...");
    public static readonly LocalizedString LobbyStartingMatch = new LocalizedString("lobby.starting_match", "Starting match...");
    public static readonly LocalizedString LobbySearchingPlayers = new LocalizedString("lobby.searching_players", "Searching for players");
    /// <summary>Label of the Join button in the Lobby_Info prefab (a LocalizedText component with this key lives on it).</summary>
    public static readonly LocalizedString LobbyJoinButton =new LocalizedString("lobby.join_button", "Join");

    // ---------- Loading screen ----------

    public static readonly LocalizedString LoadingGeneric = new LocalizedString("loading.generic", "Loading...");
    public static readonly LocalizedString LoadingConnectingToOpponent = new LocalizedString("loading.connecting_to_opponent", "Connecting to opponent...");
    public static readonly LocalizedString LoadingSyncingData = new LocalizedString("loading.syncing_data", "Syncing data...");
    public static readonly LocalizedString LoadingPreparingMatch = new LocalizedString("loading.preparing_match", "Preparing match...");
    public static readonly LocalizedString LoadingWaitingForOpponent = new LocalizedString("loading.waiting_for_opponent", "Waiting for opponent...");
    public static readonly LocalizedString LoadingReturningToMenu = new LocalizedString("loading.returning_to_menu", "Returning to the menu...");
    public static readonly LocalizedString LoadingOfflineSession = new LocalizedString("loading.offline_session", "Loading offline session...");

    /// <summary>Messages cycled by the loading screen when no custom message is given.</summary>
    public static readonly LocalizedString[] LoadingCycleMessages =
    {
        LoadingGeneric,
        LoadingConnectingToOpponent,
        LoadingSyncingData,
        LoadingPreparingMatch,
        LoadingWaitingForOpponent,
    };

    // ---------- Profiles / ranking ----------

    public static readonly LocalizedString ProfileWaiting = new LocalizedString("profile.waiting", "Waiting...");

    /// <summary>Format with the points (or "---" when unknown) as {0}.</summary>
    public static readonly LocalizedString ProfilePointsFormat = new LocalizedString("profile.points_format", "{0} pts");
    public static readonly LocalizedString RankingLoading = new LocalizedString("ranking.loading", "Loading ranking...");

    // ---------- Result screen ----------

    /// <summary>Format with the sign as {0} and the points as {1}.</summary>
    public static readonly LocalizedString ResultInfluencePointsFormat = new LocalizedString("result.influence_points_format", "{0}{1} Influence Points");

    /// <summary>
    /// Returns every text above. Used by the localization editor to register the keys in the table.
    /// </summary>
    public static IEnumerable<LocalizedString> GetTranslatableTexts()
    {
        yield return LobbyRoomLabelFormat;
        yield return LobbyWaitingForPlayers;
        yield return LobbyStartingMatch;
        yield return LobbySearchingPlayers;
        yield return LobbyJoinButton;

        yield return LoadingGeneric;
        yield return LoadingConnectingToOpponent;
        yield return LoadingSyncingData;
        yield return LoadingPreparingMatch;
        yield return LoadingWaitingForOpponent;
        yield return LoadingReturningToMenu;
        yield return LoadingOfflineSession;

        yield return ProfileWaiting;
        yield return ProfilePointsFormat;
        yield return RankingLoading;

        yield return ResultInfluencePointsFormat;
    }
}
