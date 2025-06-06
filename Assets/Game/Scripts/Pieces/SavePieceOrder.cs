using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using com.onlineobject.objectnet;

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

    public void PressServerButton()
    {
        MatchController.connection = NetworkConnectionType.Server;
        PressButton();
    }

    public void PressClientButton()
    {
        MatchController.connection = NetworkConnectionType.Client;
        PressButton();
    }

    private void PressButton()
    {
        foreach (ToggleGameMode toggleGame in toggleGames)
        {
            if (toggleGame.toggle.isOn)
            {
                gameMode.type = toggleGame.gameType;
            }
        }
        StartCoroutine(StartSavePieces());
    }

    private IEnumerator StartSavePieces()
    {
        table.DeleteTable();
        table.SaveTable();
        while (!table.Loaded())
        {
            yield return null;
        }
        SavePieces();
    }

    private void SavePieces()
    {
        foreach (EditableField editable in editableFields)
        {
            string[] newRecord = { editable.index.ToString(), editable.piece.name.ToString() };
            table.AddRecord(newRecord);
        }
        GoToGame();
    }

    private void GoToGame()
    {
        SceneManager.LoadScene("Game");
    }
}