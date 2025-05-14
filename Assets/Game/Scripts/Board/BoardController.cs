using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController instance;

    [SerializeField] private BoardData boardData;

    [SerializeField] private Field field;

    public List<Field> fields { get; private set; }

    void Awake()
    {
        instance = this;
        CreateFields(boardData.column, boardData.row, boardData.distance);
    }

    /// <summary>
    /// Creates the board fields.
    /// </summary>
    /// <param name="col">Number of columns.</param>
    /// <param name="row">Number of rows.</param>
    /// <param name="distance">Distance between fields.</param>
    /// <param name="isSelectOrder">If true, configures fields for piece ordering.</param>
    void CreateFields(int col, int row, float distance)
    {
        fields = new List<Field>();

        int iCount = 0;
        int iCountFields;
        int iCountRows = 0;

        float fPosX;
        float fPosXInit = transform.position.x;
        float fPosZ = transform.position.z;

        for (int z = 0; z < col; z++)
        {
            fPosX = fPosXInit;
            iCountFields = 0;
            for (int x = 0; x < row; x++)
            {
                Vector3 vPos = new Vector3(fPosX, field.transform.position.y, fPosZ);
                Field fieldClone = Instantiate(field, vPos, transform.rotation);
                fields.Add(fieldClone);

                int iFieldCount = iCount + 1;

                fieldClone.name = "Field" + iFieldCount.ToString();
                fieldClone.Configure(iCount, iCountFields, iCountRows);

                iCountFields++;
                iCount++;

                fPosX += distance;
            }

            iCountRows++;
            fPosZ += distance;
        }

        boardData.isFinished = true;
    }

    /// <summary>
    /// Método para criar os campos do tabuleiro para ordenar peças.
    /// </summary>
    /// <param name="col">Número de colunas.</param>
    /// <param name="row">Número de linhas.</param>
    public void CreateFieldsSelectOrder(int col, int row)
    {
        CreateFields(col, row, boardData.distance);
    }

    public int GetFields()
    {
        return boardData.column;
    }

    public bool isFinished(){
        return boardData.isFinished;
    }

    public float GetDistance(){
        return boardData.distance;
    }

    public HousePicker[] GetHousePickersFromFields(){
        List<HousePicker> pickers = new List<HousePicker>();

        foreach(Field field in fields){
            if(field.isHousePicker){
                pickers.Add(field.GetFieldType() as HousePicker);
            }
        }

        return pickers.ToArray();
    }
}
