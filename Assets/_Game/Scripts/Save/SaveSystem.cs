using System;
using System.IO;
using UnityEngine;

namespace Bloomquartz.Core
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private SaveData _data;
        private string _savePath;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _savePath = Path.Combine(Application.persistentDataPath, "bloomquartz.json");
            Load(); // Load in Awake so data is ready before any other script's Start()
        }

        public void Load()
        {
            if (File.Exists(_savePath))
            {
                string json = File.ReadAllText(_savePath);
                _data = JsonUtility.FromJson<SaveData>(json);
            }
            else
            {
                _data = new SaveData();
            }
        }

        public void Save()
        {
            _data.lastSaveTime = DateTime.UtcNow.ToBinary();
            File.WriteAllText(_savePath, JsonUtility.ToJson(_data, true));
        }

        public SaveData Data => _data;

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                // Stamp exit time so idle earnings are calculated correctly on resume
                _data.lastGardenExitTime = DateTime.UtcNow.ToBinary();
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            _data.lastGardenExitTime = DateTime.UtcNow.ToBinary();
            Save();
        }
    }

    [Serializable]
    public class SaveData
    {
        public long lastSaveTime;
        public long lastGardenExitTime; // set whenever the Garden scene is unloaded
        public int totalGems;
        public int currentWorld;
        public int highestLevel;
        public bool[] unlockedAreas = new bool[20];
        public PlantSaveEntry[] plants = new PlantSaveEntry[0];
    }

    [Serializable]
    public class PlantSaveEntry
    {
        public string plantId;
        public int evolutionLevel;
        public int slotIndex;
        public long lastHarvestTime;
    }
}
