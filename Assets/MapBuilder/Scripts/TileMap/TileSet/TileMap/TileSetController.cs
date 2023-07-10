using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSetController : MonoBehaviour
{

    // TileId, rot, image
    Action<int, int, Image> OnTileSelectedListener;

    [SerializeField] TileMapInput controlInput;

    [SerializeField] Button rotateButton;
    [SerializeField] Button delButton;
    [SerializeField] bool useDelMode = true;

    [SerializeField] RectTransform selectBorder;

    [Header("Debug")]
    [SerializeField] List<TileSetCell> tileSetCells = new List<TileSetCell>();
    [SerializeField] TileSetCell selectingCell;
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
        if (useDelMode)
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
        else
        {
            OnTileSelectedListener?.Invoke(-1, 0, null);
        }
    }

    public void AddTileSelectedListener(Action<int, int, Image> pListener)
    {
        OnTileSelectedListener -= pListener;
        OnTileSelectedListener += pListener;
    }

    void CallbackTileSelected()
    {
        OnTileSelectedListener?.Invoke(selectingCellId, curRot, selectingCell? selectingCell.iconImage : null);
    }

    void OnTileSetSelected(TileSetCell cell)
    {
        selectingCell = cell;
        int tileId = cell.TileId;
        Debug.LogWarning("OnTileSet Selected " + tileId);
        onDelMode = false;
        selectingCellId = tileId;
        curRot = 0;

        // update visible here
        for (int i = 0; i < tileSetCells.Count; i++)
        {
            tileSetCells[i].SetSelectState(tileSetCells[i] == cell);
        }

        // update select border position
        selectBorder.anchoredPosition = cell.rectTransform.anchoredPosition;
        var borderSize = cell.iconSize;
        selectBorder.sizeDelta = new Vector2(borderSize.x + 18, borderSize.y + 18);

        CallbackTileSelected();
    }
}
