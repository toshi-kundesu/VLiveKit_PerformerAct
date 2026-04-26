using UnityEngine;

[System.Serializable]
public class BlendShapeInfo
{
    public int index;
    public string name;

    public BlendShapeInfo(int index, string name)
    {
        this.index = index;
        this.name = name;
    }
}