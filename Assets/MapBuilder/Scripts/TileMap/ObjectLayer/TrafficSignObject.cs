using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrafficSignObject : MonoBehaviour
{
    Action<TrafficSignType> OnSelectedListener;

    [SerializeField] TrafficSignType type;
    [SerializeField] RectTransform root;
    [SerializeField] Image iconImage;
    [SerializeField] Button selectButton;
    [SerializeField] float rot;

    public float Rotation => rot;
    public TrafficSignType SignType => type;
    public Button SelectBtn => selectButton;

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        //selectButton.onClick.AddListener(OnSelected);
    }

    //public void AddSelectedListener(Action<TrafficSignType> pListener)
    //{
    //    this.OnSelectedListener -= pListener;
    //    this.OnSelectedListener += pListener;
    //}

    public void UpdateRot(float pRot)
    {
        this.rot = pRot;
        root.localRotation = Quaternion.Euler(0, 0, rot);
    }

    public void OnSelected()
    {
        OnSelectedListener?.Invoke(type);
        selectButton.gameObject.SetActive(false);
    }

    public void OnUnSelect()
    {
        selectButton.gameObject.SetActive(true);
    }
}
