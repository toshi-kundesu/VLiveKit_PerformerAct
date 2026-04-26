// EnsureUpdateWhenOffscreenOnStart.cs
// シーン内の SkinnedMeshRenderer で updateWhenOffscreen==false を Start 時に true にし、変更をログ出力する。
// Unity 2020+ / マルチシーン対応

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public sealed class EnsureUpdateWhenOffscreenOnStart : MonoBehaviour
{
    [Header("検索設定")]
    [Tooltip("非アクティブなGameObject内も検索する")]
    public bool includeInactive = true;

    [Header("ログ設定")]
    [Tooltip("実際にtrueへ変更したものだけログ出力する（true）/ すでにtrueのものも含めて出す（false）")]
    public bool logOnlyWhenChanged = true;

    [Tooltip("最後に合計サマリを出力する")]
    public bool logSummary = true;

    private void Start()
    {
        int total = 0;
        int changed = 0;

        foreach (var smr in EnumerateAllSkinnedMeshRenderers(includeInactive))
        {
            if (smr == null) continue;
            total++;

            if (!smr.updateWhenOffscreen)
            {
                smr.updateWhenOffscreen = true;
                changed++;
                LogSMR(smr, changedFlag: true);
            }
            else if (!logOnlyWhenChanged)
            {
                LogSMR(smr, changedFlag: false);
            }
        }

        if (logSummary)
        {
            Debug.Log($"[EnsureUpdateWhenOffscreenOnStart] 対象合計: {total}, 変更: {changed}, 既にON: {total - changed}");
        }
    }

    private static IEnumerable<SkinnedMeshRenderer> EnumerateAllSkinnedMeshRenderers(bool includeInactive)
    {
        // マルチシーン対応：ロード済み全シーンを走査
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root == null) continue;
                foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive))
                {
                    yield return smr;
                }
            }
        }
    }

    private static void LogSMR(SkinnedMeshRenderer smr, bool changedFlag)
    {
        string state = changedFlag ? "<color=#00C853>CHANGED → updateWhenOffscreen=TRUE</color>"
                                   : "<color=#9E9E9E>ALREADY TRUE</color>";
        string path = GetTransformPath(smr.transform);
        string goActive = smr.gameObject.activeInHierarchy ? "ActiveInHierarchy" : "InactiveInHierarchy";
        string compEnabled = smr.enabled ? "RendererEnabled" : "RendererDisabled";

        Debug.Log($"[EnsureUpdateWhenOffscreenOnStart] {state} | {path} | {goActive} | {compEnabled}", smr);
    }

    private static string GetTransformPath(Transform t)
    {
        // ルートまでの階層パス（Scene名/Root/Child/...）
        var stack = new System.Collections.Generic.Stack<string>();
        var current = t;
        while (current != null)
        {
            stack.Push(current.name);
            current = current.parent;
        }

        // シーン名を先頭に（任意）
        var sceneName = t.gameObject.scene.IsValid() ? t.gameObject.scene.name : "(no-scene)";
        return $"{sceneName}/" + string.Join("/", stack.ToArray());
    }
}
