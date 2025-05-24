using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnUI : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    GameObject gTurnOn;
    [SerializeField]
    GameObject gTurnOff;

    [SerializeField]
    AudioSource auTurnChange;

    MatchController turn;
    
    void Start()
    {
        turn = FindObjectOfType<MatchController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(turn.isBlueTurn)
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
