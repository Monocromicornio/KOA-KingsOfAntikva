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
    private BoardController board => BoardController.instance;
    private EditableField[] editableFields => board.editableFields;


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
                gameMode.type = toggleGame.gameType;
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
        foreach (EditableField editable in editableFields)
        {
            Debug.Log("SavePieceOrder - house.Index = " + editable.index + " - house.BusyPiece.name = " + editable.piece.name);

            string[] newRecord = { editable.index.ToString(), editable.piece.name.ToString() };
            table.AddRecord(newRecord);
        }

        SceneManager.LoadScene("SinglePlayer");
    }
}