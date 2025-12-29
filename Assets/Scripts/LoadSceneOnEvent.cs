using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnEvent : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}