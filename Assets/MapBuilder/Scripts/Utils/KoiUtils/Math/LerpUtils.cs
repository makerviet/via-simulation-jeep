using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Koi.MathUtils
{
    public enum EasyType
    {
        None = 0,
        InQuad,
        OutQuad,
        InOutQuad,
        InQuart,
        OutQuart,
        InOutQuart,
        InSine,
        OutSine,
        InOutSine,
        InBounce,
        OutBounce,
        InOutBounce
    }

    public class LerpUtils
    {
        public delegate float LerpAny(float start, float end, float ratio);

        public static float Lerp(float start, float end, float ratio, EasyType easyType = EasyType.None)
        {
            var lerpFunction = FindLerp(easyType);
            return lerpFunction(start, end, ratio);
        }

        public static Vector2 Lerp(Vector2 start, Vector2 end, float ratio, EasyType easyType = EasyType.None)
        {
            var lerpFunction = FindLerp(easyType);
            return new Vector2(x: lerpFunction(start.x, end.x, ratio),
                               y: lerpFunction(start.y, end.y, ratio));
        }

        public static Vector3 Lerp(Vector3 start, Vector3 end, float ratio, EasyType easyType = EasyType.None)
        {
            var lerpFunction = FindLerp(easyType);
            return new Vector3(x: lerpFunction(start.x, end.x, ratio),
                               y: lerpFunction(start.y, end.y, ratio),
                               z: lerpFunction(start.z, end.z, ratio));
        }


        public static LerpAny FindLerp(EasyType easyType)
        {
            switch (easyType)
            {
                case EasyType.None:
                    return Mathf.Lerp;

                case EasyType.InQuad:
                    return LerpInQuad;

                case EasyType.OutQuad:
                    return LerpOutQuad;

                case EasyType.InOutQuad:
                    return LerpInOutQuad;

                case EasyType.InQuart:
                    return LerpInQuart;

                case EasyType.OutQuart:
                    return LerpOutQuart;

                case EasyType.InOutQuart:
                    return LerpInOutQuart;

                case EasyType.InSine:
                    return LerpInSine;

                case EasyType.OutSine:
                    return LerpOutSine;

                case EasyType.InOutSine:
                    return LerpInOutSine;

                case EasyType.InBounce:
                    return LerpInBounce;

                case EasyType.OutBounce:
                    return LerpOutBounce;

                case EasyType.InOutBounce:
                    return LerpInOutBounce;

                default:
                    return Mathf.Lerp;
            }
        }


        #region Quard
        public static float LerpInQuad(float start, float end, float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            end -= start;
            return end * ratio * ratio + start;
        }

        public static float LerpOutQuad(float start, float end, float ratio)
        {
            end -= start;
            return -end * ratio * (ratio - 2) + start;
        }

        public static float LerpInOutQuad(float start, float end, float ratio)
        {
            ratio /= 0.5f;
            end -= start;
            if (ratio < 1) return end * 0.5f * ratio * ratio + start;
            ratio--;
            return -end * 0.5f * (ratio * (ratio - 2) - 1) + start;
        }
        #endregion

        #region Quart
        public static float LerpInQuart(float start, float end, float ratio)
        {
            end -= start;
            ratio = ratio * ratio;
            return end * ratio * ratio + start;
        }

        public static float LerpOutQuart(float start, float end, float ratio)
        {
            ratio--;
            end -= start;
            ratio = ratio * ratio;
            return -end * (ratio * ratio - 1) + start;
        }

        public static float LerpInOutQuart(float start, float end, float ratio)
        {
            ratio /= .5f;
            end -= start;
            if (ratio < 1)
            {
                ratio = ratio * ratio;
                return end * 0.5f * ratio * ratio + start;
            }
            ratio -= 2;
            ratio = ratio * ratio;
            return -end * 0.5f * (ratio * ratio - 2) + start;
        }
        #endregion


        #region sine
        public static float LerpInSine(float start, float end, float ratio)
        {
            end -= start;
            return -end * Mathf.Cos(ratio * (Mathf.PI * 0.5f)) + end + start;
        }

        public static float LerpOutSine(float start, float end, float ratio)
        {
            end -= start;
            return end * Mathf.Sin(ratio * (Mathf.PI * 0.5f)) + start;
        }

        public static float LerpInOutSine(float start, float end, float ratio)
        {
            end -= start;
            return -end * 0.5f * (Mathf.Cos(Mathf.PI * ratio) - 1) + start;
        }
        #endregion

        #region bounce
        /* GFX47 MOD START */
        private static float LerpInBounce(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            return end - LerpOutBounce(0, end, d - value) + start;
        }
        /* GFX47 MOD END */

        /* GFX47 MOD START */
        //private float bounce(float start, float end, float value){
        private static float LerpOutBounce(float start, float end, float value)
        {
            value /= 1f;
            end -= start;
            if (value < (1 / 2.75f))
            {
                return end * (7.5625f * value * value) + start;
            }
            else if (value < (2 / 2.75f))
            {
                value -= (1.5f / 2.75f);
                return end * (7.5625f * (value) * value + .75f) + start;
            }
            else if (value < (2.5 / 2.75))
            {
                value -= (2.25f / 2.75f);
                return end * (7.5625f * (value) * value + .9375f) + start;
            }
            else
            {
                value -= (2.625f / 2.75f);
                return end * (7.5625f * (value) * value + .984375f) + start;
            }
        }
        /* GFX47 MOD END */

        /* GFX47 MOD START */
        private static float LerpInOutBounce(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            if (value < d / 2) return LerpInBounce(0, end, value * 2) * 0.5f + start;
            else return LerpOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
        }
        /* GFX47 MOD END */
        #endregion
    }
}


