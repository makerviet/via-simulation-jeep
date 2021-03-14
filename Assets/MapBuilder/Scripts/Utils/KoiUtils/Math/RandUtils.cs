using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koi.MathUtils
{
    public class RandUtils
    {
        public static bool Bool { get { return Random.Range(0, 2) == 0; } }

        public static float Abs1 { get { return Random.Range(-1.0f, 1.0f); } }

        public static float Float0_1 { get { return Random.Range(0.0f, 1.0f); } }

        public static float Rand(Vector2 range)
        {
            return Random.Range(range.x, range.y);
        }

        public static float Rand(Vector2Int range)
        {
            return Random.Range(range.x, range.y);
        }
    }

}
