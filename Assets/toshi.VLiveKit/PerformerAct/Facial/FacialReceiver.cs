using UnityEngine;
using OscJack;

class FacialReceiver : MonoBehaviour
{
    // ip address
    // public string ipAddress = "192.168.0.255";
    [Range(0, 10000)]
    public int port = 3940;

    OscServer _server;
    
    // 各母音の値を保持
    [Range(0, 100)]
    public float valueA = 0f;
    [Range(0, 100)]
    public float valueI = 0f;
    [Range(0, 100)]
    public float valueU = 0f;
    [Range(0, 100)]
    public float valueE = 0f;
    [Range(0, 100)]
    public float valueO = 0f;

    void Start()
    {
        _server = new OscServer(port); // Port number

        // 各母音のコールバックを登録
        _server.MessageDispatcher.AddCallback(
            "/ulipsync/vowel/a",
            (string address, OscDataHandle data) => {
                valueA = data.GetElementAsFloat(0);
                // Debug.Log($"A: {valueA:F3}");
            }
        );

        _server.MessageDispatcher.AddCallback(
            "/ulipsync/vowel/i",
            (string address, OscDataHandle data) => {
                valueI = data.GetElementAsFloat(0);
                // Debug.Log($"I: {valueI:F3}");
            }
        );

        _server.MessageDispatcher.AddCallback(
            "/ulipsync/vowel/u",
            (string address, OscDataHandle data) => {
                valueU = data.GetElementAsFloat(0);
                // Debug.Log($"U: {valueU:F3}");
            }
        );

        _server.MessageDispatcher.AddCallback(
            "/ulipsync/vowel/e",
            (string address, OscDataHandle data) => {
                valueE = data.GetElementAsFloat(0);
                // Debug.Log($"E: {valueE:F3}");
            }
        );

        _server.MessageDispatcher.AddCallback(
            "/ulipsync/vowel/o",
            (string address, OscDataHandle data) => {
                valueO = data.GetElementAsFloat(0);
                // Debug.Log($"O: {valueO:F3}");
            }
        );
    }

    // void OnGUI()
    // {
    //     // デバッグ表示
    //     GUILayout.BeginArea(new Rect(10, 10, 200, 150));
    //     GUILayout.BeginVertical("box");
    //     GUILayout.Label("Received Values:");
    //     GUILayout.Label($"A: {valueA:F3}");
    //     GUILayout.Label($"I: {valueI:F3}");
    //     GUILayout.Label($"U: {valueU:F3}");
    //     GUILayout.Label($"E: {valueE:F3}");
    //     GUILayout.Label($"O: {valueO:F3}");
    //     GUILayout.EndVertical();
    //     GUILayout.EndArea();
    // }

    void OnDestroy()
    {
        _server?.Dispose();
        _server = null;
    }
}