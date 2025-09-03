using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTooltipController : MonoBehaviour
{
    [Header("Masks")]
    [SerializeField] LayerMask hoverMask; // SOMENTE a layer Hover
    [SerializeField] LayerMask clickMask; // Suas layers clicáveis (excluir Hover)

    [Header("Ray")]
    [SerializeField] float maxDistance = 1000f;

    ShowTooltip current;

    void Update()
    {
        // Evita hover quando o ponteiro está sobre UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHover();
            return;
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, maxDistance, hoverMask, QueryTriggerInteraction.Collide))
        {
            var tt = hit.collider.GetComponentInParent<ShowTooltip>();
            if (tt != current)
            {
                ClearHover();
                current = tt;
                current?.Show();
            }
        }
        else
        {
            ClearHover();
        }

        // Exemplo de clique que NÃO considera a layer Hover
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out var clickHit, maxDistance, clickMask, QueryTriggerInteraction.Ignore))
            {
                // trate seu clique aqui (terreno, etc.)
                // Debug.Log("Clique em: " + clickHit.collider.name);
            }
        }
    }

    void ClearHover()
    {
        if (current != null) { current.Hide(); current = null; }
    }
}
