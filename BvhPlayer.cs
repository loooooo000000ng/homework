using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class BvhPlayer : MonoBehaviour
{
    public string bvhFileName = "test.bvh";
    public float scale = 0.01f;

    private BvhData bvhData;
    private Dictionary<string, Transform> boneDict;
    private float frameTime;
    private int currentFrame;
    private float timer;

    void Start()
    {
        boneDict = new Dictionary<string, Transform>();
        string filePath = Path.Combine(Application.streamingAssetsPath, bvhFileName);
        if (!File.Exists(filePath))
        {
            Debug.LogError("找不到BVH文件：" + filePath);
            return;
        }
        string content = File.ReadAllText(filePath);
        bvhData = BvhParser.Parse(content);
        if (bvhData == null)
        {
            Debug.LogError("BVH解析失败");
            return;
        }
        BuildSkeleton(bvhData.root, null);
        frameTime = bvhData.frameTime;
        currentFrame = 0;
        timer = 0;
    }

    void Update()
    {
        if (bvhData == null) return;
        timer += Time.deltaTime;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            ApplyFrame(bvhData.frames[currentFrame]);
            currentFrame = (currentFrame + 1) % bvhData.frameCount;
        }
    }

    void BuildSkeleton(BvhJoint joint, Transform parent)
    {
        // End Site 不创建物体
        if (joint.isEndSite) return;

        GameObject go = new GameObject(joint.name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = joint.offset * scale;
        boneDict[joint.name] = go.transform;

        foreach (var child in joint.children)
        {
            BuildSkeleton(child, go.transform);
        }
    }

    void ApplyFrame(float[] data)
    {
        int ptr = 0;
        ApplyJoint(bvhData.root, ref ptr, data);
    }

    void ApplyJoint(BvhJoint joint, ref int ptr, float[] data)
    {
        if (joint.isEndSite) return;

        if (boneDict.TryGetValue(joint.name, out var t))
        {
            Vector3 pos = t.localPosition;
            Vector3 rot = Vector3.zero;
            for (int i = 0; i < joint.channelCount; i++)
            {
                float v = data[ptr++];
                switch (joint.channels[i])
                {
                    case BvhChannel.Xposition: pos.x = v * scale; break;
                    case BvhChannel.Yposition: pos.y = v * scale; break;
                    case BvhChannel.Zposition: pos.z = v * scale; break;
                    case BvhChannel.Xrotation: rot.x = v; break;
                    case BvhChannel.Yrotation: rot.y = v; break;
                    case BvhChannel.Zrotation: rot.z = v; break;
                }
            }
            t.localPosition = pos;
            t.localEulerAngles = rot;
        }

        foreach (var c in joint.children)
        {
            ApplyJoint(c, ref ptr, data);
        }
    }
}

public enum BvhChannel
{
    Xposition, Yposition, Zposition,
    Xrotation, Yrotation, Zrotation
}

public class BvhJoint
{
    public string name;
    public Vector3 offset;
    public List<BvhChannel> channels;
    public int channelCount => channels.Count;
    public List<BvhJoint> children;
    public bool isEndSite;

    public BvhJoint()
    {
        channels = new List<BvhChannel>();
        children = new List<BvhJoint>();
    }
}

public class BvhData
{
    public BvhJoint root;
    public int frameCount;
    public float frameTime;
    public List<float[]> frames;
    public BvhData()
    {
        frames = new List<float[]>();
    }
}

public static class BvhParser
{
    public static BvhData Parse(string text)
    {
        var tokens = text.Split(new[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        int idx = 0;
        if (tokens[idx++] != "HIERARCHY") return null;
        var root = ReadJoint(tokens, ref idx);
        if (tokens[idx++] != "MOTION") return null;
        if (!tokens[idx++].StartsWith("Frames:")) return null;
        int frameNum = int.Parse(tokens[idx++]);
        if (tokens[idx++] != "Frame") return null;
        if (tokens[idx++] != "Time:") return null;
        float ft = float.Parse(tokens[idx++]);
        var data = new BvhData { root = root, frameCount = frameNum, frameTime = ft };
        int totalChannel = CountChannels(root);
        for (int f = 0; f < frameNum; f++)
        {
            float[] frame = new float[totalChannel];
            for (int i = 0; i < totalChannel; i++)
                frame[i] = float.Parse(tokens[idx++]);
            data.frames.Add(frame);
        }
        return data;
    }

    static BvhJoint ReadJoint(string[] tokens, ref int idx)
    {
        string type = tokens[idx++];
        string name = tokens[idx++];
        var j = new BvhJoint
        {
            name = name,
            isEndSite = type == "End"
        };
        if (tokens[idx++] != "{") return null;

        while (tokens[idx] != "}")
        {
            if (tokens[idx] == "OFFSET")
            {
                idx++;
                j.offset = new Vector3(
                    float.Parse(tokens[idx++]),
                    float.Parse(tokens[idx++]),
                    float.Parse(tokens[idx++])
                );
            }
            else if (tokens[idx] == "CHANNELS")
            {
                idx++;
                int n = int.Parse(tokens[idx++]);
                for (int i = 0; i < n; i++)
                {
                    string ch = tokens[idx++];
                    if (System.Enum.TryParse<BvhChannel>(ch, out var c))
                        j.channels.Add(c);
                }
            }
            else if (tokens[idx] == "JOINT" || tokens[idx] == "End")
            {
                j.children.Add(ReadJoint(tokens, ref idx));
            }
            else
            {
                idx++;
            }
        }
        idx++;
        return j;
    }

    static int CountChannels(BvhJoint j)
    {
        if (j.isEndSite) return 0;
        int sum = j.channelCount;
        foreach (var c in j.children) sum += CountChannels(c);
        return sum;
    }
}
