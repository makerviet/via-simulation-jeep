using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koi.Common
{
    public class URandUtils
    {
        public static bool R_Bool => (R_Abs1 >= 0);
        public static float R_Abs1 => Random.Range(-1.0f, 1.0f);
        public static float R_0_1 => Random.Range(0.0f, 1.0f);
    }
}