using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSetCell : MonoBehaviour
{
    public enum CellType
    {
        TileMap = 0,
        Sign = 1
    }

    Action<int> OnSelectedListener;

    Button m_button;
    Image m_image;

    [SerializeField] CellType type = CellType.TileMap;
    [SerializeField] MapCellType cellId;
    [SerializeField] TrafficSignType signId;
    
    public Image iconImage
    {
        get
        {
            if (m_image == null)
            {
                m_image = GetComponent<Image>();
            }
            return m_image;
        }
    }

    public Vector2 iconSize => iconImage.rectTransform.sizeDelta;
    public RectTransform rectTransform => iconImage.rectTransform;
    public Sprite sprite => iconImage.sprite;

    //Outline outline
    //{
    //    get
    //    {
    //        if (m_outline == null)
    //        {
    //            m_outline = GetComponent<Outline>();
    //        }
    //        return m_outline;
    //    }
    //}


    Button button
    {
        get
        {
            if (m_button == null)
            {
                m_button = GetComponent<Button>();
            }
            return m_button;
        }
    }

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        button.onClick.AddListener(OnCellSelected);
    }

    public void AddSelectedListener(Action<int> pListener)
    {
        OnSelectedListener -= pListener;
        OnSelectedListener += pListener;
    }


    void OnCellSelected()
    {
        if (type == CellType.TileMap)
        {
            OnSelectedListener?.Invoke((int)cellId);
        }
        else
        {
            OnSelectedListener?.Invoke((int)signId);
        }
    }

    public void SetSelectState(bool isSelect)
    {
        //outline.enabled = isSelect;
    }


}
