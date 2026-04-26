// VLiveKit is all Unlicense.
// unlicense: https://unlicense.org/
// this comment & namespace can be removed. you can use this code freely.

using UnityEngine;
using VRM;

// 「任意のキーを押すと、シーン内のVRMSpringBoneをSetup()して初期化」
namespace toshi.VLiveKit.Character
{

    public class VrmSpringBoneResetOnKey : MonoBehaviour
    {
        [Header("==任意のキーでVRMSpringBoneを初期化==")]

        // トグルがオンなら、キーでリセット可能にする
        [SerializeField]
        private KeyCode resetKey = KeyCode.C;
        [SerializeField]
        private bool isResetEnabled = true;

        void Update()
        {
            // Spaceキーなど指定のキーが押されたときに実行
            if (Input.GetKeyDown(resetKey) && isResetEnabled)
            {
                ResetVRMSpringBone();
            }
        }

        // ResetVRMSpringBone()を呼び出す
        public void ResetVRMSpringBone()
        {
            var allSpringBones = FindObjectsOfType<VRMSpringBone>();
            foreach (var springBone in allSpringBones)
            {
                springBone.Setup();
                Debug.Log($"VRMSpringBoneを {springBone.name} を初期化(Setup)しました。");
            }
        }
    }
}