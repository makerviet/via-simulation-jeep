using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayfabLoadingPopup : MonoBehaviour
{
    [SerializeField] Text loadingText;
    [SerializeField] List<string> msgTexts = new List<string>();
    [SerializeField] float textDuration = 0.5f;

    void Update()
    {
        int id = (int)(Time.time / Mathf.Max(0.01f, textDuration)) % msgTexts.Count;
        loadingText.text = msgTexts[id];
    }
}
