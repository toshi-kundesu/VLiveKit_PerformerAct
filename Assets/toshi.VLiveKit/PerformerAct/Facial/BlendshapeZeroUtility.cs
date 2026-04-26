// VLiveKit is all Unlicense.
// unlicense: https://unlicense.org/
// last update: 2024/11/26
//
// BlendshapeZeroUtility.cs
// -------------------------------------------------------------
// public 関数 ZeroBlendShapes() を呼ぶだけで
// targetRoots 以下の SkinnedMeshRenderer の BlendShape をすべて 0 にします。
// -------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace toshi.VLiveKit.Lighting
{
    [AddComponentMenu("VLiveKit/Blendshape Zero Utility")]
    public sealed class BlendshapeZeroUtility : MonoBehaviour
    {
        [Header("BlendShape を 0 にする検索ルート（複数指定可）")]
        public List<Transform> targetRoots = new();

        [Tooltip("子階層も含めて検索（false なら直下のみ）")]
        public bool includeChildren = true;

        [Tooltip("非アクティブの子も対象にするか")]
        public bool includeInactive = true;

        [Header("自動実行")]
        [Tooltip("Start 時に一度だけ ZeroBlendShapes を実行する")]
        public bool executeOnStart = false;

        //────────────────────────────────────────────
        // Unity Lifecycle
        //────────────────────────────────────────────
        private void Start()
        {
            if (!executeOnStart) return;
            ZeroBlendShapes();
        }

        //────────────────────────────────────────────
        // 公開 API ─ Timeline／Button／他スクリプトから呼ぶ用
        //────────────────────────────────────────────
        [ContextMenu("Zero All BlendShapes")]   // Inspector 右クリックでも実行可能
        public void ZeroBlendShapes()
        {
            if (targetRoots == null || targetRoots.Count == 0)
            {
                Debug.LogWarning("[BlendshapeZeroUtility] targetRoots が未設定です。");
                return;
            }

            int totalCount = 0;

            foreach (var root in targetRoots)
            {
                if (root == null) continue;

                var renderers = includeChildren
                    ? root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive)
                    : root.GetComponents<SkinnedMeshRenderer>();

                foreach (var smr in renderers)
                {
                    if (smr == null || smr.sharedMesh == null) continue;

                    int count = smr.sharedMesh.blendShapeCount;
                    for (int i = 0; i < count; i++)
                        smr.SetBlendShapeWeight(i, 0f);

                    totalCount += count;
                }
            }

            Debug.Log($"[BlendshapeZeroUtility] Zero All BlendShapes 完了 ({totalCount} 個リセット)");
        }
    }
}
