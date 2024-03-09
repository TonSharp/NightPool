// ----------------------------------------------------------------------------
// The MIT License
// NightPool is an object pool for Unity https://github.com/MeeXaSiK/NightPool
// Copyright (c) 2021-2023 Night Train Code
// ----------------------------------------------------------------------------

using System;
using UnityEngine;
using Object = UnityEngine.Object;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace NTC.Pool
{
#if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
#endif
    internal sealed class Poolable
    {
        internal bool IsSetup;
        internal PoolableStatus Status;
        
        internal Transform Transform;
        internal GameObject GameObject;
        internal NightGameObjectPool Pool;

        internal void SetupAsDefault()
        {
#if DEBUG
            if (IsSetup)
                throw new Exception("Poolable is already setup!");
#endif
            NightPool.ClonesMap.Add(GameObject, this);
            Status = PoolableStatus.Despawned;
            IsSetup = true;
        }

        internal void SetupAsSpawnedOverCapacity()
        {
#if DEBUG
            if (IsSetup)
                throw new Exception("Poolable is already setup!");
#endif
            NightPool.ClonesMap.Add(GameObject, this);
            Status = PoolableStatus.SpawnedOverCapacity;
            IsSetup = true;
        }

        internal void Dispose(bool immediately)
        {
            NightPool.ClonesMap.Remove(GameObject);

            if (immediately)
                Object.DestroyImmediate(GameObject);
            else
                Object.Destroy(GameObject);
        }
    }
}