using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextInputPopup : MonoBehaviour
{
    static TextInputPopup Instance;

    Action<string> OnPopupClosedListener;

    [SerializeField] RectTransform root;
    [SerializeField] InputField nameInputField;
    [SerializeField] Button okButton;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitListener();
    }

    void InitListener()
    {
        okButton.onClick.AddListener(OnOKButtonClicked);
    }

    public static void OpenPopup(Action<string> callback)
    {
        Instance.DoOpenPopup(callback);
    }

    public void DoOpenPopup(Action<string> callback)
    {
        OnPopupClosedListener = callback;
        root.gameObject.SetActive(true);
    }

    void OnOKButtonClicked()
    {
        OnPopupClosedListener?.Invoke(nameInputField.text);
        OnPopupClosedListener = null;
        root.gameObject.SetActive(false);
    }
}
