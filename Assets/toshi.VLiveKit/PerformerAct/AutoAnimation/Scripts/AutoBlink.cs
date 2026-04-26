using UnityEngine;
using VRM;

public class AutoBlink : MonoBehaviour
{
    public enum BlinkMode
    {
        SkinnedMeshRenderer,
        VRMBlendShapeProxy
    }

    [Header("Blink Target")]
    public BlinkMode mode = BlinkMode.SkinnedMeshRenderer;
    public SkinnedMeshRenderer ref_SMR_EYE_DEF;
    public VRMBlendShapeProxy vrmBlendShapeProxy;

    [Tooltip("SkinnedMeshRendererのブレンドシェイプ名（VRMBlendShapeProxy使用時は無視）")]
    public string blendShapeName = "EyeBlink";

    [Header("Blink Timing")]
    [Range(0.0f, 1.0f)]
    public float minBlinkDuration = 0.1f;
    [Range(0.0f, 1.0f)]
    public float maxBlinkDuration = 1.0f;
    [Range(0.0f, 5.0f)]
    public float minBlinkPause = 1.0f;
    [Range(0.0f, 5.0f)]
    public float maxBlinkPause = 3.0f;

    [Header("Blink Animation Curve")]
    public AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Blink Weight Range")]
    [Range(0.0f, 100.0f)]
    public float openWeight = 37.77778f;   // 開いているとき
    [Range(0.0f, 100.0f)]
    public float closeWeight = 100.0f;     // 閉じているとき（新規追加）

    private float blinkDuration;
    private float blinkPause;
    private float blinkTimer = 0.0f;
    private float pauseTimer = 0.0f;
    private bool isBlinking = false;

    private int blendShapeIndex = -1;

    void Start()
    {
        blinkDuration = maxBlinkDuration;
        blinkPause = maxBlinkPause;

        if (mode == BlinkMode.SkinnedMeshRenderer && ref_SMR_EYE_DEF != null)
        {
            blendShapeIndex = ref_SMR_EYE_DEF.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex != -1)
            {
                ref_SMR_EYE_DEF.SetBlendShapeWeight(blendShapeIndex, openWeight);
            }
        }
    }

    void Update()
    {
        if (!isBlinking)
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= blinkPause)
            {
                pauseTimer = 0.0f;
                isBlinking = true;
                blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration);
            }
        }
        else
        {
            blinkTimer += Time.deltaTime;
            float t = Mathf.Clamp01(blinkTimer / blinkDuration);
            float eval = blinkCurve.Evaluate(t);

            // openWeight → closeWeight の範囲で補間
            float weight = Mathf.Lerp(openWeight, closeWeight, eval);
            ApplyBlinkWeight(weight);

            if (blinkTimer >= blinkDuration)
            {
                blinkTimer = 0.0f;
                isBlinking = false;
                blinkPause = Random.Range(minBlinkPause, maxBlinkPause);
                ApplyBlinkWeight(openWeight);
            }
        }
    }

    void ApplyBlinkWeight(float weight)
    {
        if (mode == BlinkMode.SkinnedMeshRenderer && ref_SMR_EYE_DEF != null && blendShapeIndex != -1)
        {
            ref_SMR_EYE_DEF.SetBlendShapeWeight(blendShapeIndex, weight);
        }
        else if (mode == BlinkMode.VRMBlendShapeProxy && vrmBlendShapeProxy != null)
        {
            vrmBlendShapeProxy.ImmediatelySetValue(BlendShapePreset.Blink, weight / 100.0f);
        }
    }
}
