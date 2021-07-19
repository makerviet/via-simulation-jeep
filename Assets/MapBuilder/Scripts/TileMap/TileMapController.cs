using System.Collections;
using System.Collections.Generic;
using Koi.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static LayerSelector;
using static MapData;

public class TileMapController : MonoBehaviour
{
    public enum DrawState
    {
        Moving = 0,
        Dragging = 1
    }

    [System.Serializable]
    public class CellResource
    {
        public MapCellType cellType;
        public Image cellImagePrefab;
    }

    [Header("MapRender")]
    [SerializeField] RenderTexture mapRenderTexture;

    [Header("Layer")]
    [SerializeField] MapLayer currentLayer = MapLayer.TileMap;
    [SerializeField] LayerSelector layerSelector;

    [Header("Map Setup")]
    [SerializeField] Canvas canvas;
    [SerializeField] TileMapInput mapInput;


    
    [SerializeField] TileSetController tileSetController;

    [SerializeField] MapObjectLayerGenerator trafficSignMapGenerator;


    [SerializeField] Vector2 cellSize = Vector2.one * 100.0f;
    [SerializeField] Vector2 neoCell = Vector2.one * 50.0f;
    [SerializeField] RectTransform root;
    [SerializeField] List<CellResource> cellResources;

    [Header("Bg")]
    [SerializeField] TileSetBgSelector bgSelector;
    [SerializeField] Image bg;
    [SerializeField] List<Sprite> bgSprites;

    [Header("control")]
    [SerializeField] Button clear;
    [SerializeField] Button genMapButton;
    [SerializeField] Button goButton;

    [SerializeField] Button carSelectButton;
    [SerializeField] Button loadMapButton;
    [SerializeField] Button saveAsButton;
    [SerializeField] Button saveMapButton;


    [SerializeField] WorldMapBuilder worldMapBuilder;

    [Header("Debug")]
    [SerializeField] string currentMapName = "";
    [SerializeField] int bgId = 0;
    [SerializeField] Vector2Int mapSize = new Vector2Int(12, 10);
    [SerializeField] DrawState drawState = DrawState.Moving;
    [SerializeField] List<MapCellData> listCells = new List<MapCellData>();
    [SerializeField] List<Image> listCellIcons = new List<Image>();

    [SerializeField] Image pointerImage;
    [SerializeField] Image carImage;
    bool onCarDraw = false;
    [SerializeField] int pointerTileId;
    [SerializeField] int tileRot;
    [SerializeField] Vector2 carPos;

    void Start()
    {
        InitListener();
        InitMapData();
    }

