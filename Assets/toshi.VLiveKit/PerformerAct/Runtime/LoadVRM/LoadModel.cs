using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// ビルド時無視
// #if UNITY_EDITOR
// using UnityEditor.Animations;

// animatorControllerを使用するために必要
using System.IO;
using System.Linq;

using SFB;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.Animations;
// #endif

using UniGLTF;
using VRM;
using VRMShaders;
// using MToon;


// using UnityEngine.Animations;
// using UnityEditor.Animations;

using EVMC4U;
using UniHumanoid;

// ❌ using Neuron;
 // using Neuron;

public class LoadModel : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector timelineDirector;
    [SerializeField]
    private bool useTimelineBinding = false;
    [SerializeField]
    public GameObject targetObjectInEditor;
    [SerializeField]
    private bool LoatOnStart = false;

    [SerializeField]
    public GameObject parentObject;

    RuntimeGltfInstance instance;
    // test

    public Shader shaderToUse;
    
    [SerializeField]
    private string VRM_TAG = "VRM";

    [SerializeField]
    private bool isMotionLoad = false;
    [SerializeField]
    private AnimationClip animationClip;

    [SerializeField]
    private RuntimeAnimatorController animatorController;

    void Start()
    {
        if (LoatOnStart)
        {
            Load();
        }
    }

    [ContextMenu("LoadVRM")]
    public async void Load()
    {
        Renderer[] renderers;

        if (Application.isPlaying)
        {
            var extensions = new[]
            {
                new ExtensionFilter("VRM Files", "vrm"),
                new ExtensionFilter("All Files", "*"),
            };

            string[] paths = StandaloneFileBrowser.OpenFilePanel("Open VRM File", "", extensions, false);
            if (paths.Length == 0) return;

            var PATH = paths[0];

            EnsureTagExists(VRM_TAG);
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag("VRM");
            
            this.instance = await VrmUtility.LoadAsync(PATH, new RuntimeOnlyAwaitCaller());

            var avatar = this.instance.gameObject;

            // ===== Neuron系ここからコメントアウト =====
            /*
            var neuronTransformsInstance = avatar.AddComponent<NeuronTransformsInstance>();

            var humanoidBoneCollector = avatar.AddComponent<HumanoidBoneCollector>();
            humanoidBoneCollector.animator = avatar.GetComponent<Animator>();
            humanoidBoneCollector.neuronInstance = neuronTransformsInstance;
            humanoidBoneCollector.CollectBones();
            humanoidBoneCollector.ApplyToNeuronTransforms();
            */
            // ===== Neuron系ここまで =====

            // 代替としてHumanoidだけ残す
            var humanoid = avatar.AddComponent<Humanoid>();
            humanoid.AssignBonesFromAnimator();

            this.instance.transform.position = new Vector3(0, 0, 0);
            this.instance.transform.SetParent(parentObject.transform, false);

            this.instance.gameObject.tag = "VRM";

            if (targetObjects != null && targetObjects.Length > 0)
            {
                foreach (GameObject targetObject in targetObjects)
                {
                    Destroy(targetObject);
                }
            }

            this.instance.ShowMeshes();
            this.instance.ShowMeshes();

            VRMFacialManager facialManager = FindObjectOfType<VRMFacialManager>();
            var animator = avatar.GetComponent<Animator>();

            if (facialManager != null)
            {
                VRMBlendShapeProxy proxy = this.instance.GetComponent<VRMBlendShapeProxy>();
                if (proxy != null)
                {
                    facialManager.SetBlendShapeProxy(proxy);
                    facialManager.SetAnimator(animator);
                }
            }

            renderers = this.instance.GetComponentsInChildren<Renderer>();

            if (isMotionLoad && animatorController != null)
            {
                if (animator == null)
                {
                    animator = avatar.AddComponent<Animator>();
                }
                animator.runtimeAnimatorController = animatorController;
                animator.applyRootMotion = true;
            }

            if (useTimelineBinding && timelineDirector != null)
            {
                var playableAsset = timelineDirector.playableAsset;
                if (playableAsset != null)
                {
                    foreach (var output in playableAsset.outputs)
                    {
                        if (output.outputTargetType == typeof(Animator))
                        {
                            var currentBinding = timelineDirector.GetGenericBinding(output.sourceObject) as Animator;

                            if (currentBinding == null || currentBinding != animator)
                            {
                                timelineDirector.SetGenericBinding(output.sourceObject, animator);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            renderers = targetObjectInEditor.GetComponentsInChildren<Renderer>();
        }

        foreach (var renderer in renderers)
        {
            if (renderer is SkinnedMeshRenderer smr)
            {
                smr.rootBone = Application.isPlaying ? this.instance.transform : targetObjectInEditor.transform;
            }

            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null) continue;
                if (material.shader.name == shaderToUse.name) continue;

                material.shader = shaderToUse;

                if (material.GetTexture("_MainTex") != null && material.GetTexture("_ShadeTexture") == null)
                {
                    material.SetTexture("_ShadeTexture", material.GetTexture("_MainTex"));
                }

                switch (material.GetInt("_BlendMode"))
                {
                    case 0:
                        material.renderQueue = 2225;
                        break;
                    case 1:
                        material.renderQueue = 2450;
                        break;
                    default:
                        material.renderQueue = 3000;
                        break;
                }
            }
        }
    }

    void EnsureTagExists(string tag)
    {
        bool found = false;
    }
}
