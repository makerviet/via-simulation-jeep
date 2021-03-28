using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuOptions : MonoBehaviour
{
    private int track = 0;
    private Outline[] outlines;

    public void Start ()
    {
        outlines = GetComponentsInChildren<Outline>();
		Debug.Log ("in menu script "+outlines.Length);
    }

	public void ControlMenu()
	{
		SceneManager.LoadScene ("ControlMenu");
	}

	public void MainMenu()
	{
		Debug.Log ("go to main menu");
		SceneManager.LoadScene ("MenuScene");
	}

    public void StartDrivingMode()
    {
        SceneManager.LoadScene("DesignMapAutonomous");
    }

	public void OpenDesignMap()
	{
		SceneManager.LoadScene("Tiled");
	}

    public void StartAutonomousMode()
    {
        BrowseMapPopup.OpenPopup((map_name, isDefaultMap) =>
        {
            if (!string.IsNullOrEmpty(map_name))
            {
                Texture mapTexture = null;
                if (isDefaultMap)
                {
                    mapTexture = MapDataLoader.TextureOfDefaultMap(map_name);
                }
                MapDataLoader.SetInstanceMapData(MapDataLoader.DataOfMap(map_name, isDefaultMap), mapTexture);
                SceneManager.LoadScene("DesignMapAutonomous");
            }
        }, true);

        
    }

}
