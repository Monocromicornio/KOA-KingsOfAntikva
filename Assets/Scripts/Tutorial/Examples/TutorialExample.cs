using UnityEngine;

public class TutorialExample : MonoBehaviour
{
    [Header("Tutorial Example")]
    [Tooltip("Este é um exemplo de como criar eventos customizados no tutorial")]
    public GameObject highlightUI;
    public AudioClip successSound;

    public void OnStepStart_ShowHighlight()
    {
        if (highlightUI != null)
        {
            highlightUI.SetActive(true);
        }
        Debug.Log("Tutorial Step Started!");
    }

    public void OnStepComplete_HideHighlight()
    {
        if (highlightUI != null)
        {
            highlightUI.SetActive(false);
        }
        Debug.Log("Tutorial Step Completed!");
    }

    public void PlaySuccessSound()
    {
        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, Camera.main.transform.position);
        }
    }

    public void ShowMessage(string message)
    {
        Debug.Log($"Tutorial Message: {message}");
    }
}
