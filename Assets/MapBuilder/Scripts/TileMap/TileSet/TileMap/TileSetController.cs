using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSetController : MonoBehaviour
{
    Action<int, int, Image> OnTileSelectedListener;

    [SerializeField] TileMapInput controlInput;

    [SerializeField] Button rotateButton;
    [SerializeField] Button delButton;

    [SerializeField] RectTransform selectBorder;
    [SerializeField] List<TileSetCell> tileSetCells = new List<TileSetCell>();
    [SerializeField] int selectingCellId = 0;

    [SerializeField] int curRot = 0;

    [SerializeField] bool onDelMode = false;

    void Start()
    {
        InitListener();
    }


    void InitListener()
    {
        foreach (TileSetCell cell in tileSetCells)
        {
            cell.AddSelectedListener(OnTileSetSelected);
        }

        rotateButton.onClick.AddListener(OnRotateClicked);
        delButton.onClick.AddListener(OnDelClicked);

        controlInput.AddRightMouseClickListener(OnRotateClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnRotateClicked();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnDelClicked();
        }
    }

    void OnRotateClicked()
    {
        onDelMode = false;
        curRot = (curRot + 90) % 360;

        CallbackTileSelected();
    }

    void OnDelClicked()
    {
        onDelMode = !onDelMode;
        if (onDelMode)
        {
            OnTileSelectedListener?.Invoke(-1, 0, null);
        }
        else
        {
            CallbackTileSelected();
        }
    }

    public void AddTileSelectedListener(Action<int, int, Image> pListener)
    {
        OnTileSelectedListener -= pListener;
        OnTileSelectedListener += pListener;
    }

    void CallbackTileSelected()
    {
        OnTileSelectedListener?.Invoke(selectingCellId, curRot, tileSetCells[selectingCellId].iconImage);
    }

    void OnTileSetSelected(int cellId)
    {
        Debug.LogError("OnTileSet Selected " + cellId);
        onDelMode = false;
        selectingCellId = cellId;
        curRot = 0;

        // update visible here
        for (int i = 0; i < tileSetCells.Count; i++)
        {
            tileSetCells[i].SetSelectState(i == cellId);
        }

        // update select border position
        selectBorder.anchoredPosition = tileSetCells[cellId].rectTransform.anchoredPosition;
        var borderSize = tileSetCells[cellId].iconSize;
        selectBorder.sizeDelta = new Vector2(borderSize.x + 18, borderSize.y + 18);

        CallbackTileSelected();
    }
}
