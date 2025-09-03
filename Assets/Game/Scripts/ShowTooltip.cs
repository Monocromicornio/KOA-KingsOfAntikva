using com.onlineobject.objectnet;
using UnityEngine;
using System.Collections;
public class ShowTooltip : MonoBehaviour
{
    [SerializeField] Transform tooltip; // arraste seu objeto Tooltip aqui

    void Awake()
    {
        tooltip = transform.Find("Tooltip");
        if (tooltip != null) tooltip.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (tooltip != null) tooltip.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (tooltip != null) tooltip.gameObject.SetActive(false);
    }
}
