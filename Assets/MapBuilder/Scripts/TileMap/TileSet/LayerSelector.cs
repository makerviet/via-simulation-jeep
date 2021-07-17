using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerSelector : MonoBehaviour
{
    public enum MapLayer
    {
        TileMap = 0,
        TrafficSign = 1
    }

    [System.Serializable]
    public class MapLayerPointer
    {
        public MapLayer layer;
        public RectTransform rectTransform;
        public Button button;
        public TileSetController tileSet;
        public GameObject tileSetRoot;
    }

    Action<MapLayer> OnMapLayerSelectedListener;

    [SerializeField] List<MapLayerPointer> mapLayerPointers = new List<MapLayerPointer>();
    [SerializeField] RectTransform selector;
    
    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        foreach (var mapLayer in mapLayerPointers)
        {
            mapLayer.button.onClick.AddListener(() =>
            {
                OnLayerSelected(mapLayer);
            });
        }
    }


    public void AddMapLayerSelectedListener(Action<MapLayer> pListener)
    {
        OnMapLayerSelectedListener -= pListener;
        OnMapLayerSelectedListener += pListener;
    }

    void OnLayerSelected(MapLayerPointer pointer)
    {
        selector.position = pointer.rectTransform.position;
        OnMapLayerSelectedListener?.Invoke(pointer.layer);

        foreach (var mapLayer in mapLayerPointers)
        {
            if (mapLayer == pointer)
            {
                mapLayer.tileSetRoot.SetActive(true);
            }
            else
            {
                mapLayer.tileSetRoot.SetActive(false);
            }
        }
    }
}
