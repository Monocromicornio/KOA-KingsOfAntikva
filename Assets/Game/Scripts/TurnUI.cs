using UnityEngine;

public class TurnUI : MonoBehaviour
{
    [SerializeField]
    GameObject gTurnOn;
    [SerializeField]
    GameObject gTurnOff;

    [SerializeField]
    AudioSource auTurnChange;

    MatchController matchController => MatchController.instance;

    void Update()
    {
        if(matchController.turn == TurnState.homeTeam)
        {
            gTurnOn.SetActive(true);
            gTurnOff.SetActive(false);
        }
        else
        {
            gTurnOn.SetActive(false);
            gTurnOff.SetActive(true);
        }
    }
}
