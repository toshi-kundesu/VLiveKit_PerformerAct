using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BoneOffsetSetting
{
    public HumanBodyBones bone;

    [Header("sin波によるオフセット設定")]
    public Vector3 offsetAmplitude = new Vector3(10f, 0f, 0f);
    public float offsetSpeed = 1f;
    [Range(0f, 1f)] public float phaseOffset = 0f;

    [Header("状態確認")]
    [SerializeField] public Vector3 originalLocalEulerAngles;
    [SerializeField] public Vector3 appliedLocalEulerAngles;

    [HideInInspector] public Transform target;
    [HideInInspector] public Quaternion previousOffset = Quaternion.identity;
}

[RequireComponent(typeof(Animator))]
public class MultiBoneOffsetController : MonoBehaviour
{
    public List<BoneOffsetSetting> boneSettings = new List<BoneOffsetSetting>();

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        foreach (var setting in boneSettings)
        {
            setting.target = animator.GetBoneTransform(setting.bone);
            if (setting.target == null)
            {
                Debug.LogWarning($"{setting.bone} が見つかりませんでした。Humanoidリグですか？");
            }
        }
    }

    void LateUpdate()
    {
        float globalTime = Time.time;

        foreach (var setting in boneSettings)
        {
            if (setting.target == null) continue;

            float time = globalTime * setting.offsetSpeed + setting.phaseOffset * Mathf.PI * 2;

            Vector3 offset;
            offset.x = Mathf.Sin(time) * setting.offsetAmplitude.x;
            offset.y = Mathf.Sin(time + Mathf.PI / 2f) * setting.offsetAmplitude.y;
            offset.z = Mathf.Sin(time + Mathf.PI) * setting.offsetAmplitude.z;

            Quaternion currentOffset = Quaternion.Euler(offset);

            // 前回のオフセットを打ち消す
            setting.target.localRotation *= Quaternion.Inverse(setting.previousOffset);

            // 元の回転を記録
            setting.originalLocalEulerAngles = setting.target.localEulerAngles;

            // 新しいオフセットを適用
            setting.target.localRotation *= currentOffset;

            // 適用後の回転を記録
            setting.appliedLocalEulerAngles = setting.target.localEulerAngles;

            // 現在のオフセットを保存
            setting.previousOffset = currentOffset;
        }
    }
}
