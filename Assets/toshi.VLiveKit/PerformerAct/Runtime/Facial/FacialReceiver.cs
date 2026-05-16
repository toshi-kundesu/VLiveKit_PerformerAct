using OscJack;
using UnityEngine;

class FacialReceiver : MonoBehaviour
{
    public const int DefaultPort = PerformerOscPorts.LipSync;

    [Range(1, 65535)]
    public int port = DefaultPort;

    OscServer _server;
    int _boundPort;
    bool _callbacksRegistered;

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
        if (port <= 0 || port == 3940 || port == 4000)
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

        server.MessageDispatcher.AddCallback("/ulipsync/vowel/a", OnVowelA);
        server.MessageDispatcher.AddCallback("/ulipsync/vowel/i", OnVowelI);
        server.MessageDispatcher.AddCallback("/ulipsync/vowel/u", OnVowelU);
        server.MessageDispatcher.AddCallback("/ulipsync/vowel/e", OnVowelE);
        server.MessageDispatcher.AddCallback("/ulipsync/vowel/o", OnVowelO);
        _callbacksRegistered = true;
    }

    void UnregisterCallbacks(OscServer server)
    {
        if (server == null || !_callbacksRegistered)
        {
            return;
        }

        server.MessageDispatcher.RemoveCallback("/ulipsync/vowel/a", OnVowelA);
        server.MessageDispatcher.RemoveCallback("/ulipsync/vowel/i", OnVowelI);
        server.MessageDispatcher.RemoveCallback("/ulipsync/vowel/u", OnVowelU);
        server.MessageDispatcher.RemoveCallback("/ulipsync/vowel/e", OnVowelE);
        server.MessageDispatcher.RemoveCallback("/ulipsync/vowel/o", OnVowelO);
        _callbacksRegistered = false;
    }

    void OnVowelA(string address, OscDataHandle data) { valueA = data.GetElementAsFloat(0); }
    void OnVowelI(string address, OscDataHandle data) { valueI = data.GetElementAsFloat(0); }
    void OnVowelU(string address, OscDataHandle data) { valueU = data.GetElementAsFloat(0); }
    void OnVowelE(string address, OscDataHandle data) { valueE = data.GetElementAsFloat(0); }
    void OnVowelO(string address, OscDataHandle data) { valueO = data.GetElementAsFloat(0); }
}
