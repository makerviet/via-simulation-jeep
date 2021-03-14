using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSetLayerSlot : MonoBehaviour
{
    Action<bool> OnVisibleToggleListener;

    [SerializeField] Toggle toggle;


    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggle);
    }

    void OnToggle(bool isToggle)
    {
    }
}
