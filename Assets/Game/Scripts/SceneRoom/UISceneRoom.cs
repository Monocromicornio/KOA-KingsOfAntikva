using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;

public class UISceneRoom : MonoBehaviourPunCallbacks
{
    [SerializeField]
    com.playsystems.koa.PhotonLauncher PLauncher;

    [SerializeField]
    GameObject GoFormPlayerName;
    [SerializeField]
    GameObject GoFormSelect;
    [SerializeField]
    GameObject GoFormCreateRoom;
    [SerializeField]
    GameObject GoFormSelectRoom;

    [SerializeField]
    Button[] BtnEnterRoom;

    [SerializeField]
    GameObject ContentRoom;

    [SerializeField] InputField TxtRoomName;
    [SerializeField] InputField TxtPlayerName;


    const float fTabY = 27.5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BtnCreateRoom()
    {

        PLauncher.CreateRoom(TxtRoomName.text);

    }


    public void BtnSetPlayerName()
    {
        PLauncher.SetPlayerName(TxtPlayerName.text);
        PlayerPrefs.SetString("PlayerName", TxtPlayerName.text);
        GoFormPlayerName.SetActive(false);
        GoFormSelect.SetActive(true);
    }

    public void BtnOpenFormCreateRoom()
    {
        GoFormSelect.SetActive(false);
        GoFormCreateRoom.SetActive(true);
    }

    public void BtnOpenFormEnterRoom()
    {
        GoFormSelect.SetActive(false);
        GoFormSelectRoom.SetActive(true);
    }

    public void BtnCreateButton()
    {        

        //GameObject buttonGameObject = new GameObject(sNameButton);                

        int iArrayLength = 0;        

        iArrayLength = BtnEnterRoom.Length - 1;

        Debug.Log("iArrayLength = " + iArrayLength);

        int iNumberName = iArrayLength + 1;

        string sNameButton = BtnEnterRoom[0].name + iNumberName.ToString();

        Button newButton = Instantiate(BtnEnterRoom[iArrayLength]);

        newButton.name = sNameButton;

        Text TxtName = newButton.transform.GetChild(0).GetComponent<Text>();

        TxtName.text = sNameButton;

        Array.Resize(ref BtnEnterRoom, BtnEnterRoom.Length + 1);
        BtnEnterRoom[iArrayLength+1] = newButton;

        //Button buttonComponent = buttonGameObject.AddComponent<Button>();

        // Configure a posição e o tamanho do botão na tela
        newButton.transform.SetParent(ContentRoom.transform); // Anexe o botão ao objeto pai
        newButton.transform.position = new Vector3(BtnEnterRoom[iArrayLength].transform.position.x, BtnEnterRoom[iArrayLength].transform.position.y - fTabY, 0);; // Posicione o botão na tela
        //newButton.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50); // Defina o tamanho do botão

        Debug.Log("Mathf.Sin(27.5f) = " + Mathf.Sin(27.5f));

    }

    public void CreateNewButtonRoom(string sRoomName )
    {
        int iArrayLength = 0;

        iArrayLength = BtnEnterRoom.Length - 1;

        Debug.Log("iArrayLength = " + iArrayLength);

        int iNumberName = iArrayLength + 1;

        string sNameButton = BtnEnterRoom[0].name + iNumberName.ToString();

        Button newButton = Instantiate(BtnEnterRoom[iArrayLength]);

        newButton.name = sNameButton;

        Text TxtName = newButton.transform.GetChild(0).GetComponent<Text>();

        TxtName.text = sRoomName;

        Array.Resize(ref BtnEnterRoom, BtnEnterRoom.Length + 1);
        BtnEnterRoom[iArrayLength + 1] = newButton;        

        // Configure a posição e o tamanho do botão na tela
        newButton.transform.SetParent(ContentRoom.transform); // Anexe o botão ao objeto pai
        newButton.transform.position = new Vector3(BtnEnterRoom[iArrayLength].transform.position.x, BtnEnterRoom[iArrayLength].transform.position.y - fTabY, 0); ; // Posicione o botão na tela
        
    }
}
