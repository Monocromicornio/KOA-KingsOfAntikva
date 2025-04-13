using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTest : MonoBehaviour {

    // Use this for initialization

    public Playsystems.Table table;
    public Text text1;
    public Text text2;


    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PressButton()
    {

        //table.AddRecord(int.Parse(text1.text), text2.text);       

        table.AddRecord(text1.text, text2.text);

    }
    public void SaveButton()
    {

        //table.AddRecord(int.Parse(text1.text), text2.text);       

        table.InsertToTable();

    }

}
