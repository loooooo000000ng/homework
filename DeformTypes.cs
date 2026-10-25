using UnityEngine;

public static class DeformTypes
{
    // 锥化 Taper：沿Y轴，上下缩放，taperFactor>0上粗下细，<0上细下粗
    public static Vector3 Taper(Vector3 p, float minY, float maxY, float taperFactor)
    {
        float t = Mathf.InverseLerp(minY, maxY, p.y);
        float scale = 1 + taperFactor * t;
        scale = Mathf.Max(0.01f, scale);
        return new Vector3(p.x * scale, p.y, p.z * scale);
    }

    // 扭曲 Twist：沿Y轴旋转，twistAngle 总扭曲角度(弧度)
    public static Vector3 Twist(Vector3 p, float minY, float maxY, float twistAngle)
    {
        float t = Mathf.InverseLerp(minY, maxY, p.y);
        float ang = twistAngle * t;
        float c = Mathf.Cos(ang);
        float s = Mathf.Sin(ang);
        float x = p.x * c - p.z * s;
        float z = p.x * s + p.z * c;
        return new Vector3(x, p.y, z);
    }

    // 弯曲 Bend：沿X方向弯曲，bendAngle总弯曲弧度，以Y为高度轴
    public static Vector3 Bend(Vector3 p, float minY, float maxY, float bendAngle)
    {
        float t = Mathf.InverseLerp(minY, maxY, p.y);
        float ang = bendAngle * t;
        float r = p.x;
        float newX = r * Mathf.Cos(ang);
        float newZ = r * Mathf.Sin(ang);
        return new Vector3(newX, p.y, newZ + p.z);
    }
}
