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
    [SerializeField] List<TrafficSignObject> trafficSignObjects = new List<TrafficSignObject>();
    [SerializeField] List<TrafficSignObject> unuseTrafficSignObjects = new List<TrafficSignObject>();

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

        Debug.LogWarning("Left click at Traffic Sign Mode");
        Vector2 localPoint;
        bool onMap = RectTransformUtility.ScreenPointToLocalPointInRectangle(signRoot, Input.mousePosition, canvas.worldCamera, out localPoint);

        if (onMap)
        {
            Debug.LogWarning("Left click on map");
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
                    Debug.LogWarning("Found sign obj " + signObj.name + " debugPos " + debugPoint);
                    break;
                }
            }


            // push on map
            if (!IsSelectOnSign && m_state == MapObjectState.S2_Drawing)
            {
                Debug.LogWarning("NOT Found sign obj, create new");
                TrafficSignRes res = ResourceOfSign(pointerSignId);
                signObject = Instantiate(res.trafficSignPrefab, signRoot);
                trafficSignObjects.Add(signObject);
                signObject.name = signObject.name + (signObject.transform.parent.childCount + 1);
                signObject.transform.localPosition = localPoint;
                signObject.OnSelected();
            }

            Debug.LogWarning("Setup for Pointer");
            if (signObject != null)
            {
                trafficSignPointer.gameObject.SetActive(true);
                trafficSignPointer.OnSelected(signObject);
                trafficSignPointer.transform.position = signObject.transform.position;
                Debug.LogWarning("Setup for Pointer Done");

                m_state = MapObjectState.S1_Selecting;
            }
            else
            {
                trafficSignPointer.OnUnSelected();
                trafficSignPointer.gameObject.SetActive(false);
            }
        }
    }

    void OnTrafficSignTileSelected(int signId, int id, Image pImage)
    {
        pointerSignId = signId;
        if (IsActing)
        {
            m_state = MapObjectState.S2_Drawing;
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
}
