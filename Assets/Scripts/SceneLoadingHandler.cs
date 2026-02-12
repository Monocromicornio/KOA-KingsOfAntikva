using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingHandler : MonoBehaviour
{
    private static SceneLoadingHandler instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void LoadSceneWithLoading(string sceneName, string loadingMessage = null)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.LoadSceneAsync(sceneName, loadingMessage));
        }
        else
        {
            Debug.LogWarning("[SceneLoadingHandler] Instance não encontrada, carregando cena diretamente");
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName, string loadingMessage)
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Show(loadingMessage ?? "Carregando cena...");
            LoadingScreenManager.Instance.UpdateProgress(0.1f);
        }

        yield return new WaitForSeconds(0.2f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.UpdateProgress(asyncLoad.progress);
            }

            yield return null;
        }

        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.UpdateProgress(0.9f);
        }

        yield return new WaitForSeconds(0.3f);

        asyncLoad.allowSceneActivation = true;

        yield return new WaitUntil(() => asyncLoad.isDone);

        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.UpdateProgress(1f);
        }
    }

    public static void ShowLoadingScreen(string message = null)
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Show(message);
        }
    }

    public static void HideLoadingScreen()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Hide();
        }
    }

    public static void UpdateLoadingProgress(float progress)
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.UpdateProgress(progress);
        }
    }

    public static void SetLoadingStatus(string status)
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.SetStatusText(status);
        }
    }
}
