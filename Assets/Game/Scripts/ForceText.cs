using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceText : MonoBehaviour
{
    [SerializeField]
    private string sForce;
    [SerializeField]
    private TextMesh txtForce;    

    void Awake()
    {
        bool txtEmpty = sForce == "B" || sForce != "F";
        txtForce.text = txtEmpty? "" : sForce;
    }
}
