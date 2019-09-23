using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public enum CellTypes
    {
        NONE = 0,
        WATERMELON,
        CHERRY,
        CUCUMBER,
        LEMON,
        LEMON2,
        LIME,
        APPLE,
        STRAWBERRY,
        BANANA,
        BOKCHOY,
        GRAPES,
        DRAGONFRUIT,
        KIWI,
        GREENPEPPER,
        REDPEPPER,
        REDONION,
        TURNIP,
        TOMATOES,
        CARROT,
        POTATOES,
        PUMPKIN,
        PINEAPPLE,
        ORANGE,
        CABBAGE,
        CORN,
        MANDARIN,
        HORN,
        GARLIC
    }

    CellTypes _cellType = CellTypes.NONE;
    int[] _positionInGrid = new int[2];

    Text _imageText;

    void Awake()
    {
        _imageText = transform.Find("Text").GetComponent<Text>();
    }

    public CellTypes GetCellType()
    {
        return _cellType;
    }

    public void SetCellType(CellTypes cellType)
    {
        _cellType = cellType;

        if (cellType != CellTypes.NONE)
        {
            transform.GetComponent<Image>().enabled = true;

            //USING FRUIT NAMES
            //imageText.text = System.Enum.GetName(typeof(CellTypes), _cellType);

            //USING NUMBERS
            _imageText.text = ((int)_cellType).ToString();
        }
        else
        {
            ClearType();
        }
    }

    public bool IsClear()
    {
        return _cellType == CellTypes.NONE;
    }

    public void ClearType()
    {
        _cellType = CellTypes.NONE;
        transform.GetComponent<Image>().enabled = false;

        _imageText.text = "";
    }

    public void RandomizeType()
    {
        SetCellType((CellTypes)Random.Range(1, System.Enum.GetValues(typeof(CellTypes)).Length));
    }

    public void SetGridPosition(int x, int y)
    {
        _positionInGrid[0] = x;//col
        _positionInGrid[1] = y;//row
    }

    public int[] GetGridPosition()
    {
        return _positionInGrid;
    }

    public void Select()
    {
        if (!IsClear())
            GameCells.instance.SelectCell(this);
    }

    public void DeSelect()
    {
        transform.GetComponent<Image>().color = Color.white;
    }

    //Highlight the cell for a giving time. If time = 0 it will be highlighted until it is deselected.
    public void Highlight(float time)
    {
        if (time > 0f)
            StartCoroutine(HighlightProcess(time));
        else
            transform.GetComponent<Image>().color = Color.red;
    }

    IEnumerator HighlightProcess(float time)
    {
        Image bgImage = transform.GetComponent<Image>();

        bgImage.enabled = true;
        bgImage.color = Color.red;
        yield return new WaitForSeconds(time);
        bgImage.color = Color.white;

        if (IsClear())
            bgImage.enabled = false;
    }
}
