using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabMenuManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string tabName;
        public GameObject tabContent;
        public Button tabButton;
    }

    [Header("Abas do Menu")]
    public List<Tab> tabs = new List<Tab>();

    private int currentIndex = 0;

    private void Start()
    {
        // Conecta os botões
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[i].tabButton.onClick.AddListener(() => OpenTab(index));
        }

        // Abre a primeira aba por padrão
        OpenTab(0);
    }

    public void OpenTab(int index)
    {
        currentIndex = index;

        for (int i = 0; i < tabs.Count; i++)
        {
            bool active = (i == index);
            tabs[i].tabContent.SetActive(active);

            // Destaque visual no botão (opcional)
            ColorBlock colors = tabs[i].tabButton.colors;
            colors.normalColor = active ? new Color(0.8f, 0.8f, 1f) : Color.white;
            tabs[i].tabButton.colors = colors;
        }
    }
}
