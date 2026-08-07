using TMPro;
using UnityEngine;

/// <summary>
/// Updates a TextMeshProUGUI element to display whose turn it is.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TurnInfoUI : MonoBehaviour
{
    private const string MY_TURN_TEXT = "Your Turn";
    private const string ENEMY_TURN_TEXT = "Opponent's Turn";

    private MatchController matchController => MatchController.instance;
    private TextMeshProUGUI turnText;
    private TurnState lastTurn;

    private void Awake()
    {
        turnText = GetComponent<TextMeshProUGUI>();
        lastTurn = TurnState.undefined;
    }

    private void Update()
    {
        if (matchController == null) return;

        TurnState current = matchController.currentTurn;

        if (current == lastTurn) return;
        lastTurn = current;

        if (current == TurnState.wait) return;

        turnText.text = matchController.IsMyTurn() ? MY_TURN_TEXT : ENEMY_TURN_TEXT;
    }
}
