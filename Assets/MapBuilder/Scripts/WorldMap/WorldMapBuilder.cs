using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MapData;
using static TileMapController;

public class WorldMapBuilder : MonoBehaviour
{
    [System.Serializable]
    public class CellModel
    {
        public MapCellType cell_id;
        public Transform prefab;
    }

    [System.Serializable]
    public class SignModel
    {
        public TrafficSignType sign_type;
        public WorldTrafficSign prefab;
    }

    [SerializeField] List<GameObject> bgGameObjects = new List<GameObject>();
    [SerializeField] List<CellModel> cellModels = new List<CellModel>();
    [SerializeField] List<SignModel> signModels = new List<SignModel>();

    [SerializeField] Transform root;
    [SerializeField] Transform cellRoot;
    [SerializeField] Transform signRoot;
    [SerializeField] float cellSize = 1.0f;

    [SerializeField] Transform car;

    [SerializeField] bool autoLoadMap;

    [Header("MiniMap")]
    [SerializeField] RawImage miniMap;
    [SerializeField] Image miniMapCarIcon;

    [Header("Debug")]
    [SerializeField] bool m_IsLoadedMap = false;
    [SerializeField] MapData mapData;
    [SerializeField] List<MapCellData> listCellDatas;
    [SerializeField] List<MapSignData> listSignDatas;

    [SerializeField] List<Transform> listCells = new List<Transform>();
    [SerializeField] List<WorldTrafficSign> listSigns = new List<WorldTrafficSign>();

    public string debugData = "";

    private void Start()
    {
        if (autoLoadMap)
        {
            LoadSavedMap();
        }

        var savedGamesPath = Application.persistentDataPath + "/";
        Debug.LogError("DebugPath = " + savedGamesPath);
    }

    [ContextMenu("Load saved map")]
    void LoadSavedMap()
    {
        //string json = MapData.GetInstantMapJson();
        debugData = JsonUtility.ToJson(MapDataLoader.instanceMapData);
        GenMapByJson();
    }

    [ContextMenu("GenMap by Json")]
    public void GenMapByJson()
    {
        MapData mapData = JsonUtility.FromJson<MapData>(debugData);
        GenMap(mapData);
        if (miniMap != null)
        {
            Texture texture = MapDataLoader.Instance.currentMapdataTexture;
            if (texture != null)
            {
                miniMap.texture = texture;
            }
            else
            {
                StartCoroutine(LoadMiniMap(mapData));
            }
        }
    }

    private void Update()
    {
        if (miniMapCarIcon != null)
        {
            UpdateMiniMapCarPos();
        }
    }

    void UpdateMiniMapCarPos()
    {
        float mapWidth = cellSize * mapData.map_size.x;
        float mapHeight = cellSize * mapData.map_size.y;

        Vector3 offset = car.transform.position - root.transform.position;
        float xRatio = offset.x / mapWidth;
        float yRatio = offset.z / mapHeight;
        float pX = xRatio * miniMap.rectTransform.sizeDelta.x;
        float pY = yRatio * miniMap.rectTransform.sizeDelta.y;
        miniMapCarIcon.rectTransform.anchoredPosition = new Vector2(pX, pY);
    }

    IEnumerator LoadMiniMap(MapData mapData)
    {
        yield return null;
        string path = string.Format("file://{0}", MapDataLoader.PathOfMap(mapData.map_name));
        WWW www = new WWW(path);
        yield return www;
        miniMap.texture = www.texture;
        Debug.LogError("Loaded Icon of map " + path);
    }

    public void GenMap(MapData mapData)
    {
        this.mapData = mapData;
        GenMap(mapData.map_size, mapData.cell_datas, mapData.car_pos, mapData.sign_obj_datas);

        for (int i = 0; i < bgGameObjects.Count; i++)
        {
            bgGameObjects[i].SetActive(i == mapData.bg_id);
        }
    }

    public void GenMap(Vector2Int mapSize, List<MapCellData> cellDatas, Vector2 neoOffset,
        List<MapSignData> signDatas)
    {
        m_IsLoadedMap = false;
        foreach (var cell in listCells)
        {
            GameObject.Destroy(cell.gameObject);
        }
        listCells.Clear();


        listCellDatas = cellDatas;
        CreateTileCells(mapSize, cellDatas);

        // signal
        listSignDatas = signDatas;
        CreateSignObjs(signDatas);

        if (car != null)
        {
            float posCol = neoOffset.x - mapSize.x * 0.5f;
            float posRow = neoOffset.y - mapSize.y * 0.5f;
            var posOnRoot = new Vector3(posCol * cellSize, 0, posRow * cellSize);
            transform.position = car.position - posOnRoot;
        }

        m_IsLoadedMap = true;
    }

    void CreateSignObjs(List<MapSignData> signDatas)
    {
        for (int i = 0; i < signDatas.Count; i++)
        {
            var signData = signDatas[i];
            var signModel = SignModelOf(signData.sign_id);
            WorldTrafficSign worldTrafficSign = Instantiate(signModel.prefab, signRoot);
            Transform signTrans = worldTrafficSign.transform;
            signTrans.transform.localPosition = new Vector3(signData.pos.x * cellSize, 0, signData.pos.y * cellSize);
            signTrans.transform.localRotation = Quaternion.Euler(0, -signData.rot, 0);
            signTrans.transform.localScale = Vector3.one;
            listSigns.Add(worldTrafficSign);
        }
    }


    void CreateTileCells(Vector2Int mapSize, List<MapCellData> cellDatas)
    {
        for (int i = 0; i < cellDatas.Count; i++)
        {
            var cell = cellDatas[i];
            if (cell.tile_id < 0)
            {
                continue;
            }

            int col = cell.id_inmap % mapSize.x;
            int row = cell.id_inmap / mapSize.x;

            float posCol = col - mapSize.x * 0.5f;
            float posRow = row - mapSize.y * 0.5f;

            CellModel cellModel = CellModelOf(cell.tile_id);
            Transform cellTransform = Instantiate<Transform>(cellModel.prefab, cellRoot);
            cellTransform.localPosition = new Vector3(posCol * cellSize, 0, posRow * cellSize);
            cellTransform.localRotation = Quaternion.Euler(0, -cell.rot, 0);
            cellTransform.localScale = Vector3.one;
            listCells.Add(cellTransform);
            cellTransform.name = "Cell[" + i + "]" + " tile_" + cell.tile_id + " rot_" + cell.rot;
        }
    }

    CellModel CellModelOf(int cell_id)
    {
        MapCellType celltype = (MapCellType)cell_id;
        foreach (CellModel model in cellModels)
        {
            if (model.cell_id == celltype)
            {
                return model;
            }
        }
        return cellModels[0];
    }

    SignModel SignModelOf(int sign_id)
    {
        TrafficSignType signType = (TrafficSignType)sign_id;
        foreach (SignModel model in signModels)
        {
            if (model.sign_type == signType)
            {
                return model;
            }
        }
        return signModels[0];
    }

    public float mapWidth => cellSize* mapData.map_size.x;
    public float mapHeight => cellSize * mapData.map_size.y;
    public Vector3 posRoot => root.transform.position;
    public bool IsLoadedMap => m_IsLoadedMap;
}
