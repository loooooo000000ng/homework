using UnityEngine;

public class FFDGrid
{
    public int nx, ny, nz;
    public Vector3[,,] ctrlPoints;
    public Bounds bound;

    public FFDGrid(int nx_, int ny_, int nz_, Bounds modelBound)
    {
        nx = nx_;
        ny = ny_;
        nz = nz_;
        bound = modelBound;
        ctrlPoints = new Vector3[nx, ny, nz];

        // 初始化均匀网格控制点
        for (int i = 0; i < nx; i++)
        {
            for (int j = 0; j < ny; j++)
            {
                for (int k = 0; k < nz; k++)
                {
                    float u = i / (float)(nx - 1);
                    float v = j / (float)(ny - 1);
                    float w = k / (float)(nz - 1);
                    ctrlPoints[i, j, k] = LerpBound(u, v, w);
                }
            }
        }
    }

    private Vector3 LerpBound(float u, float v, float w)
    {
        Vector3 min = bound.min;
        Vector3 max = bound.max;
        return new Vector3(
            Mathf.Lerp(min.x, max.x, u),
            Mathf.Lerp(min.y, max.y, v),
            Mathf.Lerp(min.z, max.z, w)
        );
    }

    // 对一个原始世界空间顶点做FFD插值变形
    public Vector3 Eval(Vector3 p)
    {
        Vector3 min = bound.min;
        Vector3 max = bound.max;
        float u = Mathf.InverseLerp(min.x, max.x, p.x);
        float v = Mathf.InverseLerp(min.y, max.y, p.y);
        float w = Mathf.InverseLerp(min.z, max.z, p.z);

        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);
        w = Mathf.Clamp01(w);

        float iu = u * (nx - 1);
        float iv = v * (ny - 1);
        float iw = w * (nz - 1);

        int i0 = Mathf.FloorToInt(iu);
        int j0 = Mathf.FloorToInt(iv);
        int k0 = Mathf.FloorToInt(iw);
        int i1 = Mathf.Min(i0 + 1, nx - 1);
        int j1 = Mathf.Min(j0 + 1, ny - 1);
        int k1 = Mathf.Min(k0 + 1, nz - 1);

        float fu = iu - i0;
        float fv = iv - j0;
        float fw = iw - k0;

        Vector3 c000 = ctrlPoints[i0, j0, k0];
        Vector3 c100 = ctrlPoints[i1, j0, k0];
        Vector3 c010 = ctrlPoints[i0, j1, k0];
        Vector3 c110 = ctrlPoints[i1, j1, k0];
        Vector3 c001 = ctrlPoints[i0, j0, k1];
        Vector3 c101 = ctrlPoints[i1, j0, k1];
        Vector3 c011 = ctrlPoints[i0, j1, k1];
        Vector3 c111 = ctrlPoints[i1, j1, k1];

        // 三线性插值
        Vector3 c00 = Vector3.Lerp(c000, c100, fu);
        Vector3 c10 = Vector3.Lerp(c010, c110, fu);
        Vector3 c01 = Vector3.Lerp(c001, c101, fu);
        Vector3 c11 = Vector3.Lerp(c011, c111, fu);

        Vector3 c0 = Vector3.Lerp(c00, c10, fv);
        Vector3 c1 = Vector3.Lerp(c01, c11, fv);

        return Vector3.Lerp(c0, c1, fw);
    }

    // 随机扰动控制点，演示FFD变形效果（你也可以写拖拽控制点）
    public void RandomPerturb(float strength)
    {
        for (int i = 0; i < nx; i++)
            for (int j = 0; j < ny; j++)
                for (int k = 0; k < nz; k++)
                {
                    ctrlPoints[i, j, k] += Random.insideUnitSphere * strength;
                }
    }

    public void ResetControlPoints()
    {
        for (int i = 0; i < nx; i++)
            for (int j = 0; j < ny; j++)
                for (int k = 0; k < nz; k++)
                {
                    float u = i / (float)(nx - 1);
                    float v = j / (float)(ny - 1);
                    float w = k / (float)(nz - 1);
                    ctrlPoints[i, j, k] = LerpBound(u, v, w);
                }
    }
}
