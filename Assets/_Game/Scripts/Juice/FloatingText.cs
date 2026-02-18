using System.Collections;
using UnityEngine;
using TMPro;

namespace Bloomquartz.Juice
{
    /// Pooled floating score popup that rises and fades out.
    public class FloatingText : MonoBehaviour
    {
        private TextMeshPro _tmp;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
            if (_tmp == null)
            {
                _tmp              = gameObject.AddComponent<TextMeshPro>();
                _tmp.fontSize     = 5f;
                _tmp.alignment    = TextAlignmentOptions.Center;
                _tmp.fontStyle    = FontStyles.Bold;
                _tmp.sortingOrder = 50;  // well above sprites and tile backgrounds
            }
        }

        public void Spawn(Vector3 worldPos, string text, Color color)
        {
            // Slightly in front of all sprites (camera looks toward +Z in 2D)
            transform.position = new Vector3(worldPos.x, worldPos.y, -1f);
            _tmp.text  = text;
            _tmp.color = color;
            gameObject.SetActive(true);
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float t = 0f;
            Vector3 startPos = transform.position;
            Color   startCol = _tmp.color;

            while (t < 1f)
            {
                t += Time.deltaTime / 0.7f;
                float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 2f);
                transform.position = startPos + new Vector3(0, 1.2f * ease, 0);
                float alpha = t < 0.5f ? 1f : 1f - ((t - 0.5f) / 0.5f);
                _tmp.color = new Color(startCol.r, startCol.g, startCol.b, alpha);
                yield return null;
            }

            gameObject.SetActive(false);
            FloatingTextPool.Instance?.Return(this);
        }
    }

}
