using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bloomquartz.Core;
using Bloomquartz.Plants;

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
        [SerializeField] private Button plantButton;
        [SerializeField] private Button evolveButton;
        [SerializeField] private Button closeButton;

        [Header("Offline Reward")]
        [SerializeField] private GameObject offlinePanel;
        [SerializeField] private TextMeshProUGUI offlineText;

        private int _selectedSlot = -1;

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

            if (hasPlant)
            {
                var slots = PlantGarden.Instance?.GetComponentsInChildren<Plants.GardenSlot>();
                bool canEvolve = false;
                if (slots != null)
                    foreach (var s in slots)
                        if (s.GetSlotIndex() == slotIndex) { canEvolve = s.CanEvolve(); break; }

                if (canEvolve)
                {
                    slotPanelTitle.text = "Plant Options";
                    evolveButton?.gameObject.SetActive(true);
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
                plantButton?.gameObject.SetActive(true);
                evolveButton?.gameObject.SetActive(false);
            }
        }

        public void OnPlantPressed()
        {
            if (_selectedSlot < 0) return;
            PlantGarden.Instance?.PlantDefaultInSlot(_selectedSlot);
            slotActionPanel?.SetActive(false);
            RefreshGemCount();
        }

        public void OnEvolvePressed()
        {
            PlantGarden.Instance?.EvolveAtSlot(_selectedSlot);
            slotActionPanel?.SetActive(false);
        }

        public void OnClosePanel() => slotActionPanel?.SetActive(false);

        private void CheckOfflineReward()
        {
            var idle = Idle.IdleManager.Instance;
            if (offlinePanel == null) return;
            if (idle != null && idle.HasOfflineReward())
            {
                offlinePanel.SetActive(true);
                if (offlineText != null)
                    offlineText.text = $"+{idle.PendingOfflineGems} gems collected while away!";
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
