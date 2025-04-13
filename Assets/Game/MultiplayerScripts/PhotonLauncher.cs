using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace com.playsystems.koa
{
    public class PhotonLauncher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields

        #endregion

        #region Private Fields

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>
        string gameVersion = "1";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            // #Critical            
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;

            
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            Connect();
        }

        #endregion
        


        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                //PhotonNetwork.JoinRandomRoom();
                Debug.Log("Photon IsConnected");
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;

                if (PhotonNetwork.IsConnected)
                {
                    // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                    //PhotonNetwork.JoinRandomRoom();
                    Debug.Log("Photon IsConnected");
                }

            }
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public void CreateRoom(string sNameRoom)
        {

            if(!PhotonNetwork.IsConnected)
            {
                Connect();
                Debug.Log("Is Connected");
            }

            Debug.Log("Before create room - number room = " + PhotonNetwork.CountOfRooms);

            RoomOptions roomoptions = new RoomOptions();
            roomoptions.MaxPlayers = 2;

            PhotonNetwork.CreateRoom(sNameRoom, roomoptions, TypedLobby.Default);           
            
        }

        public override void OnCreatedRoom()
        {            
            Debug.Log("Room created with success");
            PhotonNetwork.LoadLevel("Lobby");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Room created failed: " + message, this);
        }

        public void SetPlayerName(string sPlayerName)
        {
            PhotonNetwork.NickName = sPlayerName;
        }

        #endregion

    }
}