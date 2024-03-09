// ----------------------------------------------------------------------------
// The MIT License
// NightPool is an object pool for Unity https://github.com/MeeXaSiK/NightPool
// Copyright (c) 2021-2023 Night Train Code
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NTC.Pool
{
#if UNITY_EDITOR
    [DisallowMultipleComponent]
    [HelpURL(Constants.HelpUrl)]
    [AddComponentMenu(Constants.NightPoolComponentPath + "Night Pool Global")]
#endif
    public sealed class NightPoolGlobal : MonoBehaviour
    {
        [Header("Main")] [Tooltip(Constants.Tooltips.GlobalUpdateType)] [SerializeField]
        private UpdateType _updateType = UpdateType.Update;

        [FormerlySerializedAs("preloadPoolsType")] [Header("Preload Pools")] [Tooltip(Constants.Tooltips.GlobalPreloadType)] [SerializeField]
        private PreloadType _preloadPoolsType = PreloadType.Disabled;

        [FormerlySerializedAs("poolsPreset")] [Tooltip(Constants.Tooltips.PoolsToPreload)] [SerializeField]
        private List<PoolsPreset> _poolsPreset;

        [Header("Global Pool Settings")] [Tooltip(Constants.Tooltips.OverflowBehaviour)] [SerializeField]
        internal BehaviourOnCapacityReached _behaviourOnCapacityReached = Constants.BehaviourOnCapacityReached;

        [Tooltip(Constants.Tooltips.DespawnTypeTooltip)] [SerializeField]
        internal DespawnType _despawnType = Constants.DespawnType;

        [Tooltip(Constants.Tooltips.CallbacksTypeTooltip)] [SerializeField]
        internal CallbacksType _callbacksType = Constants.CallbacksType;

        [Tooltip(Constants.Tooltips.Capacity)] [SerializeField] [Min(0)]
        internal int _capacity = 64;

        [Tooltip(Constants.Tooltips.Persistent)] [SerializeField]
        internal bool _dontDestroyOnLoad;

        [Tooltip(Constants.Tooltips.Warnings)] [SerializeField]
        internal bool _sendWarnings = true;

        [Header("Safety")] [Tooltip(Constants.Tooltips.NightPoolModeTooltip)] [SerializeField]
        internal NightPoolMode _nightPoolMode = Constants.NightPoolMode;

        [Tooltip(Constants.Tooltips.DelayedDespawnReaction)] [SerializeField]
        internal ReactionOnRepeatedDelayedDespawn _reactionOnRepeatedDelayedDespawn =
            Constants.DelayedDespawnHandleType;

        [Tooltip(Constants.Tooltips.DespawnPersistentClonesOnDestroy)] [SerializeField]
        private bool _despawnPersistentClonesOnDestroy = true;

        [Tooltip(Constants.Tooltips.CheckClonesForNull)] [SerializeField]
        private bool _checkClonesForNull = true;

        [Tooltip(Constants.Tooltips.CheckForPrefab)] [SerializeField]
        private bool _checkForPrefab = true;

        [Tooltip(Constants.Tooltips.ClearEventsOnDestroy)] [SerializeField]
        private bool _clearEventsOnDestroy;

        private void Awake()
        {
            Initialize();
            PreloadPools(PreloadType.OnAwake);
        }

        private void Start() => PreloadPools(PreloadType.OnStart);

        private void Update()
        {
            if (_updateType == UpdateType.Update)
                HandleDespawnRequests(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_updateType == UpdateType.FixedUpdate)
                HandleDespawnRequests(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            if (_updateType == UpdateType.LateUpdate)
                HandleDespawnRequests(Time.deltaTime);
        }

        private void OnDestroy()
        {
            NightPool.ResetPool();

            if (_clearEventsOnDestroy || NightPool.IsApplicationQuitting)
                NightPool.GameObjectInstantiated.Clear();
        }

        private void OnApplicationQuit() => NightPool.IsApplicationQuitting = true;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;
            
            NightPool.NightPoolMode = _nightPoolMode;
            NightPool.CheckForPrefab = _checkForPrefab;
            NightPool.CheckClonesForNull = _checkClonesForNull;
            NightPool.DespawnPersistentClonesOnDestroy = _despawnPersistentClonesOnDestroy;
        }
#endif

        private void Initialize()
        {
#if DEBUG
            if (NightPool.Instance != null && NightPool.Instance != this)
                throw new Exception($"The number of {nameof(NightPool)} instances in the scene is greater than one!");

            if (enabled == false)
                Debug.LogWarning($"The <{nameof(NightPoolGlobal)}> instance is disabled! " +
                                 "Some functions may not work because of this!", this);
#endif
            NightPool.IsApplicationQuitting = false;
            NightPool.Instance = this;
            NightPool.HasTheNightPoolInitialized = true;
            NightPool.NightPoolMode = _nightPoolMode;
            NightPool.CheckForPrefab = _checkForPrefab;
            NightPool.CheckClonesForNull = _checkClonesForNull;
            NightPool.DespawnPersistentClonesOnDestroy = _despawnPersistentClonesOnDestroy;
        }

        private void PreloadPools(PreloadType requiredType)
        {
            if (requiredType != _preloadPoolsType)
                return;

            foreach (var preset in _poolsPreset)
                NightPool.InstallPools(preset);
        }

        private void HandleDespawnRequests(float deltaTime)
        {
            for (var i = 0; i < NightPool.DespawnRequests.Count; i++)
            {
                ref var request = ref NightPool.DespawnRequests.Components[i];

                if (request.Poolable.Status == PoolableStatus.Despawned)
                {
                    NightPool.DespawnRequests.RemoveUnorderedAt(i);
                    continue;
                }

                request.TimeToDespawn -= deltaTime;

                if (!(request.TimeToDespawn <= 0f))
                    continue;
                
                NightPool.DespawnImmediate(request.Poolable);
                NightPool.DespawnRequests.RemoveUnorderedAt(i);
            }
        }
    }
}