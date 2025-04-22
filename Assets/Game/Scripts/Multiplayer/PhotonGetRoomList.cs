using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;

public class PhotonGetRoomList : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    
    public Button[] BtnEnterRoom;
    public GameObject ContentRoom;

    const float fTabY = 27.5f;

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UISceneRoom uisr = new UISceneRoom();

        foreach (RoomInfo info in roomList)
        {
            Debug.Log("Roo name = " + info.Name);

            CreateNewButtonRoom(info.Name);
        }
    }

    public void CreateNewButtonRoom(string sRoomName)
    {
        int iArrayLength = 0;

        iArrayLength = BtnEnterRoom.Length - 1;

        Debug.Log("iArrayLength = " + iArrayLength);

        int iNumberName = iArrayLength + 1;

        string sNameButton = BtnEnterRoom[0].name + iNumberName.ToString();

        Button newButton = Instantiate(BtnEnterRoom[iArrayLength]);

        newButton.gameObject.AddComponent<BtnJoinRoom>();

        newButton.GetComponent<BtnJoinRoom>().AddButtonEvent();

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
