using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        trafficSignPointer.gameObject.SetActive(true);

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
        if (m_state != MapObjectState.S1_Selecting
            && m_state != MapObjectState.S2_Drawing)
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

            //PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            //eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //List<RaycastResult> results = new List<RaycastResult>();
            //EventSystem.current.RaycastAll(eventDataCurrentPosition, results);


            // push on map
            if (!IsSelectOnSign)
            {
                Debug.LogError("NOT Found sign obj, create new");
                TrafficSignRes res = ResourceOfSign(pointerSignId);
                signObject = Instantiate(res.trafficSignPrefab, signRoot);
                trafficSignObjects.Add(signObject);
                signObject.name = signObject.name + (signObject.transform.parent.childCount + 1);
                signObject.transform.localPosition = localPoint;
            }

            Debug.LogError("Setup for Pointer");
            trafficSignPointer.gameObject.SetActive(true);
            trafficSignPointer.OnSelected(signObject);
            trafficSignPointer.transform.position = signObject.transform.position;
            Debug.LogError("Setup for Pointer Done");
        }

        //int col = Mathf.RoundToInt((localPoint.x - neoCell.x) / cellSize.x);
        //int row = Mathf.RoundToInt((localPoint.y - neoCell.y) / cellSize.y);
        //localPoint.x = col * cellSize.x + neoCell.x;
        //localPoint.y = row * cellSize.y + neoCell.y;
        //pointerImage.rectTransform.anchoredPosition = localPoint;

    }

    void OnTrafficSignTileSelected(int signId, int id, Image pImage)
    {
        pointerSignId = signId;
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
}
