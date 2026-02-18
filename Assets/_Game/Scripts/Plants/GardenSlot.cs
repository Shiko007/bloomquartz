using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Bloomquartz.UI;
using Bloomquartz.Audio;
using Bloomquartz.Core;
using Bloomquartz.Juice;

namespace Bloomquartz.Plants
{
    public class GardenSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private SpriteRenderer plantRenderer;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private TextMeshPro gemReadyLabel;

        // Seconds to produce one gem per evolution level (0-4)
        private static readonly float[] ProductionRates = { 30f, 22f, 15f, 10f, 6f };

        // One colour per evolution level (0 = unevolve, 1-4 = evolved stages)
        private static readonly Color[] EvolveColors =
        {
            new Color(0.4f, 1f,   0.5f, 0.3f),  // level 0 — base green
            new Color(0.4f, 1f,   0.5f, 0.4f),  // level 1
            new Color(0.4f, 0.8f, 1f,   0.5f),  // level 2 — blue
            new Color(1f,   0.8f, 0.2f, 0.6f),  // level 3 — gold
            new Color(1f,   0.4f, 1f,   0.7f),  // level 4 — purple
        };

        private bool  _hasPlant;
        private bool  _gemReady;
        private float _pulseTimer;
        private int   _evolutionLevel;
        private float _productionTimer;
        private int   _pendingGems;

        public int  GetSlotIndex()      => slotIndex;
        public int  GetEvolutionLevel() => _evolutionLevel;

        // ── Returns the glow colour matching the current evolution level ──────
        private Color EvolutionGlowColor() =>
            EvolveColors[Mathf.Clamp(_evolutionLevel, 0, EvolveColors.Length - 1)];

        // ── Called by PlantGarden.LoadGarden to restore persisted state ───────
        public void SetEvolutionLevel(int level)
        {
            _evolutionLevel = Mathf.Clamp(level, 0, MaxEvolutionLevel);
            if (glowRenderer != null && _hasPlant)
                glowRenderer.color = EvolutionGlowColor();
        }

        public void SetHasPlant(bool hasPlant)
        {
            _hasPlant        = hasPlant;
            _productionTimer = 0f;
            if (glowRenderer != null)
                glowRenderer.color = hasPlant
                    ? EvolutionGlowColor()
                    : new Color(0.5f, 0.5f, 0.5f, 0.15f);
        }

        public void SetGemReady(bool ready)
        {
            _gemReady = ready;
            if (glowRenderer != null && _hasPlant)
                // Gold pulse while gems wait; restore evolution colour when collected
                glowRenderer.color = ready
                    ? new Color(1f, 0.88f, 0.1f, 0.7f)
                    : EvolutionGlowColor();
            if (gemReadyLabel != null)
                gemReadyLabel.gameObject.SetActive(ready);
        }

        /// Collect pending gems; returns how many were collected.
        public int CollectGems()
        {
            int gems     = _pendingGems;
            _pendingGems = 0;
            _productionTimer = 0f;
            SetGemReady(false);
            return gems;
        }

        public const int MaxEvolutionLevel = 4;
        public bool CanEvolve() => _hasPlant && _evolutionLevel < MaxEvolutionLevel;

        public void TriggerEvolve()
        {
            if (!CanEvolve()) return;
            _evolutionLevel++;
            AudioManager.Instance?.PlaySFX("evolution");

            // Use the shared colour table so it's always consistent
            if (glowRenderer != null)
                glowRenderer.color = EvolutionGlowColor();

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

            // Pulse animation
            _pulseTimer += Time.deltaTime;
            float scale = 1f + 0.04f * Mathf.Sin(_pulseTimer * 2f);
            transform.localScale = Vector3.one * scale;

            // Idle gem production — only tick while no gems are waiting
            if (_pendingGems == 0)
            {
                _productionTimer += Time.deltaTime;
                float rate = ProductionRates[Mathf.Clamp(_evolutionLevel, 0, ProductionRates.Length - 1)];
                if (_productionTimer >= rate)
                {
                    _productionTimer = 0f;
                    _pendingGems     = 1;
                    SetGemReady(true);
                }
            }
        }

        public void OnPointerClick(PointerEventData _)
        {
            // Collect pending gems first; don't open the action panel
            if (_hasPlant && _pendingGems > 0)
            {
                int gems = CollectGems();
                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.Data.totalGems += gems;

                FloatingTextPool.Instance?.Spawn(
                    transform.position + Vector3.up * 0.6f,
                    $"+{gems}",
                    new Color(1f, 0.88f, 0.1f));

                GardenUI.Instance?.RefreshGemCount();
                AudioManager.Instance?.PlaySFX("gemSpark");
                HapticFeedback.Light();
                return;
            }

            GardenUI.Instance?.OnSlotTapped(slotIndex, _hasPlant);
            HapticFeedback.Light();
        }
    }
}
