using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileMapScaler : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Text sizeText;
    [SerializeField] RectTransform mapRootScale;
    [SerializeField] RectTransform contentRoot;


    void Start()
    {
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        mapRootScale.localScale = Vector3.one * value;

        Vector2 size = mapRootScale.sizeDelta * value;
        contentRoot.sizeDelta = size;

        sizeText.text = string.Format("x{0:0.0}", value);
    }
}
