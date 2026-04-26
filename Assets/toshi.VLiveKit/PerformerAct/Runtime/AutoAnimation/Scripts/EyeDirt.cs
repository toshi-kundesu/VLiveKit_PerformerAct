using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Animator))]
public class EyeDirt : MonoBehaviour
{
    [System.Serializable]
    public class EyeOffsetSetting
    {
        public HumanBodyBones bone;
        public Vector3 offsetMin = Vector3.zero;
        public Vector3 offsetMax = new Vector3(10f, 10f, 10f);
        public Vector3 staticOffset = Vector3.zero;
        public float intensityMultiplier = 1.0f;

        [HideInInspector] public Transform transform;
        [HideInInspector] public Quaternion defaultRotation;
        [HideInInspector] public Quaternion targetRotation;
    }

    [Header("Animator")]
    public Animator animator;

    [Header("対象ボーン設定")]
    public List<EyeOffsetSetting> eyes = new List<EyeOffsetSetting>()
    {
        new EyeOffsetSetting() { bone = HumanBodyBones.LeftEye },
        new EyeOffsetSetting() { bone = HumanBodyBones.RightEye }
    };

    [Header("タイミング設定")]
    public float minTime = 0.5f;
    public float maxTime = 2.0f;

    [Header("カメラ注視")]
    public Transform cameraTransform;
    public bool lookAtCamera = false;
    public bool autoSwitchState = true;
    public bool invertLookAt = false;
    [Range(0f, 1f)] public float cameraLookIntensity = 0.7f;
    public float cameraLookProbability = 0.5f;
    public Vector3 lookAtOffset = Vector3.zero;

    private float timer;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

        foreach (var eye in eyes)
        {
            eye.transform = animator.GetBoneTransform(eye.bone);
            if (eye.transform != null)
            {
                eye.defaultRotation = eye.transform.localRotation;
            }
        }

        ResetTimer();
    }

    void LateUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ResetTimer();

            if (autoSwitchState)
                lookAtCamera = Random.value < cameraLookProbability;

            foreach (var eye in eyes)
            {
                if (eye.transform == null) continue;

                Vector3 randOffset = new Vector3(
                    Random.Range(eye.offsetMin.x, eye.offsetMax.x),
                    Random.Range(eye.offsetMin.y, eye.offsetMax.y),
                    Random.Range(eye.offsetMin.z, eye.offsetMax.z)
                ) * eye.intensityMultiplier;

                Quaternion noiseRot = Quaternion.Euler(randOffset);
                Quaternion staticOffsetRot = Quaternion.Euler(eye.staticOffset);

                if (!lookAtCamera || cameraTransform == null)
                {
                    eye.targetRotation = eye.defaultRotation * staticOffsetRot * noiseRot;
                }
                else
                {
                    Vector3 dir = cameraTransform.position - eye.transform.position;
                    if (invertLookAt) dir = -dir;
                    if (dir.sqrMagnitude > 0.0001f)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(dir.normalized);
                        Quaternion offsetRot = Quaternion.Euler(lookAtOffset);
                        eye.targetRotation = Quaternion.Slerp(
                            eye.defaultRotation,
                            lookRot * offsetRot * noiseRot,
                            cameraLookIntensity
                        );
                    }
                }

                eye.transform.localRotation = eye.targetRotation;
            }
        }
    }

    private void ResetTimer()
    {
        timer = Random.Range(minTime, maxTime);
    }
}
