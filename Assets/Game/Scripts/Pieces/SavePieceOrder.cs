using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using com.onlineobject.objectnet;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(GameMode))]
public class SavePieceOrder : MonoBehaviour
{
    public TableData table;

    [System.Serializable]
    public struct ToggleGameMode
    {
        public Toggle toggle;
        public GameMode.GameType gameType;
    }
    private BoardController board => BoardController.instance;
    private EditableField[] editableFields => board.editableFields;


    [SerializeField]
    GameMode gameMode;

    [SerializeField]
    ToggleGameMode[] toggleGames;

    public void Server()
    {
        PressButton(() => GoToGame(true));
    }

    public void Client()
    {
        PressButton(() => GoToGame(false));
    }

    public void Offline()
    {
        MatchEvents.SetRankedMatch(false);
        PressButton(() => SceneLoadingHandler.LoadSceneWithLoading("Game", "Carregando partida offline..."));
    }

    public void SavePieces()
    {
        PressButton(null);
    }

    private void PressButton(UnityAction action)
    {
        foreach (ToggleGameMode toggleGame in toggleGames)
        {
            if (toggleGame.toggle.isOn)
            {
                gameMode.type = toggleGame.gameType;
            }
        }
        StartCoroutine(StartSavePieces(action));
    }

    private IEnumerator StartSavePieces(UnityAction action)
    {
        table.DeleteTable();
        table.SaveTable();
        while (!table.Loaded()) yield return null;

        Save();

        if (action != null) action.Invoke();
    }

    private void Save()
    {
        foreach (EditableField editable in editableFields)
        {
            string[] newRecord = { editable.index.ToString(), editable.piece.name.ToString() };
            table.AddRecord(newRecord);
        }
    }

    private void GoToGame(bool isServer)
    {
        var networkManager = NetworkManager.Instance();

        networkManager.ConfigureMode(isServer ? NetworkConnectionType.Server : NetworkConnectionType.Client);
        networkManager.SetServerAddress("127.0.0.1");
        networkManager.StartNetwork();

        //SceneManager.LoadScene("Game");
    }

    public void OnConnected(IClient client)
    {
        Debug.Log("Piece Order on Connected Callback");

       // SceneManager.LoadScene("Game");
    }

    public void OnServer(IChannel channel)
    {
        Debug.Log("Piece Order on Sever Callback");
        //SceneManager.LoadScene("Game");
    }
}