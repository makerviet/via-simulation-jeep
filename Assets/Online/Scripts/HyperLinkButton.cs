using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HyperLinkButton : MonoBehaviour
{
    [SerializeField] Button mButton;
    [SerializeField] Text mText;
    [SerializeField] string url;

    Button button
    {
        get
        {
            if (mButton == null)
            {
                mButton = GetComponent<Button>();
            }
            return mButton;
        }
    }

    private void Start()
    {
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        if (string.IsNullOrEmpty(url))
        {
            url = mText.text;
        }
        Application.OpenURL(url);
    }
}
