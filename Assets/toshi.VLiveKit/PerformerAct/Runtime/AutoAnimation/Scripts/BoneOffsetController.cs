using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class BoneOffsetController : MonoBehaviour
{
    [System.Serializable]
    public class BoneOffsetSetting
    {
        public HumanBodyBones bone;
        public Vector3 offsetAmplitude = new Vector3(10f, 0f, 0f);
        public float offsetSpeed = 1f;

        [HideInInspector] public Vector3 originalLocalEulerAngles;
        [HideInInspector] public Vector3 appliedLocalEulerAngles;
        [HideInInspector] public Transform transform;
        [HideInInspector] public Vector3 offset;
    }

    [Header("対象ボーン設定")]
    public List<BoneOffsetSetting> boneOffsets = new List<BoneOffsetSetting>();

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        foreach (var boneSetting in boneOffsets)
        {
            boneSetting.transform = animator.GetBoneTransform(boneSetting.bone);
            if (boneSetting.transform == null)
            {
                Debug.LogWarning($"指定されたボーン {boneSetting.bone} が見つかりませんでした。Humanoidリグですか？");
            }
        }
    }

    void LateUpdate()
    {
        foreach (var boneSetting in boneOffsets)
        {
            if (boneSetting.transform == null) continue;

            float time = Time.time * boneSetting.offsetSpeed;

            boneSetting.offset.x = Mathf.Sin(time) * boneSetting.offsetAmplitude.x;
            boneSetting.offset.y = Mathf.Sin(time + Mathf.PI / 2) * boneSetting.offsetAmplitude.y;
            boneSetting.offset.z = Mathf.Sin(time + Mathf.PI) * boneSetting.offsetAmplitude.z;

            boneSetting.originalLocalEulerAngles = boneSetting.transform.localEulerAngles;

            Quaternion offsetRotation = Quaternion.Euler(boneSetting.offset);
            boneSetting.transform.localRotation *= offsetRotation;

            boneSetting.appliedLocalEulerAngles = boneSetting.transform.localEulerAngles;
        }
    }
}
