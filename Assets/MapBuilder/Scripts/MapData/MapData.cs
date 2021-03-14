using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    private const string INSTANT_MAP_SAVE_KEY = "INSTANT_MAP_SAVE_KEY";

    [System.Serializable]
    public class MapCellData
    {
        public int id_inmap;
        public int tile_id;
        public int rot;
    }

    [System.Serializable]
    public class MapLayerData
    {
        public int layer_id;
        public List<MapCellData> cell_datas = new List<MapCellData>();
    }

    public string map_name;
    public string map_create_id => map_name;

    public int bg_id;
    public Vector2Int map_size = new Vector2Int(10, 10);
    //public List<MapLayerData> map_layers = new List<MapLayerData>();
    public List<MapCellData> cell_datas = new List<MapCellData>();
    public Vector2 anchor_offset = Vector2.zero;

    public void CleanNullCell()
    {
        var oldList = cell_datas;
        cell_datas = new List<MapCellData>();
        for (int i = 0; i < oldList.Count; i++)
        {
            var freshCell = new MapCellData();
            freshCell.id_inmap = oldList[i].id_inmap;
            freshCell.rot = oldList[i].rot;
            freshCell.tile_id = oldList[i].tile_id;
            cell_datas.Add(freshCell);
        }
        for (int i = cell_datas.Count - 1; i >= 0; i--)
        {
            if (cell_datas[i].tile_id < 0)
            {
                cell_datas.RemoveAt(i);
            }
        }
    }


    public static void SaveInstantMapJson(string json)
    {
        PlayerPrefs.SetString(INSTANT_MAP_SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static string GetInstantMapJson()
    {
        return PlayerPrefs.GetString(INSTANT_MAP_SAVE_KEY);
    }
}
