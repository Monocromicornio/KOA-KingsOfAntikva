using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Updates a TextMeshProUGUI element to display whose turn it is, in the current language.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TurnInfoUI : MonoBehaviour, ILocalizedByCode
{
    private static readonly LocalizedString MyTurnText = new LocalizedString("game.turn_info.my_turn", "Your Turn");
    private static readonly LocalizedString OpponentTurnText = new LocalizedString("game.turn_info.opponent_turn", "Opponent's Turn");

    private MatchController matchController => MatchController.instance;
    private TextMeshProUGUI turnText;
    private TurnState lastTurn;
    private bool isShowingMyTurn = true; // matches the default text of the TurnInfo object in the scene
    private LocalizationManager subscribedLocalizationManager;

    /// <summary>
    /// Returns every translatable text used by the turn info. Used by the localization editor to register the keys.
    /// </summary>
    public static IEnumerable<LocalizedString> GetTranslatableTexts()
    {
        yield return MyTurnText;
        yield return OpponentTurnText;
    }

    private void Awake()
    {
        turnText = GetComponent<TextMeshProUGUI>();
        lastTurn = TurnState.undefined;
    }

    private void OnEnable()
    {
        RefreshText();
        StartCoroutine(SubscribeToLanguageChangesWhenReady());
    }

    private void OnDisable()
    {
        if (subscribedLocalizationManager == null) return;

        subscribedLocalizationManager.OnLanguageChanged -= RefreshText;
        subscribedLocalizationManager = null;
    }

    private void Update()
    {
        if (matchController == null) return;

        TurnState current = matchController.currentTurn;

        if (current == lastTurn) return;
        lastTurn = current;

        if (current == TurnState.wait) return;

        isShowingMyTurn = matchController.IsMyTurn();
        RefreshText();
    }

    private IEnumerator SubscribeToLanguageChangesWhenReady()
    {
        while (LocalizationManager.Instance == null)
            yield return null;

        subscribedLocalizationManager = LocalizationManager.Instance;
        subscribedLocalizationManager.OnLanguageChanged -= RefreshText;
        subscribedLocalizationManager.OnLanguageChanged += RefreshText;
        RefreshText();
    }

    private void RefreshText()
    {
        turnText.text = (isShowingMyTurn ? MyTurnText : OpponentTurnText).GetText();
    }
}
