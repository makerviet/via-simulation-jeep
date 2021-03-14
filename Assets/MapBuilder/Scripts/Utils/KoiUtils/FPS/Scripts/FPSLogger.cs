using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSLogger : MonoBehaviour
{
    [SerializeField] Text displayText;
    float deltaTime = 0.016f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text = string.Format("FPS: {0:##.0}", fps);
        }
    }

    public float fps
    {
        get
        {
            return 1.0f / deltaTime;
        }
    }
}
