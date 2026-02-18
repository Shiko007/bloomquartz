using UnityEngine;
using UnityEngine.EventSystems;
using Bloomquartz.Gems;

namespace Bloomquartz.Puzzle
{
    public class Tile : MonoBehaviour, IPointerClickHandler
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer gemRenderer;
        [SerializeField] private SpriteRenderer selectionRenderer;
        [SerializeField] private Sprite[] gemSprites;

        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public GemType GemType { get; private set; }

        private Vector3 _baseScale;
        private bool _isEmpty;

        public void Init(int x, int y, GemType gemType)
        {
            GridX = x;
            GridY = y;
            _baseScale = transform.localScale;
            SetGemType(gemType, animate: false);
            selectionRenderer.enabled = false;
        }

        public void SetGemType(GemType type, bool animate)
        {
            GemType = type;
            _isEmpty = false;

            int idx = (int)type;
            if (idx >= 0 && idx < gemSprites.Length)
                gemRenderer.sprite = gemSprites[idx];

            gemRenderer.enabled = true;

            if (animate)
                AnimatePop();
        }

        public void ClearTile()
        {
            _isEmpty = true;
            gemRenderer.enabled = false;
        }

        public bool IsEmpty() => _isEmpty;

        public void SetSelected(bool selected)
        {
            selectionRenderer.enabled = selected;
            if (selected)
                AnimateBounce();
        }

        private void AnimatePop()
        {
            StopAllCoroutines();
            StartCoroutine(ScalePop());
        }

        private void AnimateBounce()
        {
            StopAllCoroutines();
            StartCoroutine(ScaleBounce());
        }

        private System.Collections.IEnumerator ScalePop()
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.12f;
                float scale = 1f + 0.3f * Mathf.Sin(t * Mathf.PI);
                transform.localScale = _baseScale * scale;
                yield return null;
            }
            transform.localScale = _baseScale;
        }

        private System.Collections.IEnumerator ScaleBounce()
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.3f;
                float scale = 1f + 0.15f * Mathf.Sin(t * Mathf.PI * 2f);
                transform.localScale = _baseScale * scale;
                yield return null;
            }
            transform.localScale = _baseScale;
        }

        // IPointerClickHandler integrates with the EventSystem, so Unity
        // automatically suppresses this when a UI element sits above the tile.
        // This replaces OnMouseDown which fired before UI events were processed.
        public void OnPointerClick(PointerEventData _)
        {
            Board.Instance.OnTileSelected(this);
        }
    }
}
