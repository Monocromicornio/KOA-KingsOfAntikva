using TMPro;
using UnityEngine;

[ExecuteAlways]
public class CurvedTMPText : MonoBehaviour
{
    [SerializeField] private float curvature = 0.002f;

    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        CurveText();
    }

    private void Update()
    {
        CurveText();
    }

    private void CurveText()
    {
        if (text == null)
            return;

        text.ForceMeshUpdate();

        TMP_TextInfo textInfo = text.textInfo;

        if (textInfo.characterCount == 0)
            return;

        float minX = text.bounds.min.x;
        float maxX = text.bounds.max.x;
        float centerX = (minX + maxX) / 2f;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            float charCenterX =
                (vertices[vertexIndex].x +
                 vertices[vertexIndex + 2].x) / 2f;

            float distanceFromCenter =
                charCenterX - centerX;

            float yOffset =
                curvature *
                distanceFromCenter *
                distanceFromCenter;

            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j].y += yOffset;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices =
                textInfo.meshInfo[i].vertices;

            text.UpdateGeometry(
                textInfo.meshInfo[i].mesh,
                i
            );
        }
    }
}