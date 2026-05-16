using System.Collections.Generic;
using System.Net.Sockets;
using OscJack;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public static class PerformerOscPorts
{
    public const int LipSync = 39580;
    public const int Expression = 39581;

    public static int Normalize(int port, int fallback)
    {
        return port >= 1 && port <= 65535 ? port : fallback;
    }
}

internal static class PerformerOscServerRegistry
{
    private sealed class Entry
    {
        public OscServer Server;
        public int RefCount;
    }

    private static readonly Dictionary<int, Entry> Servers = new Dictionary<int, Entry>();

    public static bool TryAcquire(MonoBehaviour owner, int port, out OscServer server)
    {
        server = null;

        if (!ShouldRunReceiver(owner))
        {
            return false;
        }

        if (Servers.TryGetValue(port, out var existing))
        {
            existing.RefCount++;
            server = existing.Server;
            return true;
        }

        try
        {
            server = new OscServer(port);
            Servers[port] = new Entry
            {
                Server = server,
                RefCount = 1
            };
            return true;
        }
        catch (SocketException exception)
        {
            Debug.Log(owner.GetType().Name + " could not listen on OSC port " + port + ". Another app or receiver is already using it. " + exception.Message, owner);
            return false;
        }
    }

    public static void Release(int port, OscServer server)
    {
        if (server == null)
        {
            return;
        }

        if (!Servers.TryGetValue(port, out var entry) || entry.Server != server)
        {
            server.Dispose();
            return;
        }

        entry.RefCount--;
        if (entry.RefCount > 0)
        {
            return;
        }

        Servers.Remove(port);
        server.Dispose();
    }

    private static bool ShouldRunReceiver(MonoBehaviour owner)
    {
        if (owner == null || owner.gameObject == null)
        {
            return false;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying || EditorSceneManager.IsPreviewScene(owner.gameObject.scene))
        {
            return false;
        }
#endif

        return true;
    }
}
