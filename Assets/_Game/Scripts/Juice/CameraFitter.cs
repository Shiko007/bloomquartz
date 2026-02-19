using UnityEngine;

namespace Bloomquartz.Juice
{
    /// Adjusts the camera's orthographic size at startup so the specified
    /// world half-extents are fully visible regardless of device aspect ratio.
    [RequireComponent(typeof(Camera))]
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private float worldHalfWidth  = 4f;
        [SerializeField] private float worldHalfHeight = 3f;
        [SerializeField] [Range(1f, 1.5f)] private float padding = 1.1f;

        private void Awake()
        {
            var cam = GetComponent<Camera>();
            if (cam == null || !cam.orthographic) return;

            float aspect   = (float)Screen.width / Screen.height;
            float sizeForW = worldHalfWidth  / aspect;
            float sizeForH = worldHalfHeight;

            cam.orthographicSize = Mathf.Max(sizeForW, sizeForH) * padding;
        }
    }
}
