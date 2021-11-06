using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MapData;

public class MapObjectLayerGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TrafficSignRes
    {
        public TrafficSignType signType;
        public TrafficSignObject trafficSignPrefab;
    }


    public enum MapObjectState
    {
        S0_Idle,
        S1_Selecting,
        S2_Drawing
    }

    [SerializeField] Canvas canvas;
    [SerializeField] TileMapInput mapInput;

    [Header("Setup")]
    [SerializeField] RectTransform signRoot;
    [SerializeField] List<TrafficSignRes> trafficSignResources;
    [SerializeField] ObjectLayerPointer trafficSignPointer;

    [SerializeField] TileSetController trafficSignController;

    [Header("Debug")]
    [SerializeField] MapObjectState m_state = MapObjectState.S0_Idle;
    [SerializeField] TrafficSignObject m_SelectingObject;
    [SerializeField] List<TrafficSignObject> trafficSignObjects = new List<TrafficSignObject>();
    //[SerializeField] List<TrafficSignObject> unuseTrafficSignObjects = new List<TrafficSignObject>();

    [SerializeField] List<TrafficSignObject> roadCheckpoints = new List<TrafficSignObject>();
    //[SerializeField] List<TrafficSignObject> unuseRoadCheckpoints = new List<TrafficSignObject>();

    [SerializeField] int pointerSignId;

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        trafficSignController.AddTileSelectedListener(OnTrafficSignTileSelected);
        mapInput.AddLeftMouseClickListener(OnLeftClick);
    }

    public void DoActive()
    {
        m_state = MapObjectState.S1_Selecting;
        //trafficSignPointer.gameObject.SetActive(true);

        foreach (var trafficSign in trafficSignObjects)
        {
            trafficSign.OnUnSelect();
        }
    }

    public void DoSleep()
    {
        m_state = MapObjectState.S0_Idle;
        trafficSignPointer.gameObject.SetActive(false);
    }


    void OnLeftClick(Vector2 screenPos)
    {
        if (!IsActing)
        {
            return;
        }

        Debug.LogError("Left click at Traffic Sign Mode");
        Vector2 localPoint;
        bool onMap = RectTransformUtility.ScreenPointToLocalPointInRectangle(signRoot, Input.mousePosition, canvas.worldCamera, out localPoint);

        if (onMap)
        {
            Debug.LogError("Left click on map");
            bool IsSelectOnSign = false;
            TrafficSignObject signObject = null;

            foreach (var signObj in trafficSignObjects)
            {
                //if ((signObj.transform.localPosition - localPoint).)
                var signObjRect = signObj.SelectBtn.image.rectTransform;
                Vector2 debugPoint;
                bool onSignObj = RectTransformUtility.ScreenPointToLocalPointInRectangle(signObjRect, Input.mousePosition, canvas.worldCamera, out debugPoint);
                if (onSignObj && Mathf.Abs(debugPoint.x) < signObjRect.sizeDelta.x * 0.5f
                    && Mathf.Abs(debugPoint.y) < signObjRect.sizeDelta.y * 0.5f)
                {
                    signObject = signObj;
                    signObj.OnSelected();
                    IsSelectOnSign = true;
                    Debug.LogError("Found sign obj " + signObj.name + " debugPos " + debugPoint);
                    break;
                }
            }


            // push on map
            if (!IsSelectOnSign && m_state == MapObjectState.S2_Drawing)
            {
                Debug.LogError("NOT Found sign obj, create new");
                TrafficSignRes res = ResourceOfSign(pointerSignId);
                signObject = Instantiate(res.trafficSignPrefab, signRoot);
                if (signObject.SignType == TrafficSignType.S99_Score)
                {
                    roadCheckpoints.Add(signObject);
                    int order = roadCheckpoints.Count;
                    signObject.GetComponent<RoadCheckpointObject>().SetupOrder(order);
                }
                else
                {
                    trafficSignObjects.Add(signObject);
                }
                
                signObject.name = signObject.name + (signObject.transform.parent.childCount + 1);
                signObject.transform.localPosition = localPoint;
                signObject.OnSelected();
            }

            Debug.LogError("Setup for Pointer");
            if (signObject != null)
            {
                trafficSignPointer.gameObject.SetActive(true);
                trafficSignPointer.OnSelected(signObject);
                trafficSignPointer.transform.position = signObject.transform.position;
                Debug.LogError("Setup for Pointer Done");

                m_state = MapObjectState.S1_Selecting;
            }
            else
            {
                trafficSignPointer.OnUnSelected();
                trafficSignPointer.gameObject.SetActive(false);
            }
            m_SelectingObject = signObject;
        }
    }

    void OnTrafficSignTileSelected(int signId, int id, Image pImage)
    {
        if (pointerSignId != -1)
        {
            pointerSignId = signId;
            if (IsActing)
            {
                m_state = MapObjectState.S2_Drawing;
            }
        }
        else if (IsSelecting)
        {
            if (m_SelectingObject != null)
            {
                // remove selecting cell
                trafficSignPointer.OnUnSelected();
                m_state = MapObjectState.S0_Idle;
                var selectingObj = m_SelectingObject;

                if (trafficSignObjects.Contains(selectingObj))
                {
                    //unuseTrafficSignObjects.Add(selectingObj);
                    trafficSignObjects.Remove(selectingObj);
                    GameObject.DestroyImmediate(selectingObj);
                }
                else if (roadCheckpoints.Contains(selectingObj))
                {
                    //unuseRoadCheckpoints.Add(selectingObj);
                    roadCheckpoints.Remove(selectingObj);
                    GameObject.DestroyImmediate(selectingObj);
                    RefreshRoadCheckpontsList();
                }

                m_SelectingObject = null;
            }
        }
    }

    void RefreshRoadCheckpontsList()
    {
        for (int i = 0; i < roadCheckpoints.Count; i++)
        {
            roadCheckpoints[i].GetComponent<RoadCheckpointObject>().SetupOrder(i + 1);
        }
    }

    public List<MapSignData> GetMapObjDatas(Vector2 mapCellDesignSize, Vector2 neoCell)
    {
        List<MapSignData> result = new List<MapSignData>();
        foreach (var signObj in trafficSignObjects)
        {
            var data = new MapSignData();
            data.sign_id = (int)signObj.SignType;
            data.rot = signObj.Rotation;
            data.pos = signObj.transform.localPosition;
            data.pos.x = (data.pos.x - neoCell.x) / mapCellDesignSize.x;
            data.pos.y = (data.pos.y - neoCell.y) / mapCellDesignSize.y;
            result.Add(data);
        }

        return result;
    }

    public List<RoadCheckPointData> GetRoadCheckPointDatas(Vector2 mapCellDesignSize, Vector2 neoCell)
    {
        List<RoadCheckPointData> result = new List<RoadCheckPointData>();

        foreach (var checkpoint in roadCheckpoints)
        {
            var obj = checkpoint.GetComponent<RoadCheckpointObject>();
            var data = new RoadCheckPointData();
            data.order_id = obj.OrderInPath;
            data.score = obj.Score;
            data.rot = checkpoint.Rotation;
            data.pos = checkpoint.transform.localPosition;
            data.pos.x = (data.pos.x - neoCell.x) / mapCellDesignSize.x;
            data.pos.y = (data.pos.y - neoCell.y) / mapCellDesignSize.y;
            result.Add(data);
        }

        return result;
    }


    TrafficSignRes ResourceOfSign(int signId)
    {
        TrafficSignType signType = (TrafficSignType)signId;
        foreach (TrafficSignRes res in trafficSignResources)
        {
            if (res.signType == signType)
            {
                return res;
            }
        }
        return trafficSignResources[0];
    }


    bool IsActing => (m_state == MapObjectState.S1_Selecting || m_state == MapObjectState.S2_Drawing);
    bool IsSelecting => m_state == MapObjectState.S1_Selecting;
    bool IsDrawing => m_state == MapObjectState.S2_Drawing;
}
