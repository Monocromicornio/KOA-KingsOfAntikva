using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MinimapSetup : MonoBehaviour
{
    [Header("Setup Helper")]
    [SerializeField] private bool setupClearButtons = false;

    private void Update()
    {
        if (setupClearButtons)
        {
            setupClearButtons = false;
            SetupClearButtons();
        }
    }

    private void SetupClearButtons()
    {
        MinimapMarkingSystem[] markingSystems = GetComponentsInChildren<MinimapMarkingSystem>(true);
        
        foreach (MinimapMarkingSystem system in markingSystems)
        {
            Button[] buttons = system.GetComponentsInChildren<Button>(true);
            
            foreach (Button button in buttons)
            {
                TMPro.TextMeshProUGUI buttonText = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                
                if (buttonText != null && buttonText.text.ToLower().Contains("limpar"))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => system.ClearAllMarkings());
                    Debug.Log($"Botão 'Limpar' configurado em {button.name}");
                }
            }
        }
        
        Debug.Log("Setup de botões de limpeza concluído!");
    }
}
