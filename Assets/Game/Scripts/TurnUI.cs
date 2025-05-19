﻿using System.Collections;
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

    Turn turn;
    
    void Start()
    {
        turn = FindObjectOfType<Turn>();
    }

    // Update is called once per frame
    void Update()
    {

        if(turn.Liberate && turn.isPlayerTurn)
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
