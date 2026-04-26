using UnityEngine;
using VRM;

/// <summary>
/// ExpressionControllerやFacialBlendShapeControllerにBlendShapeProxyをセットするマネージャ
/// </summary>
public class VRMFacialManager : MonoBehaviour
{
    // PlauOnStart
    [SerializeField]
    private bool playOnStart = false;
    [Header("操作対象のAnimator")]
    public Animator vrmAnimator;
    [Header("操作対象のBlendShapeProxy")]
    public VRMBlendShapeProxy vrmBlendShapeProxy;
    [Header("操作対象のExpressionController (任意)")]
    public ExpressionController expressionController;

    [Header("操作対象のFacialBlendShapeController (任意)")]
    public FacialBlendShapeController facialBlendShapeController;
    [Header("操作対象のAutoBlink (任意)")]
    public AutoBlink autoBlink;
    [Header("操作対象のBreathingAnimation (任意)")]
    public BreathingAnimation breathingAnimation;
    [Header("操作対象のEyeDirt (任意)")]
    public EyeDirt eyeDirt;
    // [Header("NeuronTransformsInstance (任意)")]
    // public NeuronBinderCaller neuronBinderCaller;

    /// <summary>
    /// 指定されたBlendShapeProxyを対象コントローラにセットする
    /// </summary>
    /// <param name="newProxy">セットするVRMBlendShapeProxy</param>
    /// 
    void Start()
    {
        if (playOnStart)
        {
            SetBlendShapeProxy(vrmBlendShapeProxy);
            SetAnimator(vrmAnimator);
        }
    }

    public void SetAnimator(Animator newAnimator)
    {
        vrmAnimator = newAnimator;
        if (breathingAnimation != null)
        {
            breathingAnimation.animator = vrmAnimator;
        }
        if (eyeDirt != null)
        {
            eyeDirt.animator = vrmAnimator;
        }
    }
    public void SetNeuronComponent()
    {
        // 生成したvrmオブジェクトに
    }
    
    public void SetBlendShapeProxy(VRMBlendShapeProxy newProxy)
    {
        if (newProxy == null)
        {
            Debug.LogWarning("渡されたBlendShapeProxyがnullです！");
            return;
        }

        bool setAny = false;

        if (expressionController != null)
        {
            expressionController.blendShapeProxy = newProxy;
            expressionController.useBlendShapeProxy = true;
            expressionController.UpdateBlendShapeList();
            Debug.Log($"ExpressionControllerにBlendShapeProxyをセットしました: {newProxy.name}");
            setAny = true;
        }

        if (facialBlendShapeController != null)
        {
            facialBlendShapeController.blendShapeProxy = newProxy;
            facialBlendShapeController.targetType = FacialBlendShapeController.TargetType.VRM; // VRMモードにする
            Debug.Log($"FacialBlendShapeControllerにBlendShapeProxyをセットしました: {newProxy.name}");
            setAny = true;
        }

        if (autoBlink != null)
        {
            autoBlink.vrmBlendShapeProxy = newProxy;
            Debug.Log($"AutoBlinkにBlendShapeProxyをセットしました: {newProxy.name}");
            setAny = true;
        }

        if (breathingAnimation != null)
        {
            breathingAnimation.vrmBlendShapeProxy = newProxy;
            Debug.Log($"BreathingAnimationにBlendShapeProxyをセットしました: {newProxy.name}");
            setAny = true;

            // animator

            // var animator = breathingAnimation.GetComponent<Animator>();
            // if (animator != null)
            // {
            //     // vrm animatorをbreathingAnimationのAnimatorにセットする
            //     // var vrmAnimator = GetComponent<Animator>();
            //     breathingAnimation.animator = vrmAnimator;
            // }
        }
        if (!setAny)
        {
            Debug.LogWarning("どちらのコントローラも設定されていません！");
        }
    }
}
