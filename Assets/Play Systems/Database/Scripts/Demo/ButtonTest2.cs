using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTest2 : MonoBehaviour {

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
        table.GetRecord(text1.text, int.Parse(text2.text));
    }

    public void PressButton2()
    {
        table.GetTable();
    }

}
