using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [SerializeField]
    GameMode gameMode;

    [SerializeField]
    ToggleGameMode[] toggleGames;

    public void PressButton()
    {
        table.DeleteTable();
        table.SaveTable();

        foreach (ToggleGameMode toggleGame in toggleGames)
        {
            if (toggleGame.toggle.isOn)
            {
                gameMode.SetGameType(toggleGame.gameType);
            }
        }

        StartCoroutine(StartSavePieces());
    }

    int iPiecesRecord = 0;

    private IEnumerator StartSavePieces()
    {
        while (iPiecesRecord == 0)
        {
            if (table.Loaded())
            {
                iPiecesRecord = table.Count();
                SavePieces();
                Debug.Log("table Count = " + table.Count());
            }
            yield return null;
        }
    }

    void SavePieces()
    {
        HousePicker[] houses = FindObjectsOfType<HousePicker>();

        foreach (HousePicker house in houses)
        {
            Debug.Log("SavePieceOrder - house.Index = " + house.index + " - house.BusyPiece.name = " + house.piece.name);

            string[] newRecord = { house.index.ToString(), house.piece.name.ToString() };
            table.AddRecord(newRecord);
        }

        SceneManager.LoadScene("SinglePlayer");
    }
}