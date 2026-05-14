using System;
using System.Net;
using OscJack;
using UnityEditor;
using UnityEngine;

namespace VLiveKit.PerformerAct.Editor
{
    public sealed class PerformerOscTestSignalWindow : EditorWindow
    {
        enum LipPattern
        {
            Manual,
            AeiouLoop,
            TalkPulse
        }

        const string LipAddressA = "/ulipsync/vowel/a";
        const string LipAddressI = "/ulipsync/vowel/i";
        const string LipAddressU = "/ulipsync/vowel/u";
        const string LipAddressE = "/ulipsync/vowel/e";
        const string LipAddressO = "/ulipsync/vowel/o";

        static readonly string[] ExpressionAddresses =
        {
            "/b1", "/b2", "/b3", "/b4", "/b5", "/b6",
            "/p1_X", "/p1_Y", "/slider1", "/slider2", "/xaxis", "/yaxis", "/zaxis",
            "/xrot", "/yrot", "/zrot", "/_samplerate"
        };

        [SerializeField] string destinationAddress = "127.0.0.1";
        [SerializeField] int lipSyncPort = 3940;
        [SerializeField] int expressionPort = 9000;
        [SerializeField] bool sendLipSync = true;
        [SerializeField] bool sendExpression = true;
        [SerializeField] int rateFps = 30;
        [SerializeField] LipPattern lipPattern = LipPattern.AeiouLoop;
        [SerializeField, Range(0f, 100f)] float lipPeak = 80f;
        [SerializeField, Range(0.1f, 8f)] float lipSpeed = 2.4f;
        [SerializeField] bool sendZeroOnStop = true;

        [SerializeField] float valueA;
        [SerializeField] float valueI;
        [SerializeField] float valueU;
        [SerializeField] float valueE;
        [SerializeField] float valueO;

        [SerializeField] float p1X;
        [SerializeField] float p1Y;
        [SerializeField] float slider1;
        [SerializeField] float slider2;
        [SerializeField] float xAxis;
        [SerializeField] float yAxis;
        [SerializeField] float zAxis;
        [SerializeField] float xRot;
        [SerializeField] float yRot;
        [SerializeField] float zRot;
        [SerializeField] float sampleRate = 30f;

        readonly float[] buttonValues = new float[6];

        OscClient lipClient;
        OscClient expressionClient;
        string lipClientKey;
        string expressionClientKey;
        bool sending;
        double nextSendTime;
        string status = "Stopped.";
        Vector2 scroll;

        [MenuItem("toshi/VLiveKit/Performer Act/OSC Test Signal")]
        public static void Open()
        {
            GetWindow<PerformerOscTestSignalWindow>("Performer OSC Test");
        }

        void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            StopSending(sendZeroOnStop);
            DisposeClients();
        }

