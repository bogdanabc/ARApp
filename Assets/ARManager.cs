using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ARManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Загрузка начата");
        string path = Path.Combine(Application.persistentDataPath, "ExtractedFiles");
        string[] files = Directory.GetFiles(path);
        foreach(string filePath in files)
        {
            StartCoroutine(LoadOBJ(filePath));
        }
        Debug.Log("Загрузка закончена");
    }
    private IEnumerator LoadOBJ(string path)
    {
        Debug.Log("Начало загрузки");
#if UNITY_ANDROID
        if (path.StartsWith("content://"))
        {
            UnityWebRequest www = UnityWebRequest.Get(path);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка загрузки OBJ: " + www.error);
            }
            else
            {
                string objText = www.downloadHandler.text;
                OBJParser(objText, Path.GetFileNameWithoutExtension(path));
            }
        }
        else
        {
            string objText = File.ReadAllText(path);
            OBJParser(objText, Path.GetFileNameWithoutExtension(path));
        }
#else
        string objText = System.IO.File.ReadAllText(path);
        OBJParser(objText, Path.GetFileNameWithoutExtension(path));
#endif
    }

    private void OBJParser(string objText, string objName)
    {
        float minz = 0f;
        List<Vector3> originalVertices = new List<Vector3>();
        List<Vector3> originalNormals = new List<Vector3>();
        List<Vector3> finalVertices = new List<Vector3>();
        List<Vector3> finalNormals = new List<Vector3>();
        List<int> finalTriangles = new List<int>();
        string[] lines = objText.Split('\n');
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.StartsWith("v "))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                if (minz > z)
                {
                    minz = z;
                }
                originalVertices.Add(new Vector3(x, y, z));
            }
            else if (line.StartsWith("vn "))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                originalNormals.Add(new Vector3(x, y, z));
            }
            else if (line.StartsWith("f "))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 1; i < parts.Length - 2; i++)
                {
                    ParseFace(parts[1], originalVertices, originalNormals, finalVertices, finalNormals, finalTriangles);
                    ParseFace(parts[i + 1], originalVertices, originalNormals, finalVertices, finalNormals, finalTriangles);
                    ParseFace(parts[i + 2], originalVertices, originalNormals, finalVertices, finalNormals, finalTriangles);
                }
            }
        }
    Mesh mesh = new Mesh();
    mesh.SetVertices(finalVertices);
    mesh.SetNormals(finalNormals);
    mesh.SetTriangles(finalTriangles, 0);
    GameObject obj = new GameObject(objName);
    MeshFilter mf = obj.AddComponent<MeshFilter>();
    MeshRenderer mr = obj.AddComponent<MeshRenderer>();
    mf.mesh = mesh;
    mr.material = new Material(Shader.Find("Standard"));
    obj.transform.position = new Vector3(0, 0, minz*0.003f);
    MarkerManager(Convert.ToUInt64(objName));
    }
    private void ParseFace(string faceData, List<Vector3> originalVertices, List<Vector3> originalNormals, List<Vector3> finalVertices, List<Vector3> finalNormals, List<int> finalTriangles)
    {
        string[] indices = faceData.Split('/');

        int vertexIndex = int.Parse(indices[0]) - 1;
        int normalIndex = indices.Length > 2 && !string.IsNullOrEmpty(indices[2]) ? int.Parse(indices[2]) - 1 : -1;

        finalVertices.Add(originalVertices[vertexIndex]);
        finalNormals.Add(normalIndex >= 0 ? originalNormals[normalIndex] : Vector3.up); // если нормаль отсутствует, ставим вверх
        finalTriangles.Add(finalVertices.Count - 1);
    }
    void MarkerManager(ulong BarecodeID)
    {
        float a = 0.003f;
        ARXTrackable trackable = GameObject.Find("ARToolKit").AddComponent<ARXTrackable>();
        trackable.ConfigureAsSquareBarcode(BarecodeID, 0.08f);
        trackable.Tag = "t" + BarecodeID.ToString();
        ARXOrigin root = FindObjectOfType<ARXOrigin>();
        GameObject go = new GameObject("TrackedObject");
        
        go.transform.parent = root.transform;
        ARXTrackedObject to = go.AddComponent<ARXTrackedObject>();
        to.TrackableTag = trackable.Tag;
        GameObject obj = GameObject.Find(BarecodeID.ToString());
        obj.transform.localScale = new Vector3(a, a, a);
        obj.transform.Rotate(0f, 180f, 0f);
        obj.transform.parent = go.transform;
        obj.layer = 8;
        go.layer = 8;
        foreach (Transform child in obj.transform)
        {
            child.gameObject.layer = 8;
        }
    }
    void OnApplicationQuit()
    {
        string path = Application.persistentDataPath;
        if (Directory.Exists(path))
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                Debug.Log("PersistentDataPath очищен.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Ошибка при очистке папки: " + e.Message);
            }
        }
    }
}