    void InitMapData()
    {
        listCells = new List<MapCellData>();
        ClearCellIcons();
        listCellIcons = new List<Image>();
        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                MapCellData freshCell = new MapCellData();
                freshCell.id_inmap = i * mapSize.x + j;
                freshCell.tile_id = -1;
                listCells.Add(freshCell);
                listCellIcons.Add(null);
            }
        }
    }

    void InitListener()
    {
        mapInput.AddBeginDragListener(OnBeginDragging);
        mapInput.AddDraggingListener(OnMouseDragging);
        mapInput.AddFinishDragListener(OnFinishDragging);

        layerSelector.AddMapLayerSelectedListener(OnLayerSelected);

        tileSetController.AddTileSelectedListener(OnTileSelected);

        bgSelector.AddBgSelectedListener(OnMapBgChanged);

        clear.onClick.AddListener(OnClearMap);
        genMapButton.onClick.AddListener(OnGenMapClicked);
        goButton.onClick.AddListener(() =>
        {
            if (OnSaveMapClicked())
            {
                SceneManager.LoadScene("DesignMapAutonomous");
            }
        });

        carSelectButton.onClick.AddListener(OnCarSelected);
        loadMapButton.onClick.AddListener(OnLoadMapClicked);
        saveAsButton.onClick.AddListener(OnSaveAsClicked);
        saveMapButton.onClick.AddListener(() =>
        {
            OnSaveMapClicked();
        });
    }

    void OnMapBgChanged(int id)
    {
        bgId = id;
        bg.sprite = bgSprites[id];
    }

    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    void OnLoadMapClicked()
    {
        // open map popup
        BrowseMapPopup.OpenPopup(DoLoadMap, true);
    }

    void DoLoadMap(string map_name, bool isDefaultMap)
    {
        if (string.IsNullOrEmpty(map_name))
        {
            return;
        }
        Debug.LogError("Load Map: " + map_name);
        MapData mapData = MapDataLoader.DataOfMap(map_name, isDefaultMap);
        // convert data to map
        OnClearMap();

        StartCoroutine(DoLoadCreateMap(mapData));
    }

    IEnumerator DoLoadCreateMap(MapData mapData)
    {
        yield return null;
        Debug.LogError("MapData: = " + JsonUtility.ToJson(mapData));

        OnMapBgChanged(mapData.bg_id);

        var cellDatas = mapData.cell_datas;
        //listCells = mapData.cell_datas;
        //var listCellDatas = cellDatas;
        for (int i = 0; i < cellDatas.Count; i++)
        {
            var cell = cellDatas[i];
            Debug.LogError("Draw cell " + i + JsonUtility.ToJson(cell));
            if (cell.tile_id < 0)
            {
                continue;
            }

            int col = cell.id_inmap % mapSize.x;
            int row = cell.id_inmap / mapSize.x;

            listCells[cell.id_inmap] = cell;

            CellResource cellResource = ResourceOfCell(cell.tile_id);
            pointerTileId = cell.tile_id;
            tileRot = cell.rot;
            pointerImage.sprite = cellResource.cellImagePrefab.sprite;
            pointerImage.rectTransform.sizeDelta = cellResource.cellImagePrefab.rectTransform.sizeDelta;
            pointerImage.rectTransform.localRotation = Quaternion.Euler(0, 0, tileRot);

            Image cellIcon = listCellIcons[cell.id_inmap];
            Vector2 localPoint;

            if (pointerTileId >= 0)
            {
                localPoint.x = (col - mapData.map_size.x / 2) * cellSize.x + neoCell.x;
                localPoint.y = (row - mapData.map_size.y / 2) * cellSize.y + neoCell.y;
                pointerImage.rectTransform.anchoredPosition = localPoint;

                cellIcon = GameObject.Instantiate<Image>(pointerImage, root);
                cellIcon.rectTransform.localScale = Vector3.one;
                cellIcon.name = string.Format("Cell[{0}]", (col + row * mapSize.x));
                cellIcon.rectTransform.anchoredPosition = pointerImage.rectTransform.anchoredPosition;
                cellIcon.sprite = pointerImage.sprite;
                cellIcon.raycastTarget = false;
                cellIcon.rectTransform.sizeDelta = pointerImage.rectTransform.sizeDelta;
                cellIcon.rectTransform.localRotation = pointerImage.rectTransform.localRotation;

                listCellIcons[cell.id_inmap] = cellIcon;
            }
        }
    }

    void OnSaveAsClicked()
    {
        //TextInputPopup.OpenPopup(DoSaveMap);
        BrowseMapPopup.OpenPopup(DoSaveMap, false);
    }

    bool OnSaveMapClicked()
    {
        if (string.IsNullOrEmpty(currentMapName))
        {
            //TextInputPopup.OpenPopup(DoSaveMap);
            BrowseMapPopup.OpenPopup(DoSaveMap, false);
            return false;
        }
        else
        {
            DoSaveMap(currentMapName, false);
            return true;
        }
    }

    void DoSaveMap(string map_name, bool isDefaultMap)
    {
        if (string.IsNullOrEmpty(map_name))
        {
            return;
        }
        currentMapName = map_name;
        OnGenMapClicked();

        var mapData = new MapData()
        {
            bg_id = bgId,
            map_size = mapSize,
            cell_datas = listCells,
            car_pos = carPos,
            map_name = currentMapName,
            sign_obj_datas = trafficSignMapGenerator.GetMapObjDatas(cellSize, neoCell)
        };
        mapData.CleanNullCell();
        string jsonData = JsonUtility.ToJson(mapData);
        Debug.LogError("" + jsonData);
        //MapData.SaveInstantMapJson(jsonData);


        StartCoroutine(SaveMapData(mapData));
        //string path = Application.persistentDataPath
    }

    IEnumerator SaveMapData(MapData mapData)
    {
        yield return null;
        var texture2D = ToTexture2D(mapRenderTexture);

        MapDataLoader.SaveMap(mapData, texture2D);
    }

    void OnClearMap()
    {
        ClearCellIcons();
        for (int i = 0; i < listCells.Count; i++)
        {
            listCells[i].tile_id = -1;
        }
    }

    void ClearCellIcons()
    {
        for (int i = 0; i < listCellIcons.Count; i++)
        {
            if (listCellIcons[i] != null)
            {
                GameObject.DestroyImmediate(listCellIcons[i].gameObject);
            }
        }
    }


    void OnGenMapClicked()
    {
        //worldMapBuilder.GenMap(mapSize, listCells, Vector2.one);
        var mapData = new MapData() {
            bg_id = bgId,
            map_size = mapSize,
            cell_datas = listCells,
            car_pos = carPos,

            sign_obj_datas = trafficSignMapGenerator.GetMapObjDatas(cellSize, neoCell)
        };
        worldMapBuilder.GenMap(mapData);
    }

    private void Update()
    {
        if (IsMoving)
        {
            UpdateMovingCellPos();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Do Menu Here
            SceneManager.LoadScene("MenuScene");
        }
    }

    void UpdateMovingCellPos()
    {
        Vector2 localPoint;
        bool onDragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(root, Input.mousePosition, canvas.worldCamera, out localPoint);
        
        if (onDragging && Mathf.Abs(localPoint.x) * 2 < root.sizeDelta.x && Mathf.Abs(localPoint.y) * 2 < root.sizeDelta.y)
        {
            int col = Mathf.RoundToInt((localPoint.x - neoCell.x) / cellSize.x);
            int row = Mathf.RoundToInt((localPoint.y - neoCell.y) / cellSize.y);
            localPoint.x = col * cellSize.x + neoCell.x;
            localPoint.y = row * cellSize.y + neoCell.y;
            pointerImage.rectTransform.anchoredPosition = localPoint;
        }
    }

    void OnLayerSelected(MapLayer pMapLayer)
    {
        currentLayer = pMapLayer;
        Debug.LogError("select layer " + pMapLayer.ToString());

        mapInput.SwitchLayer(currentLayer);

        if (currentLayer == MapLayer.TileMap)
        {
            trafficSignMapGenerator.DoSleep();
            pointerImage.gameObject.SetActive(true);
        }
        else
        {
            pointerImage.gameObject.SetActive(false);

            trafficSignMapGenerator.DoActive();
        }
    }

    

    void OnTileSelected(int tileId, int rot, Image iconImage)
    {
        pointerTileId = tileId;

        if (currentLayer != MapLayer.TileMap)
        {
            return;
        }

        if (tileId >= 0)
        {
            // draw mode
            tileRot = rot;
            //pointerImage.sprite = iconImage.sprite;
            //pointerImage.rectTransform.sizeDelta = iconImage.rectTransform.sizeDelta;
            CellResource cellResource = ResourceOfCell(tileId);
            pointerImage.sprite = cellResource.cellImagePrefab.sprite;
            pointerImage.rectTransform.sizeDelta = cellResource.cellImagePrefab.rectTransform.sizeDelta;

            pointerImage.rectTransform.localRotation = Quaternion.Euler(0, 0, tileRot);
            pointerImage.color = iconImage.color;
        }
        else
        {
            // del mode
            pointerImage.sprite = null;
            pointerImage.rectTransform.sizeDelta = Vector2.one * 100;
            pointerImage.rectTransform.localRotation = Quaternion.identity;
            pointerImage.color = Color.red;
            URenderUtils.SetAlpha(pointerImage, 0.5f);
        }
        onCarDraw = false;
    }

    void OnCarSelected()
    {
        if (currentLayer != MapLayer.TileMap)
        {
            return;
        }

        onCarDraw = true;
        pointerImage.sprite = carImage.sprite;
        pointerImage.rectTransform.sizeDelta = carImage.rectTransform.sizeDelta;
        pointerImage.rectTransform.localRotation = carImage.rectTransform.localRotation;
        pointerImage.color = carImage.color;
    }

    

    void OnBeginDragging(Vector2 screenPos)
    {
        if (currentLayer != MapLayer.TileMap)
        {
            return;
        }

        drawState = DrawState.Dragging;

        ProcessDrag();
    }

    void OnMouseDragging(Vector2 screenPos)
    {
        if (currentLayer != MapLayer.TileMap)
        {
            return;
        }

        ProcessDrag();
    }

    void ProcessDrag()
    {
        Vector2 localPoint;
        bool onDragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(root, Input.mousePosition, canvas.worldCamera, out localPoint);

        if (onDragging && Mathf.Abs(localPoint.x) * 2 < root.sizeDelta.x && Mathf.Abs(localPoint.y) * 2 < root.sizeDelta.y)
        {
            int col = Mathf.RoundToInt((localPoint.x - neoCell.x) / cellSize.x);
            int row = Mathf.RoundToInt((localPoint.y - neoCell.y) / cellSize.y);
            localPoint.x = col * cellSize.x + neoCell.x;
            localPoint.y = row * cellSize.y + neoCell.y;
            pointerImage.rectTransform.anchoredPosition = localPoint;

            // add to this col, row
            col = col + mapSize.x / 2;
            row = row + mapSize.y / 2;
            int idInListCell = col + row * mapSize.x;

            if (onCarDraw)
            {
                carPos = new Vector2(col, row);
                carImage.rectTransform.anchoredPosition = localPoint;
                return;
            }
            var cell = listCells[idInListCell];
            cell.tile_id = pointerTileId;
            cell.rot = tileRot;

            Image cellIcon = listCellIcons[idInListCell];
            if (pointerTileId >= 0)
            {
                // draw mode
                if (cellIcon == null)
                {
                    cellIcon = GameObject.Instantiate<Image>(pointerImage, root);
                    cellIcon.rectTransform.localScale = Vector3.one;
                    cellIcon.name = string.Format("Cell[{0}]", (col + row * mapSize.x));
                    cellIcon.rectTransform.anchoredPosition = pointerImage.rectTransform.anchoredPosition;
                    cellIcon.raycastTarget = false;

                    listCellIcons[idInListCell] = cellIcon;
                }
                cellIcon.sprite = pointerImage.sprite;
                cellIcon.rectTransform.sizeDelta = pointerImage.rectTransform.sizeDelta;
                cellIcon.rectTransform.localRotation = pointerImage.rectTransform.localRotation;
            }
            else if (cellIcon != null)
            {
                // del mode
                GameObject.Destroy(cellIcon.gameObject);
            }
        }
    }

    void OnFinishDragging()
    {
        if (currentLayer != MapLayer.TileMap)
        {
            return;
        }

        drawState = DrawState.Moving;
    }

    CellResource ResourceOfCell(int cellId)
    {
        MapCellType cellType = (MapCellType)cellId;
        foreach (CellResource res in cellResources)
        {
            if (res.cellType == cellType)
            {
                return res;
            }
        }
        return cellResources[0];
    }

    

    bool IsMoving => drawState == DrawState.Moving;

    List<MapData> allMaps => MapDataLoader.Instance.allMaps;
}
