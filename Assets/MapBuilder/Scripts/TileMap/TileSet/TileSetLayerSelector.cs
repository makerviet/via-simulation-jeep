using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSetLayerSelector : MonoBehaviour
{
    Action<int> OnLayerSelectedListener;

    [SerializeField] int selectingId = 0;
    [SerializeField] List<Button> layerBtns;
    [SerializeField] Transform selectBorder;

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        for (int i = 0; i < layerBtns.Count; i++)
        {
            int id = i;
            layerBtns[i].onClick.AddListener(() =>
            {
                OnLayerSelected(id);
            });
        }
    }

    public void AddLayerSelectedListener(Action<int> pListener)
    {
        OnLayerSelectedListener -= pListener;
        OnLayerSelectedListener += pListener;
    }

    void OnLayerSelected(int id)
    {
        selectingId = id;
        selectBorder.localPosition = layerBtns[id].transform.localPosition;
        OnLayerSelectedListener?.Invoke(id);
    }
}
