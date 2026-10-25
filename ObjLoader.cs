using System.IO;
using UnityEngine;

public static class ObjLoader
{
    public static Mesh LoadObj(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("OBJ不存在: " + filePath);
            return null;
        }
        string[] lines = File.ReadAllLines(filePath);
        System.Collections.Generic.List<Vector3> verts = new();
        System.Collections.Generic.List<int> tris = new();

        foreach (var line in lines)
        {
            var parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            if (parts[0] == "v")
            {
                float x = float.Parse(parts[1]);
                float y = float.Parse(parts[2]);
                float z = float.Parse(parts[3]);
                verts.Add(new Vector3(x, y, z));
            }
            else if (parts[0] == "f")
            {
                for (int i = 1; i <= 3; i++)
                {
                    string[] idxSplit = parts[i].Split('/');
                    int vid = int.Parse(idxSplit[0]) - 1;
                    tris.Add(vid);
                }
            }
        }
        Mesh m = new Mesh();
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
