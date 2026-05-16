// WindForVRM
// Source: https://github.com/malaybaku/WindForVRM
// Copyright 2019-2022 @baku_dreameater
// Licensed under the Apache License, Version 2.0.
// Local changes: moved into VLiveKit PerformerAct runtime and added null-safe reload support.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRM;

namespace WindForVRM
{
    /// <summary>
    /// Applies wind-like variation to VRM SpringBone gravity.
    /// </summary>
    public class VRMWind : MonoBehaviour
    {
        // A single wind impulse that rises, then fades out.
        class WindItem
        {
            public WindItem(Vector3 orientation, float riseCount, float sitCount, float maxFactor)
            {
                Orientation = orientation;
                RiseCount = riseCount;
                SitCount = sitCount;
                MaxFactor = maxFactor;

                TotalTime = RiseCount + SitCount;
            }

            public Vector3 Orientation { get; }
            public float RiseCount { get; }
            public float SitCount { get; }
            public float MaxFactor { get; }
            public float TotalTime { get; }
            public float TimeCount { get; set; }

            public float CurrentFactor =>
                TimeCount < RiseCount
                    ? MaxFactor * TimeCount / RiseCount
                    : MaxFactor * (1 - (TimeCount - RiseCount) / SitCount);
        }

        [Tooltip("Enable this when the component is attached directly to the VRM root and should initialize itself on Start.")]
        [SerializeField] private bool loadAutomatic = false;

        [Tooltip("Whether wind simulation is active.")]
        [SerializeField] private bool enableWind = true;

        [Tooltip("Base wind direction in world space.")]
        [SerializeField] private Vector3 windBaseOrientation = Vector3.right;

        [Tooltip("Random offset strength applied to the base wind direction.")]
        [SerializeField] private float windOrientationRandomPower = 0.2f;

        [SerializeField] private Vector2 windStrengthRange = new Vector2(0.03f, 0.06f);
        [SerializeField] private Vector2 windIntervalRange = new Vector2(0.7f, 1.9f);
        [SerializeField] private Vector2 windRiseCountRange = new Vector2(0.4f, 0.6f);
        [SerializeField] private Vector2 windSitCountRange = new Vector2(1.3f, 1.8f);

        [SerializeField] private float strengthFactor = 1.0f;
        [SerializeField] private float timeFactor = 1.0f;

        private float _windGenerateCount = 0;
        private VRMSpringBone[] _springBones = new VRMSpringBone[] { };
        private Vector3[] _originalGravityDirections = new Vector3[] { };
        private float[] _originalGravityFactors = new float[] { };
        private readonly List<WindItem> _windItems = new List<WindItem>();

        /// <summary>Gets or sets whether wind simulation is active.</summary>
        public bool EnableWind
        {
            get => enableWind;
            set
            {
                if (enableWind == value)
                {
                    return;
                }

                enableWind = value;
                if (!value)
                {
                    DisableWind();
                }
            }
        }

        /// <summary>Gets or sets the wind direction in world space.</summary>
        public Vector3 WindBaseOrientation
        {
            get => windBaseOrientation;
            set => windBaseOrientation = value;
        }

        /// <summary>Gets or sets the randomization strength applied to the wind direction.</summary>
        public float WindOrientationRandomPower
        {
            get => windOrientationRandomPower;
            set => windOrientationRandomPower = value;
        }

        /// <summary>Gets or sets the wind strength multiplier.</summary>
        public float StrengthFactor
        {
            get => strengthFactor;
            set => strengthFactor = value;
        }

        /// <summary>Gets or sets the wind generation interval multiplier.</summary>
        public float TimeFactor
        {
            get => timeFactor;
            set => timeFactor = value;
        }

        /// <summary>
        /// Loads VRM SpringBones from the target root.
        /// </summary>
        /// <param name="vrmRoot">The VRM root transform.</param>
        public void LoadVrm(Transform vrmRoot)
        {
            if (vrmRoot == null)
            {
                UnloadVrm();
                return;
            }

            _springBones = vrmRoot.GetComponentsInChildren<VRMSpringBone>(true);
            _originalGravityDirections = _springBones.Select(b => b.m_gravityDir).ToArray();
            _originalGravityFactors = _springBones.Select(b => b.m_gravityPower).ToArray();
        }

        /// <summary>
        /// Restores the current wind changes and reloads VRM SpringBones from the target root.
        /// </summary>
        /// <param name="vrmRoot">The VRM root transform.</param>
        public void ReloadVrm(Transform vrmRoot)
        {
            DisableWind();
            _windItems.Clear();
            _windGenerateCount = 0f;
            LoadVrm(vrmRoot);
        }

        /// <summary>
        /// Clears loaded VRM SpringBone references.
        /// </summary>
        public void UnloadVrm()
        {
            _springBones = new VRMSpringBone[] { };
            _originalGravityDirections = new Vector3[] { };
            _originalGravityFactors = new float[] { };
        }

        private void Start()
        {
            if (loadAutomatic)
            {
                LoadVrm(transform);
            }
        }

        private void Update()
        {
            if (!EnableWind)
            {
                return;
            }

            UpdateWindGenerateCount();
            UpdateWindItems();

            Vector3 windForce = Vector3.zero;
            for (int i = 0; i < _windItems.Count; i++)
            {
                windForce += _windItems[i].CurrentFactor * _windItems[i].Orientation;
            }

            for (int i = 0; i < _springBones.Length; i++)
            {
                var bone = _springBones[i];
                if (bone == null)
                {
                    continue;
                }

                var forceSum = _originalGravityFactors[i] * _originalGravityDirections[i] + windForce;
                bone.m_gravityDir = forceSum.normalized;
                bone.m_gravityPower = forceSum.magnitude;
            }
        }

        /// <summary>Restores SpringBone gravity settings to their loaded state.</summary>
        private void DisableWind()
        {
            var count = Mathf.Min(_springBones.Length, _originalGravityDirections.Length, _originalGravityFactors.Length);
            for (int i = 0; i < count; i++)
            {
                var bone = _springBones[i];
                if (bone == null)
                {
                    continue;
                }

                bone.m_gravityDir = _originalGravityDirections[i];
                bone.m_gravityPower = _originalGravityFactors[i];
            }
        }

        /// <summary>Counts time and creates new randomized wind impulses when needed.</summary>
        private void UpdateWindGenerateCount()
        {
            _windGenerateCount -= Time.deltaTime;
            if (_windGenerateCount > 0)
            {
                return;
            }

            _windGenerateCount = Random.Range(windIntervalRange.x, windIntervalRange.y) * timeFactor;

            var windOrientation = (
                windBaseOrientation.normalized +
                new Vector3(
                    Random.Range(-windOrientationRandomPower, windOrientationRandomPower),
                    Random.Range(-windOrientationRandomPower, windOrientationRandomPower),
                    Random.Range(-windOrientationRandomPower, windOrientationRandomPower)
                )).normalized;

            _windItems.Add(new WindItem(
                windOrientation,
                Random.Range(windRiseCountRange.x, windRiseCountRange.y),
                Random.Range(windSitCountRange.x, windSitCountRange.y),
                Random.Range(windStrengthRange.x, windStrengthRange.y) * strengthFactor
            ));
        }

        /// <summary>Updates active wind impulses and removes finished ones.</summary>
        private void UpdateWindItems()
        {
            for (int i = _windItems.Count - 1; i >= 0; i--)
            {
                var item = _windItems[i];
                item.TimeCount += Time.deltaTime;
                if (item.TimeCount >= item.TotalTime)
                {
                    _windItems.RemoveAt(i);
                }
            }
        }
    }
}
