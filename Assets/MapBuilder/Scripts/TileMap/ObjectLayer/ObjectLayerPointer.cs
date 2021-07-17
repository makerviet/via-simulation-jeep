using System.Collections;
using System.Collections.Generic;
using Koi.Common;
using Koi.UI;
using UnityEngine;
using UnityEngine.UI;

public class ObjectLayerPointer : MonoBehaviour
{
    public enum State
    {
        S0_Idle,
        S1_OnMoveX,
        S2_OnMoveY,
        S3_OnMoveXY
    }

    [SerializeField] RectTransform root;
    [SerializeField] RectDrag axisX;
    [SerializeField] RectDrag axisY;
    [SerializeField] RectDrag center;
    [SerializeField] Slider rotationSlider;

    [SerializeField] State m_state = State.S0_Idle;

    [SerializeField] TrafficSignObject selectingObject;

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        rotationSlider.onValueChanged.AddListener(OnRotChanged);

        center.AddBeginDragListener(OnBeginDrag);
        center.AddDraggingListener(OnDragging);
        center.AddFinishDragListener(OnFinishDrag);
    }

    void OnRotChanged(float pValue)
    {
        selectingObject?.UpdateRot(pValue);
    }

    public void OnSelected(TrafficSignObject pObject)
    {
        Debug.LogError("OnSelected obj " + pObject.name);
        if (selectingObject != null)
        {
            selectingObject.OnUnSelect();
        }

        Debug.LogError("Setup new obj with rot = " + pObject.Rotation);
        this.selectingObject = pObject;
        this.rotationSlider.value = pObject.Rotation;
    }

    void UpdateSelectingObjectPos()
    {
        selectingObject.transform.position = root.position;
    }

    Vector2 savedLocalTouchPos;
    Vector2 savedLocalPos;

    void OnBeginDrag(int pointerId, Vector2 screenPos, int mouseId)
    {
        Debug.LogError("On Begin Drag " + screenPos);
        if (mouseId != 0)
        {
            return;
        }
        m_state = State.S3_OnMoveXY;
        Vector2 currentLocalTouchPos;
        bool onMap = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root.parent.GetComponent<RectTransform>(),Input.mousePosition,
            canvas.worldCamera, out currentLocalTouchPos);
        savedLocalTouchPos = currentLocalTouchPos;
        savedLocalPos = root.localPosition;
    }

    void OnDragging(int pointerId, Vector2 screenPos, int mouseId)
    {
        if (mouseId != 0)
        {
            return;
        }

        Vector2 currentLocalTouchPos;
        bool onMap = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root.parent.GetComponent<RectTransform>(), Input.mousePosition,
            canvas.worldCamera, out currentLocalTouchPos);
        root.localPosition = savedLocalPos + currentLocalTouchPos - savedLocalTouchPos;

        UpdateSelectingObjectPos();
    }

    void OnFinishDrag(int pointerId)
    {
        m_state = State.S0_Idle;
    }

    void OnBeginDragX(int pointerId, Vector2 screenPos, int mouseId)
    {
        Debug.LogError("On Begin Drag " + screenPos);
        if (mouseId != 0)
        {
            return;
        }
    }

    void OnDraggingX(int pointerId, Vector2 screenPos, int mouseId)
    {
        if (mouseId != 0)
        {
            return;
        }
    }

    void OnFinishDragX(int pointerId)
    {

    }

    void OnBeginDragY(int pointerId, Vector2 screenPos, int mouseId)
    {
        Debug.LogError("On Begin Drag " + screenPos);
        if (mouseId != 0)
        {
            return;
        }
    }

    void OnDraggingY(int pointerId, Vector2 screenPos, int mouseId)
    {
        if (mouseId != 0)
        {
            return;
        }
    }

    void OnFinishDragY(int pointerId)
    {

    }


    Canvas m_canvas;
    Canvas canvas
    {
        get
        {
            if (m_canvas == null)
            {
                m_canvas = UMonoUtils.FindComponentFromChild<Canvas>(transform);
            }
            return m_canvas;
        }
    }
}
