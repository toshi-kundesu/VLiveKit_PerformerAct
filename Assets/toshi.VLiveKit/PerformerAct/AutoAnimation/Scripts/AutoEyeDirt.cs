// VLiveKit is all Unlicense.
// unlicense: https://unlicense.org/
// last update: 2025/10/26 (camera-look dedicated offset added & per-eye defaults serialized)
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AutoEyeDirt : MonoBehaviour
{
    /* ───────────── インスペクタ ───────────── */
    [Header("カメラ目線対象")]
    public Transform targetCamera;

    [Header("カメラ目線を有効にするか")]
    public bool enableCameraLook = true;

    [Header("常にカメラ目線に固定するか")]
    public bool onlyLookAtCamera = false;

    [Header("現在カメラを見ているか（表示用）")]
    [SerializeField] private bool isLookingAtCameraNow = false;

    [Header("アイダート（ランダム）設定")]
    public Vector3 offsetRange          = new Vector3(5f, 5f, 0f);
    public float   minInterval          = 0.5f;
    public float   maxInterval          = 1.5f;
    public int     dartBeforeCameraLook = 3;

    /* ───────────── 手動ボーン指定 ───────────── */
    [Header("👀 手動ボーン指定（指定があれば最優先で使用）")]
    public Transform leftEyeOverride;
    public Transform rightEyeOverride;
    public bool autoResolveOnValidate = true;

    /* ───────────── カメラ目線専用オフセット ───────────── */
    public enum CameraOffsetSpace { EyeLocal, World } // Eye座標 or ワールド座標で回す
    [Header("🎯 カメラ目線専用オフセット")]
    public bool cameraLookOffsetEnabled = true;
    public CameraOffsetSpace cameraLookOffsetSpace = CameraOffsetSpace.EyeLocal;

    [Tooltip("左右同一のオフセットを使う場合はON")]
    public bool sameOffsetBothEyes = true;

    [Tooltip("左右同一オフセット（同一使用時のみ有効）")]
    public Vector3 cameraLookOffsetEuler = Vector3.zero;

    [Tooltip("Left専用（sameOffsetBothEyes=OFF時のみ有効）")]
    public Vector3 cameraLookOffsetEulerLeft = Vector3.zero;
    [Tooltip("Right専用（sameOffsetBothEyes=OFF時のみ有効）")]
    public Vector3 cameraLookOffsetEulerRight = Vector3.zero;

    /* ───────────── 内部変数 ───────────── */
    private Animator  animator;
    private Transform leftEye;
    private Transform rightEye;

    // 左右別の初期ローカル回転をInspectorで確認できるように保持
    [Header("📌 Debug: 取得した“正面”ローカル回転(Inspector 表示用)")]
    [SerializeField] private Quaternion defaultLeftLocalRot  = Quaternion.identity;
    [SerializeField] private Quaternion defaultRightLocalRot = Quaternion.identity;

    private float      timer;
    private int        dartCount;
    private bool       useCameraLookThisTime;
    private Vector3    randomOffsetEuler;

    /* ──────────────────────────────────── */
    void Awake()
    {
        animator = GetComponent<Animator>();
        ResolveBones(verbose: true);
        if (!ValidateEyes()) return;

        CaptureDefaultEyeLocalRotations();
        ResetTimer();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!autoResolveOnValidate) return;
        if (animator == null) animator = GetComponent<Animator>();
        ResolveBones(verbose: false);
    }

    [ContextMenu("Validate Bones (Resolve Now)")]
    private void ContextValidateBones() => ResolveBones(verbose: true);

    [ContextMenu("Capture Default Eye Local Rotations")]
    private void CaptureDefaultEyeLocalRotations_Context() => CaptureDefaultEyeLocalRotations();
