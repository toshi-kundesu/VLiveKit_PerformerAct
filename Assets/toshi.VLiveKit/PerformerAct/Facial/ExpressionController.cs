using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRM;

public class ExpressionController : MonoBehaviour
{
    [Header("Skinned Mesh Renderer")]
    public SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("VRM Blend Shape Proxy")]
    public VRMBlendShapeProxy blendShapeProxy;

    [Header("Use VRM Blend Shape Proxy (On) or Skinned Mesh Renderer (Off)")]
    public bool useBlendShapeProxy = true;

    [Header("Joystick Input")]
    public JoyStickReceiver joystickReceiver;

    public List<BlendShapeInfo> blendShapeInfos = new List<BlendShapeInfo>();
    public List<BlendShapeKeyMapping> blendShapeKeyMappings = new List<BlendShapeKeyMapping>();

    private Dictionary<string, int> blendShapeNameToIndex = new Dictionary<string, int>();
    private Dictionary<KeyCode, Coroutine> activeCoroutines = new Dictionary<KeyCode, Coroutine>();
    private Dictionary<int, Coroutine> blendShapeCoroutines = new Dictionary<int, Coroutine>();
    private Dictionary<int, float> blendShapeMaxWeights = new Dictionary<int, float>();

    void Start()
    {
        UpdateBlendShapeList();
    }

    void Update()
    {
        foreach (var mapping in blendShapeKeyMappings)
        {
            if (blendShapeNameToIndex.TryGetValue(mapping.blendShapeName, out int index))
            {
                bool inputDetected = false;

                switch (mapping.inputType)
                {
                    case BlendShapeKeyMapping.InputType.Key:
                        if (Input.GetKey(mapping.triggerKey))
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.DPadUp:
                        if (Input.GetAxis("D_Pad_V") > 0)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.DPadDown:
                        if (Input.GetAxis("D_Pad_V") < 0)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.DPadLeft:
                        if (Input.GetAxis("D_Pad_H") < 0)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.DPadRight:
                        if (Input.GetAxis("D_Pad_H") > 0)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.LTrigger:
                        if (Input.GetAxis("L_R_Trigger") < 0)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.RTrigger:
                        if (Input.GetAxis("L_R_Trigger") > 0)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.JoyStickX:
                        if (joystickReceiver != null && Mathf.Abs(joystickReceiver.p1_X) > 0.1f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.JoyStickY:
                        if (joystickReceiver != null && Mathf.Abs(joystickReceiver.p1_Y) > 0.1f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.Slider1:
                        if (joystickReceiver != null && Mathf.Abs(joystickReceiver.slider1) > 0.1f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.Slider2:
                        if (joystickReceiver != null && Mathf.Abs(joystickReceiver.slider2) > 0.1f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.Button1:
                        if (joystickReceiver != null && joystickReceiver.b1 > 0.5f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.Button2:
                        if (joystickReceiver != null && joystickReceiver.b2 > 0.5f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.Button3:
                        if (joystickReceiver != null && joystickReceiver.b3 > 0.5f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.Button4:
                        if (joystickReceiver != null && joystickReceiver.b4 > 0.5f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.Button5:
                        if (joystickReceiver != null && joystickReceiver.b5 > 0.5f)
                        {
                            inputDetected = true;
                        }
                        break;
                    case BlendShapeKeyMapping.InputType.Button6:
                        if (joystickReceiver != null && joystickReceiver.b6 > 0.5f)
                        {
                            inputDetected = true;
                        }
                        break;
                    // LRトリガー
                    // Update メソッド内のcase文を修正
                    case BlendShapeKeyMapping.InputType.zaxis:
                        if (joystickReceiver != null)
                        {
                            // トリガーの値（0.0f～1.0f）に応じてブレンドシェイプの値を直接設定
                            float triggerValue = Mathf.Clamp01(joystickReceiver.zaxis);
                            float targetWeight = triggerValue * mapping.targetWeightPercentage;
                            
                            // コルーチンを使わず直接値を設定
                            if (blendShapeCoroutines.TryGetValue(index, out Coroutine routine) && routine != null)
                            {
                                StopCoroutine(routine);
                                blendShapeCoroutines.Remove(index);
                            }
                            SetBlendShapeWeight(index, targetWeight);
                        }
                        break;
                }

                if (inputDetected)
                {
                    if (!mapping.isActive)
                    {
                        mapping.isActive = true;
                        if (blendShapeCoroutines.TryGetValue(index, out Coroutine routine) && routine != null)
                        {
                            StopCoroutine(routine);
                        }
                        blendShapeCoroutines[index] = StartCoroutine(ChangeExpressionCoroutine(index, mapping.transitionDuration, mapping.targetWeightPercentage, mapping.animationCurve));
                    }
                }
                else
                {
                    if (mapping.isActive)
                    {
                        mapping.isActive = false;
                        if (blendShapeCoroutines.TryGetValue(index, out Coroutine routine) && routine != null)
                        {
                            StopCoroutine(routine);
                        }
                        blendShapeCoroutines[index] = StartCoroutine(ChangeExpressionCoroutine(index, mapping.transitionDuration, 0.0f, mapping.animationCurve));
                    }
                }
            }
            else
            {
                Debug.LogWarning($"BlendShape '{mapping.blendShapeName}' not found.");
            }
        }
    }

    private IEnumerator ChangeExpressionCoroutine(int index, float duration, float endValuePercentage, AnimationCurve curve)
    {
        float maxWeight = blendShapeMaxWeights.ContainsKey(index) ? blendShapeMaxWeights[index] : 100f;
        float endValue = (endValuePercentage / 100f) * maxWeight;

        if (duration <= 0f)
        {
            SetBlendShapeWeight(index, endValue);
            yield break;
        }

        float elapsedTime = 0f;
        float startValue = GetBlendShapeWeight(index);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float blendValue = Mathf.Lerp(startValue, endValue, curve.Evaluate(t));
            SetBlendShapeWeight(index, blendValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetBlendShapeWeight(index, endValue);
    }

    private void SetBlendShapeWeight(int index, float weight)
    {
        if (useBlendShapeProxy && blendShapeProxy != null)
        {
            var key = blendShapeProxy.BlendShapeAvatar.Clips[index].Key;
            blendShapeProxy.ImmediatelySetValue(key, weight / 100f);
        }
        else if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(index, weight);
        }
    }

    private float GetBlendShapeWeight(int index)
    {
        if (useBlendShapeProxy && blendShapeProxy != null)
        {
            var key = blendShapeProxy.BlendShapeAvatar.Clips[index].Key;
            return blendShapeProxy.GetValue(key) * 100f;
        }
        else if (skinnedMeshRenderer != null)
        {
            return skinnedMeshRenderer.GetBlendShapeWeight(index);
        }
        return 0f;
    }

    public void UpdateBlendShapeList()
    {
        blendShapeInfos.Clear();
        blendShapeNameToIndex.Clear();
        blendShapeMaxWeights.Clear();

        if (useBlendShapeProxy && blendShapeProxy != null)
        {
            var clips = blendShapeProxy.BlendShapeAvatar.Clips;
            for (int i = 0; i < clips.Count; i++)
            {
                string shapeName = clips[i].Key.ToString();
                blendShapeInfos.Add(new BlendShapeInfo(i, shapeName));
                blendShapeNameToIndex[shapeName] = i;
                blendShapeMaxWeights[i] = 100f;
            }
        }
        else if (skinnedMeshRenderer != null)
        {
            Mesh mesh = skinnedMeshRenderer.sharedMesh;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string shapeName = mesh.GetBlendShapeName(i);
                blendShapeInfos.Add(new BlendShapeInfo(i, shapeName));
                blendShapeNameToIndex[shapeName] = i;
                blendShapeMaxWeights[i] = 100f;
            }
        }
    }

    [ContextMenu("Update BlendShape List")]
    private void UpdateBlendShapeListContextMenu()
    {
        UpdateBlendShapeList();
    }
}

[System.Serializable]
public class BlendShapeKeyMapping
{
    public enum InputType
    {
        Key,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        LTrigger,
        RTrigger,
        JoyStickX,
        JoyStickY,
        Slider1,
        Slider2,
        Button1,
        Button2,
        Button3,
        Button4,
        Button5,
        Button6,
        zaxis
    }

    public string blendShapeName;
    public KeyCode triggerKey;
    [Range(0, 100)]
    public float targetWeightPercentage = 100.0f;
    [Range(0, 1)]
    public float transitionDuration = 0.1f;
    public InputType inputType = InputType.Key;
    public bool isActive = false;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}

// [System.Serializable]
// public class BlendShapeInfo
// {
//     public int index;
//     public string name;

//     public BlendShapeInfo(int index, string name)
//     {
//         this.index = index;
//         this.name = name;
//     }
// }