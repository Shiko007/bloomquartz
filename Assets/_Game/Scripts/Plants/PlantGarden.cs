using System.Collections.Generic;
using UnityEngine;
using Bloomquartz.Core;

namespace Bloomquartz.Plants
{
    public class PlantGarden : MonoBehaviour
    {
        public static PlantGarden Instance { get; private set; }

        [Header("Slots")]
        [SerializeField] private Transform[] gardenSlots;
        [SerializeField] private PlantCreature plantPrefab;

        private List<PlantCreature> _activePlants = new List<PlantCreature>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
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
