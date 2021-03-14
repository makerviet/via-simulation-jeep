using System;
using System.Collections;
using System.Collections.Generic;
using Koi.UI;
using UnityEngine;

public class TileMapInput : MonoBehaviour
{
    Action<Vector2> OnBeginDragListener;
    Action<Vector2> OnDraggingListener;
    Action OnFinishDragListener;
    Action OnRightMouseClickListener;

    [SerializeField] private RectDrag _rectDrag;

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        _rectDrag.AddBeginDragListener(OnBeginDrag);
        _rectDrag.AddDraggingListener(OnDragging);
        _rectDrag.AddFinishDragListener(OnFinishDrag);

        _rectDrag.AddPointerDownListener(OnPointerDown);
        _rectDrag.AddPointerUpListener(OnFinishDrag);
        _rectDrag.AddPointerExitListener(OnFinishDrag);
    }

    public void AddBeginDragListener(Action<Vector2> pListener)
    {
        OnBeginDragListener -= pListener;
        OnBeginDragListener += pListener;
    }

    public void AddDraggingListener(Action<Vector2> pListener)
    {
        OnDraggingListener -= pListener;
        OnDraggingListener += pListener;
    }

    public void AddFinishDragListener(Action pListener)
    {
        OnFinishDragListener -= pListener;
        OnFinishDragListener += pListener;
    }

    public void AddRightMouseClickListener(Action pListener)
    {
        OnRightMouseClickListener -= pListener;
        OnRightMouseClickListener += pListener;
    }


    public int debugPointerId = -1000;
    public Vector2 debugScreenPos = Vector2.zero;

    void OnPointerDown(int pointerId, Vector2 screenPos, int mouseId)
    {
        if (mouseId == 1)
        {
            OnRightMouseClickListener?.Invoke();
        }
        else
        {
            OnBeginDrag(pointerId, screenPos, mouseId);
        }
    }

    void OnBeginDrag(int pointerId, Vector2 screenPos, int mouseId)
    {
        if (mouseId != 0)
        {
            return;
        }
        debugPointerId = pointerId;
        debugScreenPos = screenPos;
        OnBeginDragListener?.Invoke(screenPos);
    }

    void OnDragging(int pointerId, Vector2 screenPos, int mouseId)
    {
        if (mouseId != 0)
        {
            return;
        }
        debugPointerId = pointerId;
        debugScreenPos = screenPos;

        OnDraggingListener?.Invoke(screenPos);
    }

    void OnFinishDrag(int pointerId)
    {
        debugPointerId = -1000;

        OnFinishDragListener?.Invoke();
    }
}
