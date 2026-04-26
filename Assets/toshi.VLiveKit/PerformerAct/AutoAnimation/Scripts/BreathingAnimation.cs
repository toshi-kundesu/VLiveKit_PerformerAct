using UnityEngine;
using System.Collections.Generic;
using VRM;

[System.Serializable]
public class BoneSettings {
    public bool useAnimator = false;
    public HumanBodyBones humanoidBone;

    public Transform bone;
    [Range(0f, 10f)] public float angleAmplitudeX = 5f;
    [Range(0f, 10f)] public float angleAmplitudeY = 0f;
    [Range(0f, 10f)] public float angleAmplitudeZ = 0f;
    [Range(0f, 1f)] public float phaseOffset = 0f;

    [HideInInspector] public Quaternion previousOffsetRotation = Quaternion.identity;
}

[System.Serializable]
public class BlendShapeSettings {
    public bool useVRM = false;
    public string shapeName;
    public BlendShapePreset vrmPreset = BlendShapePreset.Unknown;

    [Range(0f, 100f)] public float amplitude = 50f;
    [Range(0f, 1f)] public float phaseOffset = 0.5f;

    [HideInInspector] public int shapeIndex;
}

[DefaultExecutionOrder(-100)]
public class BreathingAnimation : MonoBehaviour {
    public List<BoneSettings> bones = new List<BoneSettings>();

    [Header("BlendShape Settings")]
    public SkinnedMeshRenderer faceMesh;
    public VRMBlendShapeProxy vrmBlendShapeProxy;
    public List<BlendShapeSettings> blendShapes = new List<BlendShapeSettings>();

    [Header("Animator & General Settings")]
    public Animator animator;
    [Range(0.1f, 5f)]
    public float frequency = 1f;

    [Header("Bone Animation Master Intensity")]
    [Range(0f, 10f)]
    public float boneIntensity = 1f;

    void LateUpdate() {
        float time = Time.time * frequency;

        foreach (var bone in bones) {
            if (bone.useAnimator && bone.bone == null && animator != null) {
                var resolved = animator.GetBoneTransform(bone.humanoidBone);
                if (resolved != null) {
                    bone.bone = resolved;
                }
            }

            if (bone.bone != null) {
                // 前の呼吸オフセットを打ち消す
                bone.bone.localRotation *= Quaternion.Inverse(bone.previousOffsetRotation);

                // 新しい呼吸回転オフセットを計算（Intensity反映）
                float angleX = bone.angleAmplitudeX * boneIntensity * Mathf.Sin(time + bone.phaseOffset * Mathf.PI * 2);
                float angleY = bone.angleAmplitudeY * boneIntensity * Mathf.Sin(time + bone.phaseOffset * Mathf.PI * 2);
                float angleZ = bone.angleAmplitudeZ * boneIntensity * Mathf.Sin(time + bone.phaseOffset * Mathf.PI * 2);
                Quaternion currentOffset = Quaternion.Euler(angleX, angleY, angleZ);

                bone.bone.localRotation *= currentOffset;
                bone.previousOffsetRotation = currentOffset;
            }
        }

        // BlendShapeはそのまま（必要なら同様に masterBlendShapeIntensity を追加可）
    }
}
