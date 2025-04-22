using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject gButtonStart;
    public GameObject gButtonKick;
    public Text TxtPlayerName;
    public string StrNameScene;
    
    // Start is called before the first frame update
    void Start()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            gButtonStart.SetActive(false);
            gButtonKick.SetActive(false);
        }


    }

    //public override void OnJoinedRoom()
    //{
    //    foreach (var playersName in PhotonNetwork.PlayerList)
    //    {
    //        Debug.Log(playersName + " is in the room");
    //        // Add a button for each player in the room.
    //        // You can use p.NickName to access the player's nick name.
    //    }
    //}

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        foreach (var playersName in PhotonNetwork.PlayerList)
        {
            Debug.Log("NickName = " + playersName.NickName + " - ActorNumber = " + playersName.ActorNumber);
            
            // Add a button for each player in the room.
            // You can use p.NickName to access the player's nick name.
        }

        TxtPlayerName.text = PhotonNetwork.PlayerList[PhotonNetwork.PlayerList.Length-1].NickName;
        //base.OnPlayerEnteredRoom(newPlayer);
    }

    public void KickPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //var targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.PlayerList[1].ActorNumber);

            if (PhotonNetwork.PlayerList[1] != null)
            {
                PhotonNetwork.EnableCloseConnection = true;
                PhotonNetwork.CloseConnection(PhotonNetwork.PlayerList[1]);
                TxtPlayerName.text = "";


            }
            else
            {
                Debug.Log("O jogador especificado não está na sala.");
            }
        }
        else
        {
            Debug.Log("Apenas o cliente mestre pode remover jogadores da sala.");
        }

        foreach (var playersName in PhotonNetwork.PlayerList)
        {
            Debug.Log("Player Kicked = " + playersName.NickName + " - ActorNumber = " + playersName.ActorNumber);

            // Add a button for each player in the room.
            // You can use p.NickName to access the player's nick name.
        }
    }

    public void SceneOpen()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //int idScene = SceneManager.GetSceneByName(StrNameScene).buildIndex;

            //Debug.Log("idScene = " + idScene);

            PhotonNetwork.LoadLevel(StrNameScene);
        }
    }

}
