using System.Collections;
using UnityEngine;

namespace Bloomquartz.Juice
{
    public class CameraShaker : MonoBehaviour
    {
        public static CameraShaker Instance { get; private set; }

        private Vector3 _originPos;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            Instance = this;
            _originPos = transform.localPosition;
        }

        public void Shake(float duration = 0.2f, float magnitude = 0.12f)
        {
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float dampen   = 1f - Mathf.Pow(progress, 2f); // ease out
                float x = Random.Range(-1f, 1f) * magnitude * dampen;
                float y = Random.Range(-1f, 1f) * magnitude * dampen;
                transform.localPosition = _originPos + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = _originPos;
        }
    }
}
