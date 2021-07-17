using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koi.Common
{
    public class UMonoUtils
    {


        public static T FindComponentFromChild<T>(Transform trans) where T : Behaviour
        {
            if (trans == null)
            {
                return null;
            }
            var result = trans.GetComponent<T>();
            if (result != null)
            {
                return result;
            }
            return FindComponentFromChild<T>(trans.parent);
        }
    }
}