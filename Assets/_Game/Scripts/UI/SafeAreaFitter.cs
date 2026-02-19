using UnityEngine;

namespace Bloomquartz.UI
{
    /// Resizes the RectTransform to match the device's safe area so that
    /// HUD elements are pushed below the Dynamic Island / notch automatically.
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private void Awake()
        {
            var rt         = GetComponent<RectTransform>();
            Rect safe      = Screen.safeArea;
            var  screen    = new Vector2(Screen.width, Screen.height);
            rt.anchorMin   = safe.position / screen;
            rt.anchorMax   = (safe.position + safe.size) / screen;
            rt.offsetMin   = Vector2.zero;
            rt.offsetMax   = Vector2.zero;
        }
    }
}
