using UnityEngine;
using OscJack;

public class JoyStickReceiver : MonoBehaviour
{
    private OscServer _server;
    
    [SerializeField] private int port = 9000;

    // 受け取りたいOSCアドレスごとに変数を用意する
    [Range(-1f, 1f)] public float p1_Y;
    [Range(-1f, 1f)] public float p1_X;
    [Range(-1f, 1f)] public float b6;
    [Range(-1f, 1f)] public float b5;
    [Range(-1f, 1f)] public float b4;
    [Range(-1f, 1f)] public float b3;
    [Range(-1f, 1f)] public float b2;
    [Range(-1f, 1f)] public float b1;

    [Range(-1f, 1f)] public float slider2;
    [Range(-1f, 1f)] public float slider1;

    [Range(-1f, 1f)] public float zrot;
    [Range(-1f, 1f)] public float yrot;
    [Range(-1f, 1f)] public float xrot;

    [Range(-1f, 1f)] public float zaxis;
    [Range(-1f, 1f)] public float yaxis;
    [Range(-1f, 1f)] public float xaxis;

    // サンプルレートは -1〜1 でないなら適宜変更
    [Range(0f, 120f)] public float samplerate; 

    void Start()
    {
        // サーバーを生成
        _server = new OscServer(port);

        // それぞれのアドレスを受け取るコールバックを登録

        // Stick系
        _server.MessageDispatcher.AddCallback(
            "/p1_Y",
            (address, data) => {
                p1_Y = data.GetElementAsFloat(0);
                // Debug.Log($"p1_Y: {p1_Y}");
            }
        );

        _server.MessageDispatcher.AddCallback(
            "/p1_X",
            (address, data) => {
                p1_X = data.GetElementAsFloat(0);
                // Debug.Log($"p1_X: {p1_X}");
            }
        );

        // ボタン系
        _server.MessageDispatcher.AddCallback(
            "/b6",
            (address, data) => {
                b6 = data.GetElementAsFloat(0);
                // Debug.Log($"b6: {b6}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/b5",
            (address, data) => {
                b5 = data.GetElementAsFloat(0);
                // Debug.Log($"b5: {b5}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/b4",
            (address, data) => {
                b4 = data.GetElementAsFloat(0);
                // Debug.Log($"b4: {b4}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/b3",
            (address, data) => {
                b3 = data.GetElementAsFloat(0);
                // Debug.Log($"b3: {b3}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/b2",
            (address, data) => {
                b2 = data.GetElementAsFloat(0);
                // Debug.Log($"b2: {b2}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/b1",
            (address, data) => {
                b1 = data.GetElementAsFloat(0);
                // Debug.Log($"b1: {b1}");
            }
        );

        // スライダー系
        _server.MessageDispatcher.AddCallback(
            "/slider2",
            (address, data) => {
                slider2 = data.GetElementAsFloat(0);
                // Debug.Log($"slider2: {slider2}");
            }
        );

        _server.MessageDispatcher.AddCallback(
            "/slider1",
            (address, data) => {
                slider1 = data.GetElementAsFloat(0);
                // Debug.Log($"slider1: {slider1}");
            }
        );

        // 回転系
        _server.MessageDispatcher.AddCallback(
            "/zrot",
            (address, data) => {
                zrot = data.GetElementAsFloat(0);
                // Debug.Log($"zrot: {zrot}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/yrot",
            (address, data) => {
                yrot = data.GetElementAsFloat(0);
                // Debug.Log($"yrot: {yrot}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/xrot",
            (address, data) => {
                xrot = data.GetElementAsFloat(0);
                // Debug.Log($"xrot: {xrot}");
            }
        );

        // 軸系
        _server.MessageDispatcher.AddCallback(
            "/zaxis",
            (address, data) => {
                zaxis = data.GetElementAsFloat(0);
                // Debug.Log($"zaxis: {zaxis}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/yaxis",
            (address, data) => {
                yaxis = data.GetElementAsFloat(0);
                // Debug.Log($"yaxis: {yaxis}");
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/xaxis",
            (address, data) => {
                xaxis = data.GetElementAsFloat(0);
                // Debug.Log($"xaxis: {xaxis}");
            }
        );

        // サンプルレート（もし使うなら）
        _server.MessageDispatcher.AddCallback(
            "/_samplerate",
            (address, data) => {
                samplerate = data.GetElementAsFloat(0);
                // Debug.Log($"_samplerate: {samplerate}");
            }
        );
    }

    void OnDestroy()
    {
        _server?.Dispose();
        _server = null;
    }
}
