using OscJack;
using UnityEngine;

public class JoyStickReceiver : MonoBehaviour
{
    public const int DefaultPort = PerformerOscPorts.Expression;

    [SerializeField, Range(1, 65535)] private int port = DefaultPort;

    OscServer _server;
    int _boundPort;
    bool _callbacksRegistered;

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

    [Range(0f, 120f)] public float samplerate;

    public int Port
    {
        get => port;
        set => port = PerformerOscPorts.Normalize(value, DefaultPort);
    }

    void Reset()
    {
        port = DefaultPort;
    }

    void OnValidate()
    {
        UseDefaultPortIfLegacy();
    }

    public void UseDefaultPortIfLegacy()
    {
        if (port <= 0 || port == 4000 || port == 9000)
        {
            port = DefaultPort;
        }

        port = PerformerOscPorts.Normalize(port, DefaultPort);
    }

    void OnEnable()
    {
        if (_server != null)
        {
            return;
        }

        UseDefaultPortIfLegacy();

        if (!PerformerOscServerRegistry.TryAcquire(this, port, out _server))
        {
            return;
        }

        _boundPort = port;
        RegisterCallbacks(_server);
    }

    void OnDisable()
    {
        UnregisterCallbacks(_server);
        PerformerOscServerRegistry.Release(_boundPort, _server);
        _server = null;
        _boundPort = 0;
    }

    void OnDestroy()
    {
        OnDisable();
    }

    void RegisterCallbacks(OscServer server)
    {
        if (server == null || _callbacksRegistered)
        {
            return;
        }

        server.MessageDispatcher.AddCallback("/p1_Y", OnP1Y);
        server.MessageDispatcher.AddCallback("/p1_X", OnP1X);
        server.MessageDispatcher.AddCallback("/b6", OnB6);
        server.MessageDispatcher.AddCallback("/b5", OnB5);
        server.MessageDispatcher.AddCallback("/b4", OnB4);
        server.MessageDispatcher.AddCallback("/b3", OnB3);
        server.MessageDispatcher.AddCallback("/b2", OnB2);
        server.MessageDispatcher.AddCallback("/b1", OnB1);
        server.MessageDispatcher.AddCallback("/slider2", OnSlider2);
        server.MessageDispatcher.AddCallback("/slider1", OnSlider1);
        server.MessageDispatcher.AddCallback("/zrot", OnZRot);
        server.MessageDispatcher.AddCallback("/yrot", OnYRot);
        server.MessageDispatcher.AddCallback("/xrot", OnXRot);
        server.MessageDispatcher.AddCallback("/zaxis", OnZAxis);
        server.MessageDispatcher.AddCallback("/yaxis", OnYAxis);
        server.MessageDispatcher.AddCallback("/xaxis", OnXAxis);
        server.MessageDispatcher.AddCallback("/_samplerate", OnSampleRate);
        _callbacksRegistered = true;
    }

    void UnregisterCallbacks(OscServer server)
    {
        if (server == null || !_callbacksRegistered)
        {
            return;
        }

        server.MessageDispatcher.RemoveCallback("/p1_Y", OnP1Y);
        server.MessageDispatcher.RemoveCallback("/p1_X", OnP1X);
        server.MessageDispatcher.RemoveCallback("/b6", OnB6);
        server.MessageDispatcher.RemoveCallback("/b5", OnB5);
        server.MessageDispatcher.RemoveCallback("/b4", OnB4);
        server.MessageDispatcher.RemoveCallback("/b3", OnB3);
        server.MessageDispatcher.RemoveCallback("/b2", OnB2);
        server.MessageDispatcher.RemoveCallback("/b1", OnB1);
        server.MessageDispatcher.RemoveCallback("/slider2", OnSlider2);
        server.MessageDispatcher.RemoveCallback("/slider1", OnSlider1);
        server.MessageDispatcher.RemoveCallback("/zrot", OnZRot);
        server.MessageDispatcher.RemoveCallback("/yrot", OnYRot);
        server.MessageDispatcher.RemoveCallback("/xrot", OnXRot);
        server.MessageDispatcher.RemoveCallback("/zaxis", OnZAxis);
        server.MessageDispatcher.RemoveCallback("/yaxis", OnYAxis);
        server.MessageDispatcher.RemoveCallback("/xaxis", OnXAxis);
        server.MessageDispatcher.RemoveCallback("/_samplerate", OnSampleRate);
        _callbacksRegistered = false;
    }

    void OnP1Y(string address, OscDataHandle data) { p1_Y = data.GetElementAsFloat(0); }
    void OnP1X(string address, OscDataHandle data) { p1_X = data.GetElementAsFloat(0); }
    void OnB6(string address, OscDataHandle data) { b6 = data.GetElementAsFloat(0); }
    void OnB5(string address, OscDataHandle data) { b5 = data.GetElementAsFloat(0); }
    void OnB4(string address, OscDataHandle data) { b4 = data.GetElementAsFloat(0); }
    void OnB3(string address, OscDataHandle data) { b3 = data.GetElementAsFloat(0); }
    void OnB2(string address, OscDataHandle data) { b2 = data.GetElementAsFloat(0); }
    void OnB1(string address, OscDataHandle data) { b1 = data.GetElementAsFloat(0); }
    void OnSlider2(string address, OscDataHandle data) { slider2 = data.GetElementAsFloat(0); }
    void OnSlider1(string address, OscDataHandle data) { slider1 = data.GetElementAsFloat(0); }
    void OnZRot(string address, OscDataHandle data) { zrot = data.GetElementAsFloat(0); }
    void OnYRot(string address, OscDataHandle data) { yrot = data.GetElementAsFloat(0); }
    void OnXRot(string address, OscDataHandle data) { xrot = data.GetElementAsFloat(0); }
    void OnZAxis(string address, OscDataHandle data) { zaxis = data.GetElementAsFloat(0); }
    void OnYAxis(string address, OscDataHandle data) { yaxis = data.GetElementAsFloat(0); }
    void OnXAxis(string address, OscDataHandle data) { xaxis = data.GetElementAsFloat(0); }
    void OnSampleRate(string address, OscDataHandle data) { samplerate = data.GetElementAsFloat(0); }
}
