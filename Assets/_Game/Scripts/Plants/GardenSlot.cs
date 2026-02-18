using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Bloomquartz.UI;
using Bloomquartz.Audio;

namespace Bloomquartz.Plants
{
    public class GardenSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private SpriteRenderer plantRenderer;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private TextMeshPro gemReadyLabel;

        private bool _hasPlant;
        private bool _gemReady;
        private float _pulseTimer;
        private int _evolutionLevel;

        public int GetSlotIndex() => slotIndex;

        public void SetHasPlant(bool hasPlant)
        {
            _hasPlant = hasPlant;
            if (glowRenderer != null)
                glowRenderer.color = hasPlant
                    ? new Color(0.4f, 1f, 0.5f, 0.3f)
                    : new Color(0.5f, 0.5f, 0.5f, 0.15f);
        }

        public void SetGemReady(bool ready)
        {
            _gemReady = ready;
            if (gemReadyLabel != null)
                gemReadyLabel.gameObject.SetActive(ready);
        }

        public const int MaxEvolutionLevel = 4;

        public bool CanEvolve() => _hasPlant && _evolutionLevel < MaxEvolutionLevel;

        public void TriggerEvolve()
        {
            if (!CanEvolve()) return;
            _evolutionLevel++;
            AudioManager.Instance?.PlaySFX("evolution");
            // Brighten glow color per evolution level
            if (glowRenderer != null)
            {
                Color[] evolveColors = {
                    new Color(0.4f, 1f,   0.5f, 0.4f),
                    new Color(0.4f, 0.8f, 1f,   0.5f),
                    new Color(1f,   0.8f, 0.2f, 0.6f),
                    new Color(1f,   0.4f, 1f,   0.7f),
                };
                int idx = Mathf.Clamp(_evolutionLevel - 1, 0, evolveColors.Length - 1);
                glowRenderer.color = evolveColors[idx];
            }
            StartCoroutine(EvolveScalePop());
        }

        private System.Collections.IEnumerator EvolveScalePop()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.4f;
                float s = 1f + 0.35f * Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (!_hasPlant) return;
            _pulseTimer += Time.deltaTime;
            float scale = 1f + 0.04f * Mathf.Sin(_pulseTimer * 2f);
            transform.localScale = Vector3.one * scale;
        }

        public void OnPointerClick(PointerEventData _)
        {
            GardenUI.Instance?.OnSlotTapped(slotIndex, _hasPlant);
            Juice.HapticFeedback.Light();
        }
    }
}
