using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class MemoryUtils
{

    [MenuItem("Utils/Memory/ClearCache")]
    static void ClearCache()
    {
        //Clears all of the caches
        bool success = Caching.ClearCache();

        if (!success)
        {
            Debug.Log("Unable to clear cache");
        }
    }


    [MenuItem("Utils/Memory/ClearPlayerPrefs")]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}

#endif