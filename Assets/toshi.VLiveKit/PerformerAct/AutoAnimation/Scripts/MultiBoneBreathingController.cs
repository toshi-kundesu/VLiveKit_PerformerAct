using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BoneOffsetSettings
{
    public HumanBodyBones bone;
    public Vector3 offsetAmplitude = new Vector3(10f, 0f, 0f);
    [Range(0.1f, 5f)] public float frequency = 1f;
    [Range(0f, 1f)] public float phaseOffset = 0f;
    [Range(0f, 1f)] public float intensity = 1f;

    [HideInInspector] public Transform target;
    [HideInInspector] public Quaternion previousOffset = Quaternion.identity;
}

[RequireComponent(typeof(Animator))]
public class MultiBoneBreathingController : MonoBehaviour
{
    public List<BoneOffsetSettings> boneOffsets = new List<BoneOffsetSettings>();

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        foreach (var setting in boneOffsets)
        {
            if (setting.target == null)
            {
                var t = animator.GetBoneTransform(setting.bone);
                if (t == null)
                {
                    Debug.LogWarning($"ボーン {setting.bone} が見つかりませんでした。");
                }
                setting.target = t;
            }
        }
    }

    void LateUpdate()
    {
        float globalTime = Time.time;

        foreach (var setting in boneOffsets)
        {
            if (setting.target == null) continue;

            float time = globalTime * setting.frequency + setting.phaseOffset * Mathf.PI * 2f;

            float x = Mathf.Sin(time) * setting.offsetAmplitude.x * setting.intensity;
            float y = Mathf.Sin(time + Mathf.PI / 2f) * setting.offsetAmplitude.y * setting.intensity;
            float z = Mathf.Sin(time + Mathf.PI) * setting.offsetAmplitude.z * setting.intensity;

            Quaternion currentOffset = Quaternion.Euler(x, y, z);

            // オフセットを打ち消してから適用
            setting.target.localRotation *= Quaternion.Inverse(setting.previousOffset);
            setting.target.localRotation *= currentOffset;

            setting.previousOffset = currentOffset;
        }
    }
}
