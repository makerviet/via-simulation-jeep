using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Koi.UI
{

    public class UCanvasUtils
    {
        public static CanvasScaler FindcanvasScaler(Transform child)
        {
            CanvasScaler scaler = child.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                return scaler;
            }
            if (child.parent != null)
            {
                return FindcanvasScaler(child);
            }
            return null;
        }

        public static Canvas FindCanvas(Transform child)
        {
            Canvas canvas = child.GetComponent<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }
            if (child.parent != null)
            {
                return FindCanvas(child.parent);
            }
            return null;
        }
    }
}