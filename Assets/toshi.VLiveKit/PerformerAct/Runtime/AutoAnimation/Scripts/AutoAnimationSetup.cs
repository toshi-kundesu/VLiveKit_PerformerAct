using System.Collections.Generic;
using UnityEngine;
using WindForVRM;
using VRM;

[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("VLiveKit/Performer Act/Auto Animation Setup")]
public sealed class AutoAnimationSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool setupOnEnable = true;
    [SerializeField] private bool requireHumanoid = true;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private VRMBlendShapeProxy blendShapeProxy;

    [Header("Auto Animation")]
    [SerializeField] private bool setupBlink = true;
    [SerializeField] private bool setupEyeDart = true;
    [SerializeField] private bool setupBreathing = true;
    [SerializeField] private bool setupMotionJitter = true;
    [SerializeField] private bool setupFacialJitter = true;
    [SerializeField] private bool setupVrmWind = true;
    [SerializeField] private AutoBlink autoBlink;
    [SerializeField] private AutoEyeDirt eyeDart;
    [SerializeField] private BreathingAnimation breathingAnimation;
    [SerializeField] private MultiBoneOffsetController motionJitter;
    [SerializeField] private BlendShapeFollower facialJitter;
    [SerializeField] private VRMWind vrmWind;

    [Header("Facial Control")]
    [SerializeField] private bool setupFacialControls = true;
    [SerializeField] private bool setupLipSync = true;
    [SerializeField] private bool setupFacialManager = true;
    [SerializeField] private JoyStickReceiver joystickReceiver;
    [SerializeField] private ExpressionController expressionController;
    [SerializeField] private FacialReceiver facialReceiver;
    [SerializeField] private FacialBlendShapeController lipSyncController;
    [SerializeField] private VRMFacialManager facialManager;

    public bool IsHumanoid
    {
        get
        {
            var animator = ResolveAnimator();
            return animator != null && animator.isHuman;
        }
    }

    private void Reset()
    {
        characterAnimator = FindAnimator();
        targetCamera = FindCamera();
        blendShapeProxy = FindBlendShapeProxy();
        faceRenderer = FindFaceRenderer();
        Setup();
    }

    private void OnEnable()
    {
        if (setupOnEnable)
        {
            Setup();
        }
    }

    [ContextMenu("Setup Auto Animation")]
    public void Setup()
    {
        SetupInternal(rebuildGeneratedDefaults:false);
    }

    [ContextMenu("Resetup Auto Animation")]
    public void Resetup()
    {
        RefreshDetectedReferences();
        SetupInternal(rebuildGeneratedDefaults:true);
    }

    private void SetupInternal(bool rebuildGeneratedDefaults)
    {
        var animator = ResolveAnimator();
        if (requireHumanoid && (animator == null || !animator.isHuman))
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = FindCamera();
        }

        if (blendShapeProxy == null)
        {
            blendShapeProxy = FindBlendShapeProxy();
        }

        if (faceRenderer == null)
        {
            faceRenderer = FindFaceRenderer();
        }

        var host = animator != null ? animator.gameObject : gameObject;

        if (setupBlink)
        {
            SetupBlink(host);
        }

        if (animator != null && setupEyeDart)
        {
            SetupEyeDart(host);
        }

        if (setupBreathing)
        {
            SetupBreathing(host, animator, rebuildGeneratedDefaults);
        }

        if (setupFacialJitter)
        {
            SetupFacialJitter(host, rebuildGeneratedDefaults);
        }

        if (animator != null && setupMotionJitter)
        {
            SetupMotionJitter(host, rebuildGeneratedDefaults);
        }

        if (setupVrmWind)
        {
            SetupVrmWind(host, rebuildGeneratedDefaults);
        }

        if (setupFacialControls)
        {
            SetupFacialControls(host);
        }

        if (setupLipSync)
        {
            SetupLipSync(host);
        }

        if (setupFacialManager)
        {
            SetupFacialManager(host, animator);
        }
    }

    private void RefreshDetectedReferences()
    {
        characterAnimator = FindAnimator();
        targetCamera = FindCamera();
        blendShapeProxy = FindBlendShapeProxy();
        faceRenderer = FindFaceRenderer();
    }

    [ContextMenu("Use Default Motion Preset")]
    private void UseDefaultMotionPreset()
    {
        var animator = ResolveAnimator();
        var host = animator != null ? animator.gameObject : gameObject;

        breathingAnimation = EnsureComponent(host, breathingAnimation);
        SeedDefaultBreathingBones(breathingAnimation, true);

        if (animator != null)
        {
            motionJitter = EnsureComponent(host, motionJitter);
            SeedDefaultMotionJitter(motionJitter, true);
        }
    }

    private void SetupBlink(GameObject host)
    {
        autoBlink = EnsureComponent(host, autoBlink);
        if (autoBlink == null)
        {
            return;
        }

        if (blendShapeProxy != null)
        {
            autoBlink.mode = AutoBlink.BlinkMode.VRMBlendShapeProxy;
            autoBlink.vrmBlendShapeProxy = blendShapeProxy;
            return;
        }

        if (faceRenderer == null)
        {
            return;
        }

        autoBlink.mode = AutoBlink.BlinkMode.SkinnedMeshRenderer;
        autoBlink.ref_SMR_EYE_DEF = faceRenderer;

        var blinkShapeName = FindBlinkBlendShapeName(faceRenderer);
        if (!string.IsNullOrEmpty(blinkShapeName))
        {
            autoBlink.blendShapeName = blinkShapeName;
        }
    }

    private void SetupEyeDart(GameObject host)
    {
        eyeDart = EnsureComponent(host, eyeDart);
        if (eyeDart != null && targetCamera != null)
        {
            eyeDart.SetTargetCamera(targetCamera.transform);
        }
    }

    private void SetupBreathing(GameObject host, Animator animator, bool rebuildGeneratedDefaults)
    {
        breathingAnimation = EnsureComponent(host, breathingAnimation);
        if (breathingAnimation == null)
        {
            return;
        }

        breathingAnimation.animator = animator;
        breathingAnimation.faceMesh = faceRenderer;
        breathingAnimation.vrmBlendShapeProxy = blendShapeProxy;
        SeedDefaultBreathingBones(breathingAnimation, rebuildGeneratedDefaults);
    }

    private void SetupMotionJitter(GameObject host, bool rebuildGeneratedDefaults)
    {
        motionJitter = EnsureComponent(host, motionJitter);
        SeedDefaultMotionJitter(motionJitter, rebuildGeneratedDefaults);
    }

    private void SetupFacialJitter(GameObject host, bool rebuildGeneratedDefaults)
    {
        if (faceRenderer == null)
        {
            return;
        }

        facialJitter = EnsureComponent(host, facialJitter);
        if (facialJitter == null)
        {
            return;
        }

        var defaultSets = CreateDefaultFacialJitterSets(faceRenderer);
        facialJitter.Configure(faceRenderer, defaultSets, rebuildGeneratedDefaults);
    }

    private void SetupVrmWind(GameObject host, bool rebuildGeneratedDefaults)
    {
        vrmWind = EnsureComponent(host, vrmWind);
        if (vrmWind == null)
        {
            return;
        }

        if (rebuildGeneratedDefaults)
        {
            vrmWind.ReloadVrm(host.transform);
        }
        else
        {
            vrmWind.LoadVrm(host.transform);
        }
    }

    private void SetupFacialControls(GameObject host)
    {
        joystickReceiver = EnsureComponent(host, joystickReceiver);
        expressionController = EnsureComponent(host, expressionController);
        if (expressionController == null)
        {
            return;
        }

        expressionController.joystickReceiver = joystickReceiver;
        expressionController.blendShapeProxy = blendShapeProxy;
        expressionController.skinnedMeshRenderer = faceRenderer;
        expressionController.useBlendShapeProxy = HasUsableBlendShapeAvatar(blendShapeProxy);

        SeedDefaultExpressionMappings(expressionController);
        expressionController.UpdateBlendShapeList();
    }

    private void SetupLipSync(GameObject host)
    {
        facialReceiver = EnsureComponent(host, facialReceiver);
        lipSyncController = EnsureComponent(host, lipSyncController);
        if (lipSyncController == null)
        {
            return;
        }

        if (blendShapeProxy != null)
        {
            lipSyncController.targetType = FacialBlendShapeController.TargetType.VRM;
            lipSyncController.blendShapeProxy = blendShapeProxy;
            lipSyncController.targetMesh = null;
            return;
        }

        lipSyncController.targetType = FacialBlendShapeController.TargetType.SkinnedMeshRenderer;
        lipSyncController.targetMesh = faceRenderer;
        lipSyncController.blendShapeProxy = null;
    }

    private void SetupFacialManager(GameObject host, Animator animator)
    {
        facialManager = EnsureComponent(host, facialManager);
        if (facialManager == null)
        {
            return;
        }

        facialManager.vrmAnimator = animator;
        facialManager.vrmBlendShapeProxy = blendShapeProxy;
        facialManager.expressionController = expressionController;
        facialManager.facialBlendShapeController = lipSyncController;
        facialManager.autoBlink = autoBlink;
        facialManager.breathingAnimation = breathingAnimation;
    }

    private Animator ResolveAnimator()
    {
        if (characterAnimator != null)
        {
            return characterAnimator;
        }

        characterAnimator = FindAnimator();
        return characterAnimator;
    }

    private Animator FindAnimator()
    {
        return GetComponentInChildren<Animator>(true);
    }

    private VRMBlendShapeProxy FindBlendShapeProxy()
    {
        return GetComponentInChildren<VRMBlendShapeProxy>(true);
    }

    private SkinnedMeshRenderer FindFaceRenderer()
    {
        var renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        SkinnedMeshRenderer best = null;
        var bestScore = int.MinValue;

        foreach (var renderer in renderers)
        {
            if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount == 0)
            {
                continue;
            }

            var score = ScoreFaceRenderer(renderer);
            if (score > bestScore)
            {
                best = renderer;
                bestScore = score;
            }
        }

        return best;
    }

    private static int ScoreFaceRenderer(SkinnedMeshRenderer renderer)
    {
        var score = renderer.sharedMesh.blendShapeCount;
        var rendererName = renderer.name.ToLowerInvariant();
        var meshName = renderer.sharedMesh.name.ToLowerInvariant();

        if (ContainsAny(rendererName, "face", "head", "body", "eye", "facial") ||
            ContainsAny(meshName, "face", "head", "body", "eye", "facial"))
        {
            score += 100;
        }

        if (!string.IsNullOrEmpty(FindBlinkBlendShapeName(renderer)))
        {
            score += 200;
        }

        return score;
    }

    private static string FindBlinkBlendShapeName(SkinnedMeshRenderer renderer)
    {
        if (renderer == null || renderer.sharedMesh == null)
        {
            return null;
        }

        var mesh = renderer.sharedMesh;
        var preferredNames = new[]
        {
            "Blink",
            "EyeBlink",
            "EyeBlink_L",
            "Blink_L",
            "blink",
            "まばたき"
        };

        foreach (var preferredName in preferredNames)
        {
            var index = mesh.GetBlendShapeIndex(preferredName);
            if (index >= 0)
            {
                return mesh.GetBlendShapeName(index);
            }
        }

        for (var i = 0; i < mesh.blendShapeCount; i++)
        {
            var shapeName = mesh.GetBlendShapeName(i);
            var lowerName = shapeName.ToLowerInvariant();
            if (lowerName.Contains("blink") || shapeName.Contains("まばたき"))
            {
                return shapeName;
            }
        }

        return null;
    }

    private static Camera FindCamera()
    {
        if (Camera.main != null)
        {
            return Camera.main;
        }

        var cameras = FindSceneCameras();
        foreach (var camera in cameras)
        {
            if (camera != null && camera.isActiveAndEnabled)
            {
                return camera;
            }
        }

        return null;
    }

    private static Camera[] FindSceneCameras()
    {
#if UNITY_2022_2_OR_NEWER || UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        return FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        return FindObjectsOfType<Camera>();
#endif
    }

    private T EnsureComponent<T>(GameObject host, T current) where T : Component
    {
        if (current != null)
        {
            return current;
        }

        if (host != null && host.TryGetComponent<T>(out var existingOnHost))
        {
            return existingOnHost;
        }

        var existingInRoot = GetComponentInChildren<T>(true);
        if (existingInRoot != null)
        {
            return existingInRoot;
        }

        return host != null ? host.AddComponent<T>() : null;
    }

    private static void SeedDefaultBreathingBones(BreathingAnimation target, bool replace)
    {
        if (target == null)
        {
            return;
        }

        if (replace)
        {
            target.bones.Clear();
        }
        else if (target.bones.Count > 0)
        {
            return;
        }

        AddBreathingBone(target.bones, HumanBodyBones.Spine, 1.2f, 0.2f, 0.0f, 0.00f);
        AddBreathingBone(target.bones, HumanBodyBones.Chest, 2.2f, 0.4f, 0.0f, 0.08f);
        AddBreathingBone(target.bones, HumanBodyBones.UpperChest, 1.6f, 0.3f, 0.0f, 0.16f);
        AddBreathingBone(target.bones, HumanBodyBones.Neck, 0.4f, 0.0f, 0.2f, 0.45f);
    }

    private static void AddBreathingBone(
        List<BoneSettings> bones,
        HumanBodyBones humanoidBone,
        float angleAmplitudeX,
        float angleAmplitudeY,
        float angleAmplitudeZ,
        float phaseOffset)
    {
        bones.Add(new BoneSettings
        {
            useAnimator = true,
            humanoidBone = humanoidBone,
            angleAmplitudeX = angleAmplitudeX,
            angleAmplitudeY = angleAmplitudeY,
            angleAmplitudeZ = angleAmplitudeZ,
            phaseOffset = phaseOffset
        });
    }

    private static void SeedDefaultMotionJitter(MultiBoneOffsetController target, bool replace)
    {
        if (target == null)
        {
            return;
        }

        if (replace)
        {
            target.boneSettings.Clear();
        }
        else if (target.boneSettings.Count > 0)
        {
            return;
        }

        AddMotionJitterBone(target.boneSettings, HumanBodyBones.Head, new Vector3(0.35f, 0.55f, 0.25f), 0.37f, 0.10f);
        AddMotionJitterBone(target.boneSettings, HumanBodyBones.Neck, new Vector3(0.20f, 0.35f, 0.20f), 0.31f, 0.30f);
        AddMotionJitterBone(target.boneSettings, HumanBodyBones.LeftShoulder, new Vector3(0.15f, 0.20f, 0.30f), 0.23f, 0.55f);
        AddMotionJitterBone(target.boneSettings, HumanBodyBones.RightShoulder, new Vector3(0.15f, 0.20f, 0.30f), 0.27f, 0.75f);
    }

    private static void AddMotionJitterBone(
        List<BoneOffsetSetting> bones,
        HumanBodyBones humanoidBone,
        Vector3 offsetAmplitude,
        float offsetSpeed,
        float phaseOffset)
    {
        bones.Add(new BoneOffsetSetting
        {
            bone = humanoidBone,
            offsetAmplitude = offsetAmplitude,
            offsetSpeed = offsetSpeed,
            phaseOffset = phaseOffset
        });
    }

    private static List<BlendShapeFollower.BlendShapeSet> CreateDefaultFacialJitterSets(SkinnedMeshRenderer renderer)
    {
        var sets = new List<BlendShapeFollower.BlendShapeSet>();
        if (renderer == null || renderer.sharedMesh == null)
        {
            return sets;
        }

        var blinkShapeName = FindBlinkBlendShapeName(renderer);
        if (string.IsNullOrEmpty(blinkShapeName))
        {
            return sets;
        }

        AddFacialJitterSetIfFound(
            sets,
            renderer,
            blinkShapeName,
            0.65f,
            0.45f,
            0.9f,
            0,
            "下",
            "EyeDown",
            "LookDown",
            "Lower",
            "lower");

        AddFacialJitterSetIfFound(
            sets,
            renderer,
            blinkShapeName,
            0.12f,
            0.45f,
            0.9f,
            1,
            "困る",
            "Troubled",
            "Sorrow",
            "Sad",
            "sad");

        return sets;
    }

    private static void AddFacialJitterSetIfFound(
        List<BlendShapeFollower.BlendShapeSet> sets,
        SkinnedMeshRenderer renderer,
        string controllerBlendShapeName,
        float followRatio,
        float noiseStrength,
        float noiseSpeed,
        int noiseSyncNum,
        params string[] targetCandidates)
    {
        var targetBlendShapeName = FindBlendShapeName(renderer, targetCandidates);
        if (string.IsNullOrEmpty(targetBlendShapeName) || targetBlendShapeName == controllerBlendShapeName)
        {
            return;
        }

        sets.Add(new BlendShapeFollower.BlendShapeSet
        {
            controllerBlendShapeName = controllerBlendShapeName,
            targetBlendShapeName = targetBlendShapeName,
            followRatio = followRatio,
            noiseStrength = noiseStrength,
            noiseSpeed = noiseSpeed,
            noiseSyncNum = noiseSyncNum
        });
    }

    private void SeedDefaultExpressionMappings(ExpressionController target)
    {
        if (target == null || target.blendShapeKeyMappings.Count > 0)
        {
            return;
        }

        if (HasUsableBlendShapeAvatar(blendShapeProxy))
        {
            AddVRMExpressionMapping(target.blendShapeKeyMappings, "Joy", BlendShapeKeyMapping.InputType.Button1);
            AddVRMExpressionMapping(target.blendShapeKeyMappings, "Angry", BlendShapeKeyMapping.InputType.Button2);
            AddVRMExpressionMapping(target.blendShapeKeyMappings, "Sorrow", BlendShapeKeyMapping.InputType.Button3);
            AddVRMExpressionMapping(target.blendShapeKeyMappings, "Fun", BlendShapeKeyMapping.InputType.Button4);
            return;
        }

        AddRendererExpressionMapping(target.blendShapeKeyMappings, BlendShapeKeyMapping.InputType.Button1, "Joy", "Happy", "Smile", "smile");
        AddRendererExpressionMapping(target.blendShapeKeyMappings, BlendShapeKeyMapping.InputType.Button2, "Angry", "angry");
        AddRendererExpressionMapping(target.blendShapeKeyMappings, BlendShapeKeyMapping.InputType.Button3, "Sorrow", "Sad", "sad");
        AddRendererExpressionMapping(target.blendShapeKeyMappings, BlendShapeKeyMapping.InputType.Button4, "Fun", "Surprise", "surprise");
    }

    private void AddVRMExpressionMapping(
        List<BlendShapeKeyMapping> mappings,
        string blendShapeName,
        BlendShapeKeyMapping.InputType inputType)
    {
        if (HasVRMBlendShape(blendShapeName))
        {
            mappings.Add(CreateExpressionMapping(blendShapeName, inputType));
        }
    }

    private void AddRendererExpressionMapping(
        List<BlendShapeKeyMapping> mappings,
        BlendShapeKeyMapping.InputType inputType,
        params string[] candidates)
    {
        var shapeName = FindBlendShapeName(faceRenderer, candidates);
        if (!string.IsNullOrEmpty(shapeName))
        {
            mappings.Add(CreateExpressionMapping(shapeName, inputType));
        }
    }

    private static BlendShapeKeyMapping CreateExpressionMapping(
        string blendShapeName,
        BlendShapeKeyMapping.InputType inputType)
    {
        return new BlendShapeKeyMapping
        {
            blendShapeName = blendShapeName,
            inputType = inputType,
            targetWeightPercentage = 100.0f,
            transitionDuration = 0.1f,
            animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1)
        };
    }

    private bool HasVRMBlendShape(string blendShapeName)
    {
        if (!HasUsableBlendShapeAvatar(blendShapeProxy))
        {
            return false;
        }

        var clips = blendShapeProxy.BlendShapeAvatar.Clips;
        foreach (var clip in clips)
        {
            if (clip != null && clip.Key.ToString() == blendShapeName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasUsableBlendShapeAvatar(VRMBlendShapeProxy proxy)
    {
        return proxy != null && proxy.BlendShapeAvatar != null && proxy.BlendShapeAvatar.Clips != null;
    }

    private static string FindBlendShapeName(SkinnedMeshRenderer renderer, params string[] candidates)
    {
        if (renderer == null || renderer.sharedMesh == null)
        {
            return null;
        }

        var mesh = renderer.sharedMesh;
        foreach (var candidate in candidates)
        {
            var index = mesh.GetBlendShapeIndex(candidate);
            if (index >= 0)
            {
                return mesh.GetBlendShapeName(index);
            }
        }

        foreach (var candidate in candidates)
        {
            var lowerCandidate = candidate.ToLowerInvariant();
            for (var i = 0; i < mesh.blendShapeCount; i++)
            {
                var shapeName = mesh.GetBlendShapeName(i);
                if (shapeName.ToLowerInvariant().Contains(lowerCandidate))
                {
                    return shapeName;
                }
            }
        }

        return null;
    }

    private static bool ContainsAny(string source, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (source.Contains(token))
            {
                return true;
            }
        }

        return false;
    }
}
