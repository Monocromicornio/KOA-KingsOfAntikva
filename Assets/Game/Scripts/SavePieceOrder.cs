using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SavePieceOrder : MonoBehaviour
{
    public Playsystems.Table table;

    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PressButton()
    {
        table.DeleteTable();
        table.CreateTable();

        GameMode gameMode = FindObjectOfType<GameMode>();

        if(toggle1.isOn)
        {
            gameMode.SetGameType(1);
        }
        if (toggle2.isOn)
        {
            gameMode.SetGameType(2);
        }
        if (toggle3.isOn)
        {
            gameMode.SetGameType(3);
        }


        StartCoroutine(StartSavePieces());

    }

    int iPiecesRecord = 0;

    private IEnumerator StartSavePieces()
    {
        while (iPiecesRecord == 0)
        {
            if (table.Loaded())
            {
                iPiecesRecord = table.Count();
                SavePieces();
                Debug.Log("table Count = " + table.Count());

            }
            yield return null;
        }
    }

    void SavePieces()
    {
        HousePicker[] houses = FindObjectsOfType<HousePicker>();

        foreach (HousePicker house in houses)
        {
            Debug.Log("SavePieceOrder - house.Index = " + house.Index + " - house.BusyPiece.name = " + house.BusyPiece.name);
            table.AddRecord(0, house.Index.ToString());
            table.AddRecord(1, house.BusyPiece.name.ToString());
            table.InsertToTable();
        }

        SceneManager.LoadScene("SinglePlayer");
    }

}
