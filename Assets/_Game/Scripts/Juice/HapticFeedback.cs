using UnityEngine;

namespace Bloomquartz.Juice
{
    /// <summary>
    /// Wraps iOS haptic feedback. Safe no-op on non-iOS platforms.
    /// </summary>
    public static class HapticFeedback
    {
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerImpactLight();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerImpactMedium();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _TriggerImpactHeavy();
#endif

        public static void Light()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactLight();
#endif
        }

        public static void Medium()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactMedium();
#endif
        }

        public static void Heavy()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHeavy();
#endif
        }
    }
}
