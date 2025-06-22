using UnityEngine;

public class GameMode : MonoBehaviour
{
    public enum GameType
    {
        Training = 1, // Show pieces any time
        Normal = 2,   // Show pieces when dying
        Hard = 3      // No show pieces
    }

    [SerializeField]
    private GameType gameType = GameType.Training;

    public GameType type
    {
        get
        {
            if (gameType == 0)
            {
                gameType = (GameType)PlayerPrefs.GetInt("GameMode", (int)GameType.Training);
            }
            return gameType;
        }
        set
        {
            gameType = value;
            PlayerPrefs.SetInt("GameMode", (int)gameType);
        }
    }
}
