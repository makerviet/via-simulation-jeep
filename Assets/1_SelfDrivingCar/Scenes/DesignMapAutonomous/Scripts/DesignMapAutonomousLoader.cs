using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DesignMapAutonomousLoader : MonoBehaviour
{
    private static DesignMapAutonomousLoader Instance;

    private void Start()
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


    public static void LoadDesignMapAutonomous(string json)
    {
        MapData data = JsonUtility.FromJson<MapData>(json);
        MapDataLoader.GenMapTexture(data, (texture) =>
        {
            //string texturePath = MapDataLoader.PathOfMap(data.map_name);
            //Debug.LogWarning("Save Texture for map " + data.map_name);
            //MapDataLoader.SaveTextureToJPG(texture, texturePath);

            MapDataLoader.SetInstanceMapData(data, texture);
            SceneManager.LoadScene("DesignMapAutonomous");
        });
        
    }
}