        void OnGUI()
        {
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = scrollScope.scrollPosition;

                EditorGUILayout.LabelField("Destination", EditorStyles.boldLabel);
                destinationAddress = EditorGUILayout.TextField("Address", destinationAddress);
                using (new EditorGUILayout.HorizontalScope())
                {
                    lipSyncPort = EditorGUILayout.IntField("LipSync Port", lipSyncPort);
                    expressionPort = EditorGUILayout.IntField("Expression Port", expressionPort);
                }

                sendLipSync = EditorGUILayout.ToggleLeft("Send lip sync vowels to FacialReceiver", sendLipSync);
                sendExpression = EditorGUILayout.ToggleLeft("Send expression controls to JoyStickReceiver", sendExpression);
                rateFps = EditorGUILayout.IntSlider("Rate (fps)", rateFps, 1, 120);
                sendZeroOnStop = EditorGUILayout.ToggleLeft("Send zero values on stop", sendZeroOnStop);

                EditorGUILayout.Space(8f);
                DrawTransport();

                EditorGUILayout.Space(10f);
                DrawLipSync();

                EditorGUILayout.Space(10f);
                DrawExpressionControls();

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(status, EditorStyles.helpBox, GUILayout.Height(34f));
            }
        }

        void DrawTransport()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var stateText = sending ? "SENDING" : "STOPPED";
                var stateStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                EditorGUILayout.LabelField(stateText, stateStyle);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(sending))
                    {
                        if (GUILayout.Button("Start", GUILayout.Height(28f)))
                            StartSending();
                    }

                    using (new EditorGUI.DisabledScope(!sending))
                    {
                        if (GUILayout.Button("Stop", GUILayout.Height(28f)))
                            StopSending(sendZeroOnStop);
                    }

                    if (GUILayout.Button("Send Once", GUILayout.Height(28f)))
                        SendCurrentValues();

                    if (GUILayout.Button("Zero", GUILayout.Height(28f)))
                        SendZeroValues();
                }
            }
        }

        void DrawLipSync()
        {
            EditorGUILayout.LabelField("Lip Sync", EditorStyles.boldLabel);
            lipPattern = (LipPattern)EditorGUILayout.EnumPopup("Pattern", lipPattern);
            lipPeak = EditorGUILayout.Slider("Peak", lipPeak, 0f, 100f);
            lipSpeed = EditorGUILayout.Slider("Speed", lipSpeed, 0.1f, 8f);

            valueA = EditorGUILayout.Slider("A", valueA, 0f, 100f);
            valueI = EditorGUILayout.Slider("I", valueI, 0f, 100f);
            valueU = EditorGUILayout.Slider("U", valueU, 0f, 100f);
            valueE = EditorGUILayout.Slider("E", valueE, 0f, 100f);
            valueO = EditorGUILayout.Slider("O", valueO, 0f, 100f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("A")) SetOneVowel(0);
                if (GUILayout.Button("I")) SetOneVowel(1);
                if (GUILayout.Button("U")) SetOneVowel(2);
                if (GUILayout.Button("E")) SetOneVowel(3);
                if (GUILayout.Button("O")) SetOneVowel(4);
                if (GUILayout.Button("Rest")) SetLipValues(0f, 0f, 0f, 0f, 0f);
            }
        }

        void DrawExpressionControls()
        {
            EditorGUILayout.LabelField("Expression Controls", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Buttons", EditorStyles.miniBoldLabel);

            for (var i = 0; i < buttonValues.Length; i++)
                buttonValues[i] = EditorGUILayout.Slider("B" + (i + 1), buttonValues[i], 0f, 1f);

            using (new EditorGUILayout.HorizontalScope())
            {
                for (var i = 0; i < buttonValues.Length; i++)
                {
                    if (GUILayout.Button("Set B" + (i + 1)))
                    {
                        SetOnlyButton(i);
                        SendCurrentValues();
                    }
                }
            }

            if (GUILayout.Button("Clear Buttons"))
                Array.Clear(buttonValues, 0, buttonValues.Length);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Axes", EditorStyles.miniBoldLabel);
            p1X = EditorGUILayout.Slider("P1 X", p1X, -1f, 1f);
            p1Y = EditorGUILayout.Slider("P1 Y", p1Y, -1f, 1f);
            slider1 = EditorGUILayout.Slider("Slider 1", slider1, -1f, 1f);
            slider2 = EditorGUILayout.Slider("Slider 2", slider2, -1f, 1f);
            xAxis = EditorGUILayout.Slider("X Axis", xAxis, -1f, 1f);
            yAxis = EditorGUILayout.Slider("Y Axis", yAxis, -1f, 1f);
            zAxis = EditorGUILayout.Slider("Z Axis", zAxis, -1f, 1f);
            xRot = EditorGUILayout.Slider("X Rot", xRot, -1f, 1f);
            yRot = EditorGUILayout.Slider("Y Rot", yRot, -1f, 1f);
            zRot = EditorGUILayout.Slider("Z Rot", zRot, -1f, 1f);
            sampleRate = EditorGUILayout.Slider("Sample Rate", sampleRate, 0f, 120f);
        }

        void OnEditorUpdate()
        {
            if (!sending)
                return;

            var now = EditorApplication.timeSinceStartup;
            if (now < nextSendTime)
                return;

            nextSendTime = now + 1.0 / Mathf.Max(1, rateFps);
            UpdatePattern(now);
            SendCurrentValues();
            Repaint();
        }

        void StartSending()
        {
            if (!ValidateDestination())
                return;

            sending = true;
            nextSendTime = 0.0;
            status = "Sending to " + destinationAddress + " (lip:" + lipSyncPort + ", expression:" + expressionPort + ").";
        }

        void StopSending(bool sendZero)
        {
            if (sendZero)
                SendZeroValues();

            sending = false;
            status = "Stopped.";
        }

        void SendCurrentValues()
        {
            if (!ValidateDestination())
                return;

            try
            {
                if (sendLipSync)
                    SendLipSyncValues();

                if (sendExpression)
                    SendExpressionValues();

                status = "Last send: " + DateTime.Now.ToString("HH:mm:ss") + " to " + destinationAddress + ".";
            }
            catch (Exception exception)
            {
                DisposeClients();
                sending = false;
                status = "OSC send failed: " + exception.Message;
            }
        }

        void SendLipSyncValues()
        {
            var client = GetLipClient();
            client.Send(LipAddressA, valueA);
            client.Send(LipAddressI, valueI);
            client.Send(LipAddressU, valueU);
            client.Send(LipAddressE, valueE);
            client.Send(LipAddressO, valueO);
        }

        void SendExpressionValues()
        {
            var client = GetExpressionClient();
            for (var i = 0; i < buttonValues.Length; i++)
                client.Send(ExpressionAddresses[i], buttonValues[i]);

            client.Send("/p1_X", p1X);
            client.Send("/p1_Y", p1Y);
            client.Send("/slider1", slider1);
            client.Send("/slider2", slider2);
            client.Send("/xaxis", xAxis);
            client.Send("/yaxis", yAxis);
            client.Send("/zaxis", zAxis);
            client.Send("/xrot", xRot);
            client.Send("/yrot", yRot);
            client.Send("/zrot", zRot);
            client.Send("/_samplerate", sampleRate);
        }

        void SendZeroValues()
        {
            SetLipValues(0f, 0f, 0f, 0f, 0f);
            Array.Clear(buttonValues, 0, buttonValues.Length);
            p1X = p1Y = slider1 = slider2 = xAxis = yAxis = zAxis = xRot = yRot = zRot = 0f;
            SendCurrentValues();
        }

        void UpdatePattern(double now)
        {
            if (lipPattern == LipPattern.Manual)
                return;

            if (lipPattern == LipPattern.AeiouLoop)
            {
                var phase = (float)(now * lipSpeed);
                var vowelIndex = Mathf.FloorToInt(phase) % 5;
                var local = phase - Mathf.Floor(phase);
                var value = Mathf.SmoothStep(0f, lipPeak, Mathf.Sin(local * Mathf.PI));
                SetOneVowel(vowelIndex, value);
                return;
            }

            var t = (float)(now * lipSpeed);
            SetLipValues(
                Mathf.Abs(Mathf.Sin(t * 1.1f)) * lipPeak,
                Mathf.Abs(Mathf.Sin(t * 1.7f + 1.4f)) * lipPeak * 0.65f,
                Mathf.Abs(Mathf.Sin(t * 1.3f + 2.2f)) * lipPeak * 0.45f,
                Mathf.Abs(Mathf.Sin(t * 1.9f + 0.7f)) * lipPeak * 0.55f,
                Mathf.Abs(Mathf.Sin(t * 1.5f + 2.8f)) * lipPeak * 0.5f);
        }

        void SetOneVowel(int index)
        {
            SetOneVowel(index, lipPeak);
        }

        void SetOneVowel(int index, float value)
        {
            SetLipValues(
                index == 0 ? value : 0f,
                index == 1 ? value : 0f,
                index == 2 ? value : 0f,
                index == 3 ? value : 0f,
                index == 4 ? value : 0f);
        }

        void SetLipValues(float a, float i, float u, float e, float o)
        {
            valueA = Mathf.Clamp(a, 0f, 100f);
            valueI = Mathf.Clamp(i, 0f, 100f);
            valueU = Mathf.Clamp(u, 0f, 100f);
            valueE = Mathf.Clamp(e, 0f, 100f);
            valueO = Mathf.Clamp(o, 0f, 100f);
        }

        void SetOnlyButton(int index)
        {
            Array.Clear(buttonValues, 0, buttonValues.Length);
            if (index >= 0 && index < buttonValues.Length)
                buttonValues[index] = 1f;
        }

        bool ValidateDestination()
        {
            if (IPAddress.TryParse(destinationAddress, out _))
                return true;

            status = "Address must be an IPv4 address such as 127.0.0.1.";
            sending = false;
            return false;
        }

        OscClient GetLipClient()
        {
            var key = destinationAddress + ":" + lipSyncPort;
            if (lipClient != null && lipClientKey == key)
                return lipClient;

            if (lipClient != null)
                lipClient.Dispose();

            lipClient = new OscClient(destinationAddress, lipSyncPort);
            lipClientKey = key;
            return lipClient;
        }

        OscClient GetExpressionClient()
        {
            var key = destinationAddress + ":" + expressionPort;
            if (expressionClient != null && expressionClientKey == key)
                return expressionClient;

            if (expressionClient != null)
                expressionClient.Dispose();

            expressionClient = new OscClient(destinationAddress, expressionPort);
            expressionClientKey = key;
            return expressionClient;
        }

        void DisposeClients()
        {
            if (lipClient != null)
            {
                lipClient.Dispose();
                lipClient = null;
            }

            if (expressionClient != null)
            {
                expressionClient.Dispose();
                expressionClient = null;
            }

            lipClientKey = null;
            expressionClientKey = null;
        }
    }
}
