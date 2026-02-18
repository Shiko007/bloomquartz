using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bloomquartz.Core;
using Bloomquartz.Plants;
using Bloomquartz.Audio;

namespace Bloomquartz.UI
{
    public class GardenUI : MonoBehaviour
    {
        public static GardenUI Instance { get; private set; }

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI gemCountText;

        [Header("Slot Panel")]
        [SerializeField] private GameObject slotActionPanel;
        [SerializeField] private TextMeshProUGUI slotPanelTitle;
        [SerializeField] private TextMeshProUGUI slotRateText;
        [SerializeField] private Button plantButton;
        [SerializeField] private Button evolveButton;
        [SerializeField] private Button unlockButton;
        [SerializeField] private Button closeButton;

        [Header("Offline Reward")]
        [SerializeField] private GameObject offlinePanel;
        [SerializeField] private TextMeshProUGUI offlineText;

        private const int PlantCost  = 50;
        private const int UnlockCost = 500;
        private static int EvolveCost(int evoLevel) => 100 * (evoLevel + 1);

        // Mirrors GardenSlot.ProductionRates — one gem per N seconds per evo level
        private static readonly float[] ProductionRates = { 30f, 22f, 15f, 10f, 6f };

        private int _selectedSlot    = -1;
        private int _selectedEvoLevel = 0;

        private void Awake() => Instance = this;

        private void Start()
        {
            slotActionPanel?.SetActive(false);
            RefreshGemCount();
            CheckOfflineReward();
        }

        private void Update()
        {
            // Refresh gem count every second
            if (Time.frameCount % 60 == 0)
                RefreshGemCount();
        }

        public void RefreshGemCount()
        {
            if (gemCountText == null) return;
            int gems = SaveSystem.Instance != null ? SaveSystem.Instance.Data.totalGems : 0;
            gemCountText.text = $"Gems: {gems:N0}";
        }

        public void OnSlotTapped(int slotIndex, bool hasPlant)
        {
            _selectedSlot = slotIndex;
            slotActionPanel?.SetActive(true);
            unlockButton?.gameObject.SetActive(false);

            if (hasPlant)
            {
                var slots = PlantGarden.Instance?.GetComponentsInChildren<Plants.GardenSlot>();
                bool canEvolve = false;
                _selectedEvoLevel = 0;
                if (slots != null)
                    foreach (var s in slots)
                        if (s.GetSlotIndex() == slotIndex)
                        {
                            canEvolve         = s.CanEvolve();
                            _selectedEvoLevel = s.GetEvolutionLevel();
                            break;
                        }

                // Show production rate
                if (slotRateText != null)
                {
                    float rate = ProductionRates[Mathf.Clamp(_selectedEvoLevel, 0, ProductionRates.Length - 1)];
                    slotRateText.text = $"1 gem every {rate:F0}s";
                    slotRateText.gameObject.SetActive(true);
                }

                if (canEvolve)
                {
                    int cost = EvolveCost(_selectedEvoLevel);
                    slotPanelTitle.text = "Plant Options";
                    evolveButton?.gameObject.SetActive(true);
                    var tmp = evolveButton?.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = $"EVOLVE  ({cost} gems)";
                }
                else
                {
                    slotPanelTitle.text = "MAX LEVEL";
                    evolveButton?.gameObject.SetActive(false);
                }
                plantButton?.gameObject.SetActive(false);
            }
            else
            {
                slotPanelTitle.text = "Empty Slot";
                slotRateText?.gameObject.SetActive(false);
                plantButton?.gameObject.SetActive(true);
                evolveButton?.gameObject.SetActive(false);
                var tmp = plantButton?.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = $"PLANT  ({PlantCost} gems)";
            }
        }

        public void OnLockedSlotTapped(int slotIndex)
        {
            _selectedSlot = slotIndex;
            slotActionPanel?.SetActive(true);
            slotPanelTitle.text = "Locked Slot";
            slotRateText?.gameObject.SetActive(false);
            plantButton?.gameObject.SetActive(false);
            evolveButton?.gameObject.SetActive(false);

            if (unlockButton != null)
            {
                unlockButton.gameObject.SetActive(true);
                var tmp = unlockButton.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = $"UNLOCK  ({UnlockCost} gems)";
            }
        }

        public void OnUnlockPressed()
        {
            if (_selectedSlot < 0) return;
            int gems = SaveSystem.Instance?.Data.totalGems ?? 0;
            if (gems < UnlockCost)
            {
                slotPanelTitle.text = $"Need {UnlockCost - gems} more gems!";
                AudioManager.Instance?.PlaySFX("uiTap");
                return;
            }

            SaveSystem.Instance.Data.totalGems -= UnlockCost;
            SaveSystem.Instance.Data.unlockedAreas[_selectedSlot] = true;
            SaveSystem.Instance.Save();
            PlantGarden.Instance?.UnlockSlot(_selectedSlot);
            slotActionPanel?.SetActive(false);
            RefreshGemCount();
            AudioManager.Instance?.PlaySFX("evolution");
            Idle.IdleManager.Instance?.CalculateAndApplyEarnings();
        }

        public void OnPlantPressed()
        {
            if (_selectedSlot < 0) return;

            int gems = SaveSystem.Instance?.Data.totalGems ?? 0;
            if (gems < PlantCost)
            {
                slotPanelTitle.text = $"Need {PlantCost - gems} more gems!";
                AudioManager.Instance?.PlaySFX("uiTap");
                return;
            }

            SaveSystem.Instance.Data.totalGems -= PlantCost;
            SaveSystem.Instance.Save();
            PlantGarden.Instance?.PlantDefaultInSlot(_selectedSlot);
            slotActionPanel?.SetActive(false);
            RefreshGemCount();
        }

        public void OnEvolvePressed()
        {
            int cost = EvolveCost(_selectedEvoLevel);
            int gems = SaveSystem.Instance?.Data.totalGems ?? 0;
            if (gems < cost)
            {
                slotPanelTitle.text = $"Need {cost - gems} more gems!";
                AudioManager.Instance?.PlaySFX("uiTap");
                return;
            }

            SaveSystem.Instance.Data.totalGems -= cost;
            SaveSystem.Instance.Save();
            PlantGarden.Instance?.EvolveAtSlot(_selectedSlot);
            slotActionPanel?.SetActive(false);
            RefreshGemCount();
        }

        public void OnClosePanel() => slotActionPanel?.SetActive(false);

        private void CheckOfflineReward()
        {
            // Trigger calculation before reading the result
            Idle.IdleManager.Instance?.CalculateAndApplyEarnings();

            RefreshGemCount();

            var idle = Idle.IdleManager.Instance;
            if (offlinePanel == null) return;
            if (idle != null && idle.HasOfflineReward())
            {
                offlinePanel.SetActive(true);
                if (offlineText != null)
                    offlineText.text = $"+{idle.PendingOfflineGems} gems while you were away!";
            }
            else
            {
                offlinePanel.SetActive(false);
            }
        }

        public void OnClaimOfflineReward()
        {
            Idle.IdleManager.Instance?.ClaimOfflineReward();
            offlinePanel?.SetActive(false);
            RefreshGemCount();
        }

        public void OnPuzzlePressed() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("PuzzleBoard");

        public void OnMenuPressed() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
