using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koi.Common
{
    public class UDateTimeUtils
    {
        static DateTime originPoint = new DateTime(1970,1,1,0,0,0);

        public static int nDayFrom1970 => DateTime.Now.Subtract(originPoint).Days;

        public static int nDayFrom1970_Of(DateTime pDateTime)
        {
            return pDateTime.Subtract(originPoint).Days;
        }

        public static long nowSecondValue
        {
            get
            {
                var now = DateTime.Now;
                int dateValue = (int)(DateTime.Now.Subtract(originPoint).TotalDays);
                return (dateValue * 86400 + now.Hour * 3600 + now.Minute * 60 + now.Second);
            }
        }
    }
}
