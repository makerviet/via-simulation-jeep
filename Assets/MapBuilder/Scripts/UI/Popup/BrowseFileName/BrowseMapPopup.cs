using System;
using System.Collections;
using System.Collections.Generic;
using Koi.Common;
using UnityEngine;
using UnityEngine.UI;

public class BrowseMapPopup : MonoBehaviour
{
    private static BrowseMapPopup Instance;

    Action<string> OnSelectMapListener;

    [SerializeField] RectTransform root;
    [SerializeField] Button okButton;
    [SerializeField] Button cancelButton;
    [SerializeField] InputField inputName;

    [SerializeField] MapIcon mapIconPrefab;
    [SerializeField] Vector2 tileSize = new Vector2(230, 210);

    [SerializeField] List<RectTransform> listMapRows = new List<RectTransform>();
    [SerializeField] List<MapIcon> listMapIcons = new List<MapIcon>();

    private void Awake()
    {
        Instance = this;
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

    public static void OpenPopup(Action<string> callback)
    {
        if (Instance == null)
        {
            callback?.Invoke("");
        }
        else
        {
            Instance.DoOpenPopup(callback);
        }
    }


    void DoOpenPopup(Action<string> callback)
    {
        OnSelectMapListener = callback;
        root.gameObject.SetActive(true);
        InitListMapIcons();
    }

    void InitListMapIcons()
    {
        ClearMapIcons();
        int nMap = this.nMap;
        var allMaps = this.allMaps;
        float neoX = -tileSize.x * 2;
        float neoY = -tileSize.y * 0.5f;
        for (int i = 0; i < nMap; i++)
        {
            int row = i / 5;
            int col = i % 5;

            var freshIcon = Instantiate(mapIconPrefab, listMapRows[row]);
            UTransformUtils.ResetTransorm(freshIcon.transform);
            freshIcon.mRectTransform.anchoredPosition = new Vector2(neoX + col*tileSize.x, 0);
            freshIcon.AddSelectedListener(OnMapSelected);
            freshIcon.SetupIcon(allMaps[i]);
            listMapRows[row].anchoredPosition = new Vector2(0, 0 - tileSize.y * row);

            listMapIcons.Add(freshIcon);
        }
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
            OnSelectMapListener?.Invoke(mapName);
        }

        OnSelectMapListener = null;
        root.gameObject.SetActive(false);
    }

    void OnCancelClicked()
    {
        OnSelectMapListener?.Invoke("");

        OnSelectMapListener = null;
        root.gameObject.SetActive(false);
    }

    void OnMapSelected(string mapId)
    {
        string mapName = inputName.text;
        foreach (var map in allMaps)
        {
            if (string.Compare(map.map_create_id, mapId) == 0)
            {
                mapName = map.map_name;
            }
        }

        Debug.LogError("Selected map id = " + mapId + " map name = " + mapName);
        inputName.text = mapName;
    }


    int nMap => MapDataLoader.Instance.nMap;
    List<MapData> allMaps => MapDataLoader.Instance.allMaps;
}
