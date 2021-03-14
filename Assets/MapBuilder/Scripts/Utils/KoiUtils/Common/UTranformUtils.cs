using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koi.Common
{
    public class UTransformUtils
    {
        public static void ResetTransorm(Transform target)
        {
            UTranformUtils.ResetTransorm(target);
        }

        public static void SetPosX(Transform target, float posX)
        {
            UTranformUtils.SetPosX(target, posX);
        }

        public static void SetPosY(Transform target, float posY)
        {
            UTranformUtils.SetPosY(target, posY);
        }

        public static void SetPosZ(Transform target, float posZ)
        {
            UTranformUtils.SetPosZ(target, posZ);
        }

        public static void SetPosXY(Transform target, float posX, float posY)
        {
            UTranformUtils.SetPosXY(target, posX, posY);
        }

        public static void SetPosXY(Transform target, Vector2 posXY)
        {
            UTranformUtils.SetPosXY(target, posXY);
        }

        public static void SetPos2D(Transform target, Vector2 pos2D)
        {
            SetPosXY(target, pos2D);
        }

        public static void SetPos2D(Transform target, Transform source)
        {
            SetPos2D(target, source.position);
        }

        public static void SetLocalPosX(Transform target, float posX)
        {
            UTranformUtils.SetLocalPosX(target, posX);
        }

        public static void SetLocalPosY(Transform target, float posY)
        {
            UTranformUtils.SetLocalPosY(target, posY);
        }

        public static void SetLocalPosZ(Transform target, float posZ)
        {
            UTranformUtils.SetLocalPosZ(target, posZ);
        }

        public static void SetLocalScaleX(Transform target, float scaleX)
        {
            var localScale = target.localScale;
            localScale.x = scaleX;
            target.localScale = localScale;
        }

        public static void SetLocalScaleY(Transform target, float scaleY)
        {
            var localScale = target.localScale;
            localScale.y = scaleY;
            target.localScale = localScale;
        }

        public static void SetLocalScaleZ(Transform target, float scaleZ)
        {
            var localScale = target.localScale;
            localScale.z = scaleZ;
            target.localScale = localScale;
        }
    }

    public class UTranformUtils
    {
        public static void ResetTransorm(Transform target)
        {
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }

        public static void SetPosX(Transform target, float posX)
        {
            var pos = target.position;
            pos.x = posX;
            target.position = pos;
        }

        public static void SetPosY(Transform target, float posY)
        {
            var pos = target.position;
            pos.y = posY;
            target.position = pos;
        }

        public static void SetPosZ(Transform target, float posZ)
        {
            var pos = target.position;
            pos.z = posZ;
            target.position = pos;
        }

        public static void SetPosXY(Transform target, float posX, float posY)
        {
            var pos = target.position;
            pos.x = posX;
            pos.y = posY;
            target.position = pos;
        }

        public static void SetPosXY(Transform target, Vector2 posXY)
        {
            var pos = target.position;
            pos.x = posXY.x;
            pos.y = posXY.y;
            target.position = pos;
        }

        public static void SetPos2D(Transform target, Vector2 pos2D)
        {
            SetPosXY(target, pos2D);
        }

        public static void SetLocalPosX(Transform target, float posX)
        {
            var pos = target.localPosition;
            pos.x = posX;
            target.localPosition = pos;
        }

        public static void SetLocalPosY(Transform target, float posY)
        {
            var pos = target.localPosition;
            pos.y = posY;
            target.localPosition = pos;
        }

        public static void SetLocalPosZ(Transform target, float posZ)
        {
            var pos = target.localPosition;
            pos.z = posZ;
            target.localPosition = pos;
        }
    }
}
