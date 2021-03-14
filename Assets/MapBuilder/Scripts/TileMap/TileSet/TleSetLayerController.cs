using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TleSetLayerController : MonoBehaviour
{
    Action<int> OnBgSelectedListener;

    [SerializeField] int selectingId = 0;
    [SerializeField] List<Button> bgBtns;
    [SerializeField] Transform selectBorder;

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        for (int i = 0; i < bgBtns.Count; i++)
        {
            int id = i;
            bgBtns[i].onClick.AddListener(() =>
            {
                OnBgSelected(id);
            });
        }
    }

    public void AddBgSelectedListener(Action<int> pListener)
    {
        OnBgSelectedListener -= pListener;
        OnBgSelectedListener += pListener;
    }

    void OnBgSelected(int id)
    {
        selectingId = id;
        selectBorder.localPosition = bgBtns[id].transform.localPosition;
        OnBgSelectedListener?.Invoke(id);
    }
}
