using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class BtnJoinRoom : MonoBehaviour
{
    Button bJoinRoom;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickJoinRoom()
    {
        string roomname;

        roomname = transform.GetChild(0).gameObject.GetComponent<Text>().text;

        Debug.Log("roomname = " + roomname);

        PhotonNetwork.JoinRoom(roomname);
    }

    public void AddButtonEvent()
    {

        bJoinRoom = GetComponent<Button>();
        //Calls the TaskOnClick method when you click the Button
        bJoinRoom.onClick.AddListener(ClickJoinRoom);
    }
}
