#if UNITY_EDITOR
using UnityEditor;

namespace NTC.Pool.ExecutionOrder
{
    [InitializeOnLoad]
    internal sealed class NightPoolGlobalExecutionOrder
    {
        static NightPoolGlobalExecutionOrder()
        {
            var nightPoolType = typeof(NightPoolGlobal);

            foreach (var runtimeMonoScript in MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (runtimeMonoScript.GetClass() != nightPoolType)
                    continue;

                var currentExecutionOrder = MonoImporter.GetExecutionOrder(runtimeMonoScript);

                if (currentExecutionOrder != Constants.NightPoolExecutionOrder)
                    MonoImporter.SetExecutionOrder(runtimeMonoScript, Constants.NightPoolExecutionOrder);

                return;
            }
        }
    }
}
#endif