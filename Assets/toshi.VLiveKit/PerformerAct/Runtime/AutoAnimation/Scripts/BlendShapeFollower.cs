using UnityEngine;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BlendShapeFollower : MonoBehaviour
{
    [Serializable]
    public class BlendShapeSet
    {
        public string controllerBlendShapeName;
        public string targetBlendShapeName;
        [Range(0f, 1f)] public float followRatio = 1f;
        [Range(0f, 1f)] public float noiseStrength = 0f;
        [Range(0.1f, 10f)] public float noiseSpeed = 1f;
        public int noiseSyncNum = 0; // 新しいフィールド
    }

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private List<BlendShapeSet> blendShapeSets = new List<BlendShapeSet>();

    private Dictionary<string, int> blendShapeIndexCache = new Dictionary<string, int>();
    private Dictionary<int, float> noiseOffsets = new Dictionary<int, float>();

    public bool HasBlendShapeSets => blendShapeSets.Count > 0;

    public void Configure(SkinnedMeshRenderer renderer, List<BlendShapeSet> sets, bool replaceExisting)
    {
        skinnedMeshRenderer = renderer;

        if (replaceExisting)
        {
            blendShapeSets.Clear();
        }

        if (sets != null)
        {
            foreach (var set in sets)
            {
                if (set == null || string.IsNullOrEmpty(set.controllerBlendShapeName) || string.IsNullOrEmpty(set.targetBlendShapeName))
                {
                    continue;
                }

                if (!ContainsBlendShapeSet(set.controllerBlendShapeName, set.targetBlendShapeName))
                {
                    blendShapeSets.Add(set);
                }
            }
        }

        InitializeBlendShapeIndices();
        InitializeNoiseOffsets();
    }

    void Start()
    {
        InitializeBlendShapeIndices();
        InitializeNoiseOffsets();
    }

    void InitializeBlendShapeIndices()
    {
        if (skinnedMeshRenderer != null)
        {
            blendShapeIndexCache.Clear();
            foreach (var set in blendShapeSets)
            {
                CacheBlendShapeIndex(set.controllerBlendShapeName);
                CacheBlendShapeIndex(set.targetBlendShapeName);
            }
        }
    }

    void CacheBlendShapeIndex(string blendShapeName)
    {
        if (string.IsNullOrEmpty(blendShapeName) || skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
        {
            return;
        }

        if (!blendShapeIndexCache.ContainsKey(blendShapeName))
        {
            int index = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
            blendShapeIndexCache[blendShapeName] = index;
            if (index == -1)
            {
                Debug.LogWarning($"BlendShape '{blendShapeName}' not found on {skinnedMeshRenderer.name}");
            }
        }
    }

    void InitializeNoiseOffsets()
    {
        noiseOffsets.Clear();
        foreach (var set in blendShapeSets)
        {
            if (!noiseOffsets.ContainsKey(set.noiseSyncNum))
            {
                noiseOffsets[set.noiseSyncNum] = UnityEngine.Random.value * 1000f;
            }
        }
    }

    void Update()
    {
        UpdateBlendShapes();
    }

    void UpdateBlendShapes()
    {
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null) return;

        foreach (var set in blendShapeSets)
        {
            if (set == null || string.IsNullOrEmpty(set.controllerBlendShapeName) || string.IsNullOrEmpty(set.targetBlendShapeName))
            {
                continue;
            }

            var controllerIndex = -1;
            if (!blendShapeIndexCache.TryGetValue(set.controllerBlendShapeName, out controllerIndex))
            {
                CacheBlendShapeIndex(set.controllerBlendShapeName);
                blendShapeIndexCache.TryGetValue(set.controllerBlendShapeName, out controllerIndex);
            }

            var targetIndex = -1;
            if (!blendShapeIndexCache.TryGetValue(set.targetBlendShapeName, out targetIndex))
            {
                CacheBlendShapeIndex(set.targetBlendShapeName);
                blendShapeIndexCache.TryGetValue(set.targetBlendShapeName, out targetIndex);
            }

            if (!noiseOffsets.ContainsKey(set.noiseSyncNum))
            {
                noiseOffsets[set.noiseSyncNum] = UnityEngine.Random.value * 1000f;
            }

            if (controllerIndex != -1 && targetIndex != -1)
            {
                float controllerValue = skinnedMeshRenderer.GetBlendShapeWeight(controllerIndex);
                float noiseValue = Mathf.PerlinNoise(Time.time * set.noiseSpeed, noiseOffsets[set.noiseSyncNum]) * 2f - 1f;
                float finalValue = controllerValue * set.followRatio + noiseValue * set.noiseStrength * controllerValue;
                finalValue = Mathf.Clamp(finalValue, 0f, 100f);
                skinnedMeshRenderer.SetBlendShapeWeight(targetIndex, finalValue);
            }
        }
    }

    void OnValidate()
    {
        InitializeBlendShapeIndices();
        InitializeNoiseOffsets();
    }

    private bool ContainsBlendShapeSet(string controllerBlendShapeName, string targetBlendShapeName)
    {
        foreach (var set in blendShapeSets)
        {
            if (set != null &&
                set.controllerBlendShapeName == controllerBlendShapeName &&
                set.targetBlendShapeName == targetBlendShapeName)
            {
                return true;
            }
        }

        return false;
    }
}
