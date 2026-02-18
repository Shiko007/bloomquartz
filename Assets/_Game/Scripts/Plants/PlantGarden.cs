using System.Collections.Generic;
using UnityEngine;
using Bloomquartz.Core;
using Bloomquartz.Audio;

namespace Bloomquartz.Plants
{
    public class PlantGarden : MonoBehaviour
    {
        public static PlantGarden Instance { get; private set; }

        [Header("Slots")]
        [SerializeField] private Transform[] gardenSlots;
        [SerializeField] private PlantCreature plantPrefab;
        [SerializeField] private PlantData _defaultPlantData;

        private List<PlantCreature> _activePlants = new List<PlantCreature>();
        private bool[] _slotStates;
        private Transform[] _gardenSlotTransforms;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            int count = gardenSlots != null ? gardenSlots.Length : 6;
            _slotStates           = new bool[count];
            _gardenSlotTransforms = gardenSlots;

            if (Juice.FloatingTextPool.Instance == null)
                new GameObject("FloatingTextPool").AddComponent<Juice.FloatingTextPool>();

            LoadGarden();
        }

        private void LoadGarden()
        {
            if (SaveSystem.Instance == null) return;
            PlantSaveEntry[] saved = SaveSystem.Instance.Data.plants;
            if (saved == null) return;
            foreach (PlantSaveEntry entry in saved)
            {
                if (gardenSlots == null || entry.slotIndex >= gardenSlots.Length) continue;
                _slotStates[entry.slotIndex] = true;

                var slots = GetComponentsInChildren<GardenSlot>();
                foreach (var s in slots)
                    if (s.GetSlotIndex() == entry.slotIndex)
                        s.SetHasPlant(true);

                if (plantPrefab != null)
                    SpawnPlant(entry);
            }
        }

        private void SpawnPlant(PlantSaveEntry entry)
        {
            Transform slot = gardenSlots[entry.slotIndex];
            PlantCreature plant = Instantiate(plantPrefab, slot.position, Quaternion.identity, slot);
            plant.Init(entry);
            _activePlants.Add(plant);
        }

        public void PlantDefaultInSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotStates.Length) return;
            if (_slotStates[slotIndex]) return; // already planted

            _slotStates[slotIndex] = true;

            // Update slot visual
            var slots = GetComponentsInChildren<GardenSlot>();
            foreach (var s in slots)
                if (s.GetSlotIndex() == slotIndex)
                    s.SetHasPlant(true);

            // Save a basic entry
            var entry = new PlantSaveEntry
            {
                plantId        = "default",
                evolutionLevel = 0,
                slotIndex      = slotIndex
            };

            if (SaveSystem.Instance != null)
            {
                var list = new System.Collections.Generic.List<PlantSaveEntry>(
                    SaveSystem.Instance.Data.plants ?? new PlantSaveEntry[0]) { entry };
                SaveSystem.Instance.Data.plants = list.ToArray();
                SaveSystem.Instance.Save();
            }

            Juice.JuiceManager.Instance?.PlayGemSparkle(
                _gardenSlotTransforms != null && slotIndex < _gardenSlotTransforms.Length
                    ? _gardenSlotTransforms[slotIndex].position : Vector3.zero);
            AudioManager.Instance?.PlaySFX("uiTap");
            Juice.HapticFeedback.Medium();
        }

        public void EvolveAtSlot(int slotIndex)
        {
            // Try full PlantCreature path first
            foreach (var plant in _activePlants)
                if (plant.GetSlotIndex() == slotIndex)
                { plant.Evolve(); return; }

            // Visual-only path (no prefab assigned yet)
            var slots = GetComponentsInChildren<GardenSlot>();
            foreach (var s in slots)
            {
                if (s.GetSlotIndex() != slotIndex) continue;
                s.TriggerEvolve();

                Juice.JuiceManager.Instance?.PlayEvolutionBurst(s.transform.position);
                Juice.HapticFeedback.Heavy();

                // Update save entry evolution level
                if (SaveSystem.Instance != null)
                {
                    foreach (var entry in SaveSystem.Instance.Data.plants)
                        if (entry.slotIndex == slotIndex)
                        { entry.evolutionLevel++; break; }
                    SaveSystem.Instance.Save();
                }
                return;
            }
        }

        public void PlantInSlot(int slotIndex, PlantData data)
        {
            PlantSaveEntry entry = new PlantSaveEntry
            {
                plantId = data.plantId,
                evolutionLevel = 0,
                slotIndex = slotIndex
            };

            List<PlantSaveEntry> list = new List<PlantSaveEntry>(SaveSystem.Instance.Data.plants) { entry };
            SaveSystem.Instance.Data.plants = list.ToArray();
            SaveSystem.Instance.Save();

            SpawnPlant(entry);
        }
    }
}
