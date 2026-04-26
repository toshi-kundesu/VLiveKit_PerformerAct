using UnityEngine;
using VRM;

[RequireComponent(typeof(FacialReceiver))]
public class FacialBlendShapeController : MonoBehaviour
{
    public enum TargetType
    {
        SkinnedMeshRenderer,
        VRM
    }

    [System.Serializable]
    public class BlendShapeMapping
    {
        public string vowel = "A";
        public string blendShapeName;        // For SkinnedMeshRenderer
        public BlendShapePreset preset;      // For VRM
        [Range(0, 100)]
        public float multiplier = 1.0f;
    }

    [Header("Target Settings")]
    public TargetType targetType;
    public SkinnedMeshRenderer targetMesh;
    public VRMBlendShapeProxy blendShapeProxy;

    [Header("Blend Shape Mappings")]
    public BlendShapeMapping[] mappings = new BlendShapeMapping[]
    {
        new BlendShapeMapping { vowel = "A", blendShapeName = "A", preset = BlendShapePreset.A },
        new BlendShapeMapping { vowel = "I", blendShapeName = "I", preset = BlendShapePreset.I },
        new BlendShapeMapping { vowel = "U", blendShapeName = "U", preset = BlendShapePreset.U },
        new BlendShapeMapping { vowel = "E", blendShapeName = "E", preset = BlendShapePreset.E },
        new BlendShapeMapping { vowel = "O", blendShapeName = "O", preset = BlendShapePreset.O }
    };

    [Header("Debug Values")]
    [SerializeField, Range(0, 100)] private float currentA;
    [SerializeField, Range(0, 100)] private float currentI;
    [SerializeField, Range(0, 100)] private float currentU;
    [SerializeField, Range(0, 100)] private float currentE;
    [SerializeField, Range(0, 100)] private float currentO;

    private FacialReceiver receiver;
    private int[] blendShapeIndices;

    void Start()
    {
        receiver = GetComponent<FacialReceiver>();
        if (targetType == TargetType.SkinnedMeshRenderer)
        {
            InitializeBlendShapeIndices();
        }
    }

    void InitializeBlendShapeIndices()
    {
        if (!targetMesh) return;

        blendShapeIndices = new int[mappings.Length];
        for (int i = 0; i < mappings.Length; i++)
        {
            blendShapeIndices[i] = targetMesh.sharedMesh.GetBlendShapeIndex(mappings[i].blendShapeName);
        }
    }

    void Update()
    {
        // デバッグ値の更新
        if (receiver != null)
        {
            currentA = receiver.valueA;
            currentI = receiver.valueI;
            currentU = receiver.valueU;
            currentE = receiver.valueE;
            currentO = receiver.valueO;
        }

        switch (targetType)
        {
            case TargetType.SkinnedMeshRenderer:
                UpdateSkinnedMeshRenderer();
                break;
            case TargetType.VRM:
                UpdateVRM();
                break;
        }
    }

    void UpdateSkinnedMeshRenderer()
    {
        if (!targetMesh) return;

        for (int i = 0; i < mappings.Length; i++)
        {
            if (blendShapeIndices[i] < 0) continue;

            float value = GetVowelValue(mappings[i].vowel);
            targetMesh.SetBlendShapeWeight(blendShapeIndices[i], value * mappings[i].multiplier);
        }
    }

    void UpdateVRM()
    {
        if (!blendShapeProxy) return;

        foreach (var mapping in mappings)
        {
            float value = GetVowelValue(mapping.vowel);
            blendShapeProxy.ImmediatelySetValue(mapping.preset, value * mapping.multiplier * 0.01f);
        }
    }

    float GetVowelValue(string vowel)
    {
        switch (vowel)
        {
            case "A": return receiver.valueA;
            case "I": return receiver.valueI;
            case "U": return receiver.valueU;
            case "E": return receiver.valueE;
            case "O": return receiver.valueO;
            default: return 0f;
        }
    }

    // インスペクターでの表示を改善
    void OnValidate()
    {
        // ターゲットタイプに応じて不要なフィールドをnullに
        if (targetType == TargetType.SkinnedMeshRenderer)
        {
            blendShapeProxy = null;
        }
        else
        {
            targetMesh = null;
        }
    }
}