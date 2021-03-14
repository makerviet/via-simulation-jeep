using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koi.Common
{

    public class UColorUtils
    {
        public static void ParseHexString(string hexString, out Color color)
        {
            if (hexString[0] != '#')
            {
                hexString = "#" + hexString;
            }
            ColorUtility.TryParseHtmlString(hexString, out color);
        }

        public static Color TryParseHexString(string hexString, Color defaulResult)
        {
            Color result = defaulResult;
            ParseHexString(hexString, out result);
            return result;
        }
    }
}