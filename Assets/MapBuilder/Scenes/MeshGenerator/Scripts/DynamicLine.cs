using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
public class DynamicLine : MonoBehaviour
{
    [System.Serializable]
    public class BorderData
    {
        public bool isBorderShape;
        public bool isMainLinePointToO;
        public List<Transform> mainPoints;
        public List<Transform> extraPoints;
        public int mainPointThreshold = 5;
    }

    [SerializeField] bool exeInEditMode = false;

    [SerializeField] LineRenderer m_Line;
    [SerializeField] public List<Transform> pointers = new List<Transform>();
    [SerializeField] List<Transform> preAddPointers = new List<Transform>();

    [Header("Border")]
    [SerializeField] public BorderData borderData;

    public bool IsMainPoint(Transform point)
    {
        return borderData.mainPoints.Contains(point);
    }


    void Update()
    {
        if (exeInEditMode)
        {
            UpdatePositions();
        }
    }

    void UpdatePositions()
    {
        if (m_Line.positionCount < pointers.Count)
        {
            m_Line.positionCount = pointers.Count;
        }
        for (int i = 0; i < pointers.Count; i++)
        {
            m_Line.SetPosition(i, pointers[i].position);
        }
    }

    //[ContextMenu("Add points revert order")]
    public void AddPointRevertOrder()
    {
        for (int i = preAddPointers.Count-1; i >= 0; i--)
        {
            pointers.Add(preAddPointers[i]);
        }
    }

    public void BuildBorderData()
    {
        borderData.mainPoints.Clear();
        for (int i = 0; i < borderData.mainPointThreshold; i++)
        {
            borderData.mainPoints.Add(pointers[i]);
        }

        borderData.extraPoints.Clear();
        for (int i = borderData.mainPointThreshold; i < pointers.Count; i++)
        {
            borderData.extraPoints.Add(pointers[i]);
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(DynamicLine))]
public class DynamicLineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DynamicLine myScript = (DynamicLine)target;

        GUILayout.Label("");
        //GUILayout.Label("Update DigitRectTransform Size from prefab data or spritedata");

        if (GUILayout.Button("Add Points Revert Order"))
        {
            myScript.AddPointRevertOrder();
        }

        GUILayout.Label("");

        if (GUILayout.Button("Build border data"))
        {
            myScript.BuildBorderData();
        }

        GUILayout.Label("");
    }
}
#endif

