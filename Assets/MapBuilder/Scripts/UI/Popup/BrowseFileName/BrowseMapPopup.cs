using System;
using System.Collections;
using System.Collections.Generic;
using Koi.Common;
using UnityEngine;
using UnityEngine.UI;
using static MapDataLoader;

public class BrowseMapPopup : MonoBehaviour
{
    private static BrowseMapPopup Instance;

    Action<string, bool> OnSelectMapListener;

    [SerializeField] RectTransform root;
    [SerializeField] Button okButton;
    [SerializeField] Button cancelButton;
    [SerializeField] InputField inputName;

    [SerializeField] MapIcon mapIconPrefab;
    [SerializeField] Vector2 tileSize = new Vector2(230, 210);

    [SerializeField] List<RectTransform> listMapRows = new List<RectTransform>();
    [SerializeField] List<MapIcon> listMapIcons = new List<MapIcon>();

    [SerializeField] RectTransform content;

    bool onDefaultMapSelected = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
        else
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        okButton.onClick.AddListener(OnOkClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public static void OpenPopup(Action<string, bool> callback, bool isLoad)
    {
        if (Instance == null)
        {
            callback?.Invoke("", false);
        }
        else
        {
            Instance.DoOpenPopup(callback, isLoad);
        }
    }


    void DoOpenPopup(Action<string, bool> callback, bool isLoad)
    {
        OnSelectMapListener = callback;
        root.gameObject.SetActive(true);
        InitListMapIcons(isLoad);
    }

    void InitListMapIcons(bool isAllowDefaultMap)
    {
        ClearMapIcons();
        int nMap = this.nMap;
        var allMaps = this.allMaps;
        float neoX = -tileSize.x * 2;
        float neoY = -tileSize.y * 0.5f;

        int nDefaultMap = isAllowDefaultMap ? this.defaultMaps.Count : 0;
        for (int i = 0; i < nDefaultMap; i++)
        {
            int row = i / 5;
            int col = i % 5;

            var freshIcon = Instantiate(mapIconPrefab, listMapRows[row]);
            UTransformUtils.ResetTransorm(freshIcon.transform);
            freshIcon.mRectTransform.anchoredPosition = new Vector2(neoX + col * tileSize.x, 0);
            freshIcon.AddSelectedListener(OnMapSelected);
            freshIcon.SetupDefaultMapData(defaultMaps[i]);
            listMapRows[row].anchoredPosition = new Vector2(0, 0 - tileSize.y * row);

            listMapIcons.Add(freshIcon);
        }

        if (nDefaultMap > 0)
        {
            nDefaultMap = ((nDefaultMap - 1) / 5 + 1) * 5;
        }
        
        for (int i = 0; i < nMap; i++)
        {
            int row = (i+ nDefaultMap) / 5;
            int col = (i+ nDefaultMap) % 5;

            var freshIcon = Instantiate(mapIconPrefab, listMapRows[row]);
            UTransformUtils.ResetTransorm(freshIcon.transform);
            freshIcon.mRectTransform.anchoredPosition = new Vector2(neoX + col*tileSize.x, 0);
            freshIcon.AddSelectedListener(OnMapSelected);
            freshIcon.SetupIcon(allMaps[i]);
            listMapRows[row].anchoredPosition = new Vector2(0, 0 - tileSize.y * row);

            listMapIcons.Add(freshIcon);
        }

        int nRow = (nDefaultMap + nMap - 1) / 5 + 1;

        var contentSize = content.sizeDelta;
        contentSize.y = nRow * tileSize.y;
        content.sizeDelta = contentSize;
    }

    void ClearMapIcons()
    {
        for (int i = listMapIcons.Count - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(listMapIcons[i].gameObject);
        }
        listMapIcons.Clear();
    }

    void OnOkClicked()
    {
        string mapName = inputName.text;
        if (!string.IsNullOrEmpty(mapName))
        {
            OnSelectMapListener?.Invoke(mapName, onDefaultMapSelected);
        }

        OnSelectMapListener = null;
        root.gameObject.SetActive(false);
    }

    void OnCancelClicked()
    {
        OnSelectMapListener?.Invoke("", false);

        OnSelectMapListener = null;
        root.gameObject.SetActive(false);
    }

    void OnMapSelected(string mapId, bool isDefaultMap)
    {
        Debug.LogWarning("On Map Selected " + mapId + " is default " + isDefaultMap);
        this.onDefaultMapSelected = isDefaultMap;
        string mapName = inputName.text;
        foreach (var map in allMaps)
        {
            if (string.Compare(map.map_create_id, mapId) == 0)
            {
                mapName = map.map_name;
            }
        }
        foreach (var map in defaultMaps)
        {
            if (string.Compare(map.data.map_create_id, mapId) == 0)
            {
                mapName = map.data.map_name;
            }
        }
        Debug.LogWarning("Selected map id = " + mapId + " map name = " + mapName);
        inputName.text = mapName;
    }

    int nMap => MapDataLoader.Instance.nMap;
    List<MapData> allMaps => MapDataLoader.Instance.allMaps;
    List<MapAsset> defaultMaps => MapDataLoader.Instance.defaultMaps;
}