#endif

    /* ──────────────────────────────────── */
    void LateUpdate()
    {
        if (!ValidateEyes()) return;

        // ── 常にカメラ目線モード ──
        if (onlyLookAtCamera)
        {
            if (targetCamera == null) return;

            var lookL = Quaternion.LookRotation((targetCamera.position - leftEye.position).normalized, leftEye.up);
            var lookR = Quaternion.LookRotation((targetCamera.position - rightEye.position).normalized, rightEye.up);

            // ★ カメラ目線専用オフセット適用
            lookL = ApplyCameraLookOffset(lookL, isRight:false);
            lookR = ApplyCameraLookOffset(lookR, isRight:true);

            leftEye.rotation  = lookL;
            rightEye.rotation = lookR;
            isLookingAtCameraNow = true;
            return;
        }

        if (enableCameraLook && targetCamera == null) return;

        // タイマー処理
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            dartCount++;
            useCameraLookThisTime = enableCameraLook && (dartCount >= dartBeforeCameraLook);

            if (useCameraLookThisTime)
            {
                dartCount = 0;
            }
            else
            {
                randomOffsetEuler = new Vector3(
                    Random.Range(-offsetRange.x, offsetRange.x),
                    Random.Range(-offsetRange.y, offsetRange.y),
                    Random.Range(-offsetRange.z, offsetRange.z)
                );
            }
            ResetTimer();
        }

        if (useCameraLookThisTime)
        {
            // ── 100% カメラ目線（左右独立） ──
            var lookL = Quaternion.LookRotation((targetCamera.position - leftEye.position).normalized, leftEye.up);
            var lookR = Quaternion.LookRotation((targetCamera.position - rightEye.position).normalized, rightEye.up);

            // ★ カメラ目線専用オフセット適用
            lookL = ApplyCameraLookOffset(lookL, isRight:false);
            lookR = ApplyCameraLookOffset(lookR, isRight:true);

            leftEye.rotation  = lookL;
            rightEye.rotation = lookR;
            isLookingAtCameraNow = true;
        }
        else
        {
            // ── アイダート（カメラ基準 or デフォルト基準） ──
            Quaternion baseL, baseR;

            if (enableCameraLook)
            {
                baseL = Quaternion.LookRotation((targetCamera.position - leftEye.position).normalized, leftEye.up);
                baseR = Quaternion.LookRotation((targetCamera.position - rightEye.position).normalized, rightEye.up);

                // ★ カメラ目線“基準”にも専用オフセットを足す（その上でランダムをEyeLocalで加算）
                baseL = ApplyCameraLookOffset(baseL, isRight:false);
                baseR = ApplyCameraLookOffset(baseR, isRight:true);
            }
            else
            {
                // 親ワールド回転 × 初期ローカル（左右別）
                baseL = leftEye.parent.rotation  * defaultLeftLocalRot;
                baseR = rightEye.parent.rotation * defaultRightLocalRot;
            }

            var dartQ = Quaternion.Euler(randomOffsetEuler); // EyeLocal基準
            leftEye.rotation  = baseL * dartQ;
            rightEye.rotation = baseR * dartQ;

            isLookingAtCameraNow = false;
        }
    }

    /* ──────────────────────────────────── */
    void ResetTimer() => timer = Random.Range(minInterval, maxInterval);

    /* ───────────── ユーティリティ ───────────── */
    private void CaptureDefaultEyeLocalRotations()
    {
        if (leftEye  != null) defaultLeftLocalRot  = leftEye.localRotation;
        if (rightEye != null) defaultRightLocalRot = rightEye.localRotation;
    }

    private Quaternion ApplyCameraLookOffset(Quaternion lookRot, bool isRight)
    {
        if (!cameraLookOffsetEnabled) return lookRot;

        Vector3 euler = sameOffsetBothEyes
            ? cameraLookOffsetEuler
            : (isRight ? cameraLookOffsetEulerRight : cameraLookOffsetEulerLeft);

        var q = Quaternion.Euler(euler);

        // World: 先に世界回転で回してから視線（=q * look）
        // EyeLocal: 視線の後にローカルで回す（=look * q）
        return (cameraLookOffsetSpace == CameraOffsetSpace.World) ? (q * lookRot) : (lookRot * q);
    }

    private void ResolveBones(bool verbose)
    {
        // 手動優先
        if (leftEyeOverride != null && rightEyeOverride != null)
        {
            leftEye  = leftEyeOverride;
            rightEye = rightEyeOverride;
            if (verbose) Debug.Log($"[AutoEyeDirt] Use MANUAL eye bones: L={leftEye.name}, R={rightEye.name}", this);
            return;
        }

        if ((leftEyeOverride != null) ^ (rightEyeOverride != null))
        {
            if (verbose)
                Debug.LogWarning("[AutoEyeDirt] 片方のみ手動指定。両方指定するか、両方未指定にしてください。自動解決も試みます。", this);
        }

        leftEye = rightEye = null;

        if (animator != null && animator.isHuman && animator.avatar != null && animator.avatar.isValid)
        {
            leftEye  = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);

            if (leftEye != null && rightEye != null)
            {
                if (verbose) Debug.Log($"[AutoEyeDirt] Use HUMANOID eye bones: L={leftEye.name}, R={rightEye.name}", this);
                return;
            }
        }

        if (verbose)
            Debug.LogWarning("[AutoEyeDirt] 目ボーンが見つかりません。Humanoid の Eye が無いか、手動で両目を指定してください。", this);
    }

    private bool ValidateEyes()
    {
        if (leftEye == null || rightEye == null)
        {
            ResolveBones(verbose:false);
            if (leftEye == null || rightEye == null) return false;
        }
        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (leftEye != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(leftEye.position, leftEye.forward * 0.2f);
        }
        if (rightEye != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rightEye.position, rightEye.forward * 0.2f);
        }
    }
#endif
}
