using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Koi.Common
{
    public class URenderUtils
    {
        public static void SetAlpha(SpriteRenderer pRenderer, float pAlpha)
        {
            var color = pRenderer.color;
            color.a = pAlpha;
            pRenderer.color = color;
        }

        public static void SetAlpha(Image pImage, float pAlpha)
        {
            var color = pImage.color;
            color.a = pAlpha;
            pImage.color = color;
        }

        public static void SetAlpha(RawImage pRawImage, float pAlpha)
        {
            var color = pRawImage.color;
            color.a = pAlpha;
            pRawImage.color = color;
        }

        public static void SetAlpha(Text pText, float pAlpha)
        {
            var color = pText.color;
            color.a = pAlpha;
            pText.color = color;
        }
    }
}
