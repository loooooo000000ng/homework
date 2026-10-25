using UnityEngine;
using UnityEngine.UI;

public enum DeformMode
{
    None,
    Taper,
    Twist,
    Bend,
    FFD
}

public class MeshDeformManager : MonoBehaviour
{
    public Button btnTaper, btnTwist, btnBend, btnFFD, btnReset;
    public Slider sliderParam;
    public Text txtMode;
    public string objPath = "Assets/StreamingAssets/test.obj";

    private Mesh targetMesh;
    private Vector3[] originalVerts;
    private MeshFilter mf;
    private Renderer rd;
    private FFDGrid ffd;
    private DeformMode currentMode;
    private Bounds meshBound;

    void Start()
    {
        // 创建模型物体
        GameObject go = new GameObject("DeformModel");
        mf = go.AddComponent<MeshFilter>();
        rd = go.AddComponent<MeshRenderer>();
        rd.material = new Material(Shader.Find("Standard"));

        LoadMesh();

        btnTaper.onClick.AddListener(() => SetMode(DeformMode.Taper));
        btnTwist.onClick.AddListener(() => SetMode(DeformMode.Twist));
        btnBend.onClick.AddListener(() => SetMode(DeformMode.Bend));
        btnFFD.onClick.AddListener(() => SetMode(DeformMode.FFD));
        btnReset.onClick.AddListener(ResetMesh);
        sliderParam.onValueChanged.AddListener(v => DeformMesh(v));
    }

    void LoadMesh()
    {
        targetMesh = ObjLoader.LoadObj(objPath);
        mf.mesh = targetMesh;
        originalVerts = targetMesh.vertices;
        meshBound = targetMesh.bounds;
        // FFD网格：4×4×4控制点，可自行修改nx,ny,nz
        ffd = new FFDGrid(4,4,4, meshBound);
    }

    void SetMode(DeformMode m)
    {
        currentMode = m;
        txtMode.text = "当前模式：" + m.ToString();
        DeformMesh(sliderParam.value);
    }

    void DeformMesh(float param)
    {
        if (targetMesh == null || originalVerts == null) return;
        Vector3[] newVerts = new Vector3[originalVerts.Length];
        float minY = meshBound.min.y;
        float maxY = meshBound.max.y;

        for (int i = 0; i < originalVerts.Length; i++)
        {
            Vector3 p = originalVerts[i];
            switch (currentMode)
            {
                case DeformMode.Taper:
                    newVerts[i] = DeformTypes.Taper(p, minY, maxY, param);
                    break;
                case DeformMode.Twist:
                    newVerts[i] = DeformTypes.Twist(p, minY, maxY, param * Mathf.PI);
                    break;
                case DeformMode.Bend:
                    newVerts[i] = DeformTypes.Bend(p, minY, maxY, param * Mathf.PI);
                    break;
                case DeformMode.FFD:
                    // param用来扰动控制点；每次扰动后插值
                    ffd.ResetControlPoints();
                    ffd.RandomPerturb(param * 0.4f);
                    newVerts[i] = ffd.Eval(p);
                    break;
                default:
                    newVerts[i] = p;
                    break;
            }
        }
        targetMesh.vertices = newVerts;
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateBounds();
    }

    void ResetMesh()
    {
        currentMode = DeformMode.None;
        txtMode.text = "当前模式：None";
        sliderParam.value = 0;
        Vector3[] newVerts = (Vector3[])originalVerts.Clone();
        targetMesh.vertices = newVerts;
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateBounds();
        ffd.ResetControlPoints();
    }

    // 可选：Gizmos绘制FFD控制点网格，Scene窗口看控制点
    void OnDrawGizmosSelected()
    {
        if (ffd == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < ffd.nx; i++)
            for (int j = 0; j < ffd.ny; j++)
                for (int k = 0; k < ffd.nz; k++)
                {
                    Gizmos.DrawSphere(ffd.ctrlPoints[i, j, k], 0.04f);
                }
    }
}
