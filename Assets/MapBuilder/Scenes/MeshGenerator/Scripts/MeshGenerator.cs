using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshGenerator : MonoBehaviour
{
    [System.Serializable]
    public class BorderLineData
    {
        public string name;
        public DynamicLine borderLine;
    }

    public string model_name;
    [SerializeField] DynamicLine mainPartLines;

    [SerializeField] List<BorderLineData> borders = new List<BorderLineData>();

    public void GenAllMesh()
    {
        //StartCoroutine(DoWriteAllMeshFile());
        DoWriteAllMeshFile();
    }

    public void GenCenter()
    {
        //StartCoroutine(DoWriteCenterFile());
        DoWriteCenterFile();
    }

    void DoWriteAllMeshFile()
    {
        string path = string.Format("Assets/Resources/Export/{0}_allmesh.obj", model_name);
        Debug.LogError("Path: " + path);
        //yield return null;
        System.IO.StreamWriter write = new System.IO.StreamWriter(path, true);
        string output = StringOfMainBoard();
        //yield return null;

        string borderOutput = StringOfBorder(mainPartLines.pointers.Count);
        output += borderOutput;

        //yield return null;
        Debug.LogError("Main Plane datas");
        Debug.LogError("Write to file: " + path);
        write.Write(output);
        write.Close();
        //yield return null;
        Debug.LogError("Write done!");
    }

    string StringOfBorder(int startId)
    {
        string output = "\ng border\n\n";
        int curStartId = startId;

        string outputPlane = "";

        foreach (BorderLineData border in borders)
        {
            output += "\n# v border part " + border.name + "\n";
            outputPlane += "#\n plane part " + border.name + "\n";
            var data = border.borderLine.borderData;

            // point
            var mainPoints = data.mainPoints;
            for (int i = 0; i < mainPoints.Count; i++)
            {
                var pos = mainPoints[i].position;
                output += string.Format("v {0} {1} {2}\n", -pos.x, pos.y, pos.z);
            }
            output += "#up\n";
            for (int i = 0; i < mainPoints.Count; i++)
            {
                var pos = mainPoints[i].position;
                output += string.Format("v {0} {1} {2}\n", -pos.x, pos.y + 0.1f, pos.z);
            }
            output += "\n";

            var extraPoints = data.extraPoints;
            for (int i = 0; i < extraPoints.Count; i++)
            {
                var pos = extraPoints[i].position;
                output += string.Format("v {0} {1} {2}\n", -pos.x, pos.y + 0.1f, pos.z);
            }
            output += "#down\n";
            for (int i = 0; i < extraPoints.Count; i++)
            {
                var pos = extraPoints[i].position;
                output += string.Format("v {0} {1} {2}\n", -pos.x, pos.y, pos.z);
            }
            output += "\n";

            // plane
            for (int i = 1; i < mainPoints.Count; i++)
            {
                int id1 = curStartId + i;
                int id2 = id1 + 1;
                int id4 = id1 + mainPoints.Count;
                int id3 = id4 + 1;
                outputPlane += string.Format("f {0} {1} {2} {3}\n", id1, id2, id3, id4);
            }
            outputPlane += "f";
            for (int i = 1; i <= mainPoints.Count; i++)
            {
                outputPlane += string.Format(" {0}", (curStartId + i + mainPoints.Count));
            }
            for (int i = 1; i <= extraPoints.Count; i++)
            {
                outputPlane += string.Format(" {0}", (curStartId + mainPoints.Count * 2 + i));
            }
            outputPlane += "\n";
            for (int i = 1; i < extraPoints.Count; i++)
            {
                int id4 = curStartId + mainPoints.Count * 2 + i;
                int id1 = id4 + extraPoints.Count;
                int id2 = id1 + 1;
                int id3 = id4 + 1;

                outputPlane += string.Format("f {0} {1} {2} {3}\n", id1, id2, id3, id4);
            }

            curStartId += (mainPoints.Count + extraPoints.Count) * 2;
        }

        output += "\ns off\n";
        output += outputPlane;

        return output;
    }

    void DoWriteCenterFile()
    {
        string path = string.Format("Assets/Resources/Export/{0}_center.obj", model_name);
        Debug.LogError("Path: " + path);
        //yield return null;
        System.IO.StreamWriter write = new System.IO.StreamWriter(path, true);

        var output = StringOfMainBoard();

        //yield return null;
        Debug.LogError("Main Plane datas");
        Debug.LogError("Write to file: " + path);
        write.Write(output);
        write.Close();
        //yield return null;
        Debug.LogError("Write done!");
    }

    string StringOfMainBoard()
    {
        string output = "";
        output += "g main_board\n\n";

        var verts = mainPartLines.pointers;
        for (int i = 0; i < verts.Count; i++)
        {
            var pos = verts[i].position;
            output += string.Format("v {0} {1} {2}\n", -pos.x, pos.y, pos.z);
        }
        Debug.LogError("Main Vert datas");

        output += "\ns off\n";
        string mainPlane = "f ";
        for (int i = 1; i <= verts.Count; i++)
        {
            mainPlane += " " + i;
        }
        output += mainPlane + "\n";

        return output;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MeshGenerator))]
public class MeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshGenerator myScript = (MeshGenerator)target;

        GUILayout.Label("");
        //GUILayout.Label("Update DigitRectTransform Size from prefab data or spritedata");

        if (GUILayout.Button("Gen Center"))
        {
            myScript.GenCenter();
        }

        GUILayout.Label("Gen Center and Border");

        if (GUILayout.Button("Gen Center and Border"))
        {
            myScript.GenAllMesh();
        }

        GUILayout.Label("");
    }
}
#endif

