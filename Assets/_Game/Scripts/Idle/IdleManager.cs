using System;
using UnityEngine;
using Bloomquartz.Core;

namespace Bloomquartz.Idle
{
    /// <summary>
    /// Calculates offline gem earnings when the player returns to the game.
    /// </summary>
    public class IdleManager : MonoBehaviour
    {
        public static IdleManager Instance { get; private set; }

        [Header("Cap")]
        [SerializeField] private float maxOfflineHours = 12f;

        public int PendingOfflineGems { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            CalculateOfflineEarnings();
        }

        private void CalculateOfflineEarnings()
        {
            long savedBinary = SaveSystem.Instance.Data.lastSaveTime;
            if (savedBinary == 0) return;

            DateTime lastSave = DateTime.FromBinary(savedBinary);
            double hoursAway = (DateTime.UtcNow - lastSave).TotalHours;
            hoursAway = Math.Min(hoursAway, maxOfflineHours);

            PlantSaveEntry[] plants = SaveSystem.Instance.Data.plants;
            int gemsPerHourPerPlant = 12; // tunable
            int earned = Mathf.FloorToInt((float)(hoursAway * plants.Length * gemsPerHourPerPlant));

            if (earned > 0)
            {
                PendingOfflineGems = earned;
                SaveSystem.Instance.Data.totalGems += earned;
                SaveSystem.Instance.Save();
            }
        }

        public bool HasOfflineReward() => PendingOfflineGems > 0;

        public void ClaimOfflineReward()
        {
            PendingOfflineGems = 0;
        }
    }
}
