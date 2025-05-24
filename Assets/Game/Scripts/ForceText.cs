using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class ForceText : MonoBehaviour
{
    [SerializeField]
    private InteractivePiece piece;
    private string txtForce;

    public string force
    {
        set
        {
            txtForce = string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);
            textMesh ??= GetComponent<TextMesh>();
            UpdateText();
        }
    }
    private TextMesh textMesh;

    private void Awake()
    {
        textMesh ??= GetComponent<TextMesh>();
        UpdateText();
    }

    private void UpdateText()
    {
        txtForce = piece?.force.ToString();
        bool txtEmpty = txtForce == "B" || txtForce != "F";
        textMesh.text = txtEmpty ? "" : txtForce;
    }
}
