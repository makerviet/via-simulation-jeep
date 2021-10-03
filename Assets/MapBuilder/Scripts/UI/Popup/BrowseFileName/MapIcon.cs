using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MapDataLoader;

public class MapIcon : MonoBehaviour
{
    Action<string, bool> OnMapIconSelectedListener;

    [SerializeField] Button mButton;
    [SerializeField] RawImage rawImage;
    [SerializeField] Text label;

    [SerializeField] string mapId;

    [SerializeField] MapData mapData;
    [SerializeField] bool isDefaultMap = false;

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        mButton.onClick.AddListener(OnButtonClicked);
    }

    public void AddSelectedListener(Action<string, bool> pListener)
    {
        OnMapIconSelectedListener -= pListener;
        OnMapIconSelectedListener += pListener;
    }

    public void SetupDefaultMapData(MapAsset pMapAsset)
    {
        this.isDefaultMap = true;
        this.mapData = pMapAsset.data;
        label.text = mapData.map_name;
        this.name = string.Format("Map_{0}", mapData.map_name);
        rawImage.texture = pMapAsset.texture;
    }

    public void SetupIcon(MapData pMapData)
    {
        this.mapData = pMapData;
        label.text = mapData.map_name;
        StartCoroutine(LoadTexture());
        this.name = string.Format("Map_{0}", mapData.map_name);
    }

    IEnumerator LoadTexture()
    {
        yield return null;
        string path = string.Format("file://{0}", MapDataLoader.PathOfMap(mapData.map_name));
        WWW www = new WWW(path);
        yield return www;
        rawImage.texture = www.texture;
        Debug.LogWarning("Loaded Icon of map " + path);
    }

    void OnButtonClicked()
    {
        OnMapIconSelectedListener?.Invoke(mapData.map_create_id, isDefaultMap);
    }


    public RectTransform mRectTransform => rawImage.rectTransform;
}
