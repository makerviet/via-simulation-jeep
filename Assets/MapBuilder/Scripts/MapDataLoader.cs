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

    public List<MapData> allMaps => mapAllDatas.map_datas;
    public int nMap => mapAllDatas.map_datas.Count;

    [SerializeField] MapAllData mapAllDatas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
    }

    private void Start()
    {
        LoadData();
    }

    void LoadData()
    {
        string json = PlayerPrefs.GetString(MAP_DATAS_SAVE_KEY, "");
        mapAllDatas = JsonUtility.FromJson<MapAllData>(json);
        if (mapAllDatas == null)
        {
            mapAllDatas = new MapAllData();
        }
    }

    void SaveData()
    {
        string json = JsonUtility.ToJson(mapAllDatas);
        PlayerPrefs.SetString(MAP_DATAS_SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static void SaveMap(MapData mapData, Texture2D mapTexture)
    {
        Instance.DoSaveMap(mapData, mapTexture);
        SetInstanceMapData(mapData);
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

        string dirPath = string.Format("{0}{1}", Application.persistentDataPath, "/map/");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        string texturePath = string.Format("{0}{1}.jpg", dirPath, mapdata.map_name);
        SaveTextureToJPG(mapTexture, texturePath);

        SaveData();
    }

    public static string PathOfMap(string mapName)
    {
        string texturePath = string.Format("{0}{1}{2}.jpg", Application.persistentDataPath, "/map/", mapName);
        return texturePath;
    }

    static void SaveTextureToJPG(Texture2D texure, string fullPath)
    {
        byte[] bytes = texure.EncodeToJPG();
        File.WriteAllBytes(fullPath, bytes);
        Debug.LogError("Save success to " + fullPath);
    }

    public MapData currentMapdata;
    public static MapData instanceMapData => Instance.currentMapdata;

    public static void SetInstanceMapData(MapData pMapData)
    {
        Instance.currentMapdata = pMapData;
    }

    public static MapData DataOfMap(string mapId)
    {
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
}
