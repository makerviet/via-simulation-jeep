using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapDataLoader : MonoBehaviour
{
    private const string MAP_DATAS_SAVE_KEY = "MAP_DATAS_SAVE_KEY";
    public static MapDataLoader Instance;

    [System.Serializable]
    public class MapAllData
    {
        public List<MapData> map_datas = new List<MapData>();
    }

    [System.Serializable]
    public class MapAsset
    {
        public TextAsset jsonData;
        public Texture texture;
        public MapData data;
    }

    [SerializeField] MapAllData mapAllDatas;
    [SerializeField] List<MapAsset> defaultMapAssets = new List<MapAsset>();

    public static List<MapAsset> fixedMapAssets => (Instance != null)? Instance.defaultMapAssets : new List<MapAsset>();
    //[Header("Debug")]
    //[SerializeField] List<MapData> defaultMapDatas = new List<MapData>();

    public Texture currentMapdataTexture;

    [Header("Gen Map texture")]
    [SerializeField] WorldMapBuilder tempWorldMapBuilder;
    [SerializeField] RenderTexture tempMapRenderTexture;

    [Header("Debug")]
    [SerializeField] MapData currentMapdata;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GameObject.DontDestroyOnLoad(this.gameObject);
            LoadData();
        }
        else
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
    }

    void LoadData()
    {
        string json = PlayerPrefs.GetString(MAP_DATAS_SAVE_KEY, "");
        mapAllDatas = JsonUtility.FromJson<MapAllData>(json);
        if (mapAllDatas == null)
        {
            mapAllDatas = new MapAllData();
        }

        foreach (MapAsset asset in defaultMapAssets)
        {
            string jsonData = asset.jsonData.text;
            asset.data = JsonUtility.FromJson<MapData>(jsonData);
        }

        countCheckMapTexture = 0;
        CheckMapTexture();
    }

    int countCheckMapTexture = 0;
    public bool isCheckingMapTexture = false;
    void CheckMapTexture()
    {
        if (isCheckingMapTexture)
        {
            return;
        }
        MapData noneTextureMapData = null;
        string texturePath = "";
        for (int i = countCheckMapTexture; i < mapAllDatas.map_datas.Count; i++)
        {
            var mapData = mapAllDatas.map_datas[i];
            if (!string.IsNullOrEmpty(mapData.map_name))
            {
                string path = PathOfMap(mapData.map_name);
                if (!string.IsNullOrEmpty(path))
                {
                    bool exist = false;
                    try
                    {
                        if (System.IO.File.Exists(path))
                        {
                            exist = true;
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    if (!exist)
                    {
                        countCheckMapTexture = i + 1;
                        texturePath = path;
                        noneTextureMapData = mapData;
                        break;
                    }
                }
            }
        }
        if (noneTextureMapData != null)
        {
            Debug.LogWarning("Gen texture for map[ " + (countCheckMapTexture - 1) + "], name = " + noneTextureMapData.map_name);
            GenMapTexture(noneTextureMapData, (texture) =>
            {
                Debug.LogWarning("Save Texture for map " + noneTextureMapData.map_name);
                try
                {
                    noneTextureMapData.texture = texture;
                    SaveTextureToJPG(texture, texturePath);
                }
                catch (System.Exception ex)
                {
                }

                CheckMapTexture();
            });
        }
        else
        {
            isCheckingMapTexture = false;
        }
    }

    void SaveData()
    {
        string json = JsonUtility.ToJson(mapAllDatas);
        PlayerPrefs.SetString(MAP_DATAS_SAVE_KEY, json);
        PlayerPrefs.Save();
    }


    public void OnServerDefaultMapLoaded(MapAllData pData)
    {
        for (int i = pData.map_datas.Count - 1; i >= 0; i--)
        {
            var mapData = pData.map_datas[i];
            if (!IsExistMap(mapData))
            {
                mapAllDatas.map_datas.Insert(0, mapData);
            }
        }
        ReCheckMapTexture();
    }

    public void OnUserMapLoaded(MapAllData pData)
    {
        foreach (var mapData in pData.map_datas)
        {
            if (!IsExistMap(mapData))
            {
                mapAllDatas.map_datas.Add(mapData);
            }
        }
        ReCheckMapTexture();
    }

    void ReCheckMapTexture()
    {
        countCheckMapTexture = 0;
        if (!isCheckingMapTexture)
        {
            CheckMapTexture();
        }
    }


    bool IsExistMap(MapData pMapData)
    {
        foreach (var map in defaultMaps)
        {
            if (map.data.map_create_id.CompareTo(pMapData.map_create_id) == 0)
            {
                return true;
            }
        }
        foreach (var map in mapAllDatas.map_datas)
        {
            if (map.map_create_id.CompareTo(pMapData.map_create_id) == 0)
            {
                return true;
            }
        }
        return false;
    }


    public static void SaveMap(MapData mapData, Texture2D mapTexture)
    {
        Instance.DoSaveMap(mapData, mapTexture);
        SetInstanceMapData(mapData, mapTexture);
        PlayfabDataLoader.PostUserMapData(Instance.mapAllDatas);
    }

    void DoSaveMap(MapData mapdata, Texture2D mapTexture)
    {
        var datas = mapAllDatas.map_datas;
        int id = -1;
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i].map_create_id == mapdata.map_create_id)
            {
                id = i;
                break;
            }
        }
        if (id < 0)
        {
            mapAllDatas.map_datas.Add(mapdata);
        }
        else
        {
            mapAllDatas.map_datas[id] = mapdata;
        }
        mapdata.texture = mapTexture;

        try
        {
            string dirPath = string.Format("{0}{1}", Application.persistentDataPath, "/map/");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            string texturePath = string.Format("{0}{1}.jpg", dirPath, mapdata.map_name);
            SaveTextureToJPG(mapTexture, texturePath);

            SaveData();
        }
        catch (Exception ex)
        {
        }
    }

    public static string PathOfMap(string mapName)
    {
        string texturePath = string.Format("{0}{1}{2}.jpg", Application.persistentDataPath, "/map/", mapName);
        return texturePath;
    }

    public static void SaveTextureToJPG(Texture2D texure, string fullPath)
    {
        try
        {
            byte[] bytes = texure.EncodeToJPG();
            File.WriteAllBytes(fullPath, bytes);
            Debug.LogWarning("Save success to " + fullPath);
        }
        catch (Exception ex)
        {
        }
    }


    public static void GenMapTexture(MapData mapData, System.Action<Texture2D> callback)
    {
        if (Instance != null)
        {
            Instance.DoGenMapTexture(mapData, callback);
        }
    }

    void DoGenMapTexture(MapData mapData, System.Action<Texture2D> callback)
    {
        StartCoroutine(DoIEGenMapTexture(mapData, callback));
    }

    IEnumerator DoIEGenMapTexture(MapData mapData, System.Action<Texture2D> callback)
    {
        tempWorldMapBuilder.gameObject.SetActive(true);
        yield return null;
        tempWorldMapBuilder.GenMap(mapData);
        yield return null;
        yield return null;
        var texture2D = ToTexture2D(tempMapRenderTexture);
        yield return null;
        tempWorldMapBuilder.gameObject.SetActive(false);
        mapData.texture = texture2D;
        callback?.Invoke(texture2D);
    }

    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    public static MapData instanceMapData => Instance.currentMapdata;

    public static void SetInstanceMapData(MapData pMapData, Texture pMapTexture)
    {
        Instance.currentMapdata = pMapData;
        if (pMapTexture != null)
        {
            Instance.currentMapdataTexture = pMapTexture;
        }
        else
        {
            Instance.currentMapdataTexture = pMapData.texture;
        }
    }

    public static Texture TextureOfDefaultMap(string mapId)
    {
        foreach (MapAsset mapAsset in Instance.defaultMapAssets)
        {
            if (string.Compare(mapAsset.data.map_create_id, mapId) == 0)
            {
                return mapAsset.texture;
            }
        }
        return null;
    }

    public static MapData DataOfMap(string mapId, bool isDefaultMap)
    {
        if (isDefaultMap)
        {
            foreach (MapAsset mapAsset in Instance.defaultMapAssets)
            {
                if (string.Compare(mapAsset.data.map_create_id, mapId) == 0)
                {
                    return JsonUtility.FromJson<MapData>(JsonUtility.ToJson(mapAsset.data));
                }
            }
        }

        foreach (MapData mapData in Instance.allMaps)
        {
            if (string.Compare(mapData.map_create_id, mapId) == 0)
            {
                return JsonUtility.FromJson<MapData>(JsonUtility.ToJson(mapData));
                //return mapData;
            }
        }
        return null;
    }

    public static string JsonOfCurrentMap()
    {
        return JsonUtility.ToJson(Instance.currentMapdata);
    }


    [ContextMenu("Log All map datas")]
    void LogAllMapData()
    {
        Debug.LogError("" + JsonUtility.ToJson(mapAllDatas));
    }


    public List<MapData> allMaps => mapAllDatas.map_datas;
    public List<MapAsset> defaultMaps => defaultMapAssets;
    public int nMap => mapAllDatas.map_datas.Count;
}
