using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTest3 : MonoBehaviour {

    // Use this for initialization
    public Playsystems.Table table;
    public Text text1;
    public Text text2;
    public Text text3;

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PressButton()
    {

        //table.AddRecord(int.Parse(text1.text), text2.text);       

        table.EditRecord(text1.text, int.Parse(text2.text), text3.text);        

    }

    public void UpdateButton()
    {
        table.UpdateTable();
    }
}
