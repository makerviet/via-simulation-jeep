using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koi.Common
{
    public class URectTransformUtils
    {
        public static void ResetAnchoredTransorm(RectTransform target)
        {
            target.anchoredPosition3D = Vector3.zero;
        }

        public static void SetAnchoredPosX(RectTransform target, float posX)
        {
            var pos = target.anchoredPosition;
            pos.x = posX;
            target.anchoredPosition = pos;
        }

        public static void SetAnchoredPosY(RectTransform target, float posY)
        {
            var pos = target.anchoredPosition;
            pos.y = posY;
            target.anchoredPosition = pos;
        }

        public static void SetAnchoredPosZ(RectTransform target, float posZ)
        {
            var pos = target.anchoredPosition3D;
            pos.z = posZ;
            target.anchoredPosition3D = pos;
        }

        public static void SetAnchoredPosXY(RectTransform target, float posX, float posY)
        {
            var pos = target.anchoredPosition;
            pos.x = posX;
            pos.y = posY;
            target.anchoredPosition = pos;
        }

        public static void SetAnchoredPosXY(RectTransform target, Vector2 posXY)
        {
            SetAnchoredPosXY(target, posXY.x, posXY.y);
        }
    }
}