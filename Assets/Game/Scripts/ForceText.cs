using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class ForceText : MonoBehaviour
{
    private TextMesh textMesh;
    
    [SerializeField]
    private InteractivePiece piece;
    private string txtForce;

    public string force
    {
        set
        {
            txtForce = string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);
            if (txtForce == "B" || txtForce != "F")
            {
                txtForce = "";
            }

            if (textMesh == null) return;
            textMesh.text = txtForce;
        }
    }

    private void Awake()
    {
        textMesh ??= GetComponent<TextMesh>();
        if(piece != null) force = piece.force.ToString();
    }
}
