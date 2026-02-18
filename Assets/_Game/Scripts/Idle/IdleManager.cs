using System;
using UnityEngine;
using Bloomquartz.Core;

namespace Bloomquartz.Idle
{
    public class IdleManager : MonoBehaviour
    {
        public static IdleManager Instance { get; private set; }

        [Header("Cap")]
        [SerializeField] private float maxOfflineHours = 12f;

        // Seconds per gem per evolution level — must match GardenSlot.ProductionRates
        private static readonly float[] ProductionRates = { 30f, 22f, 15f, 10f, 6f };

        public int PendingOfflineGems { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // First-launch / app-restart calculation uses lastGardenExitTime
            CalculateAndApplyEarnings();
        }

        /// Calculates gems earned since the last Garden exit, adds them to the
        /// player's wallet, and resets the reference timestamp so calling this
        /// again immediately won't double-count.
        public void CalculateAndApplyEarnings()
        {
            if (SaveSystem.Instance == null) return;

            long exitBinary = SaveSystem.Instance.Data.lastGardenExitTime;
            if (exitBinary == 0) return; // garden never visited yet

            DateTime exitTime    = DateTime.FromBinary(exitBinary);
            double   secondsAway = (DateTime.UtcNow - exitTime).TotalSeconds;
            secondsAway = Math.Min(secondsAway, maxOfflineHours * 3600.0);

            if (secondsAway < 1.0) return;

            PlantSaveEntry[] plants = SaveSystem.Instance.Data.plants;
            if (plants == null || plants.Length == 0) return;

            int total = 0;
            foreach (var plant in plants)
            {
                int   level = Mathf.Clamp(plant.evolutionLevel, 0, ProductionRates.Length - 1);
                float rate  = ProductionRates[level];
                total += Mathf.FloorToInt((float)secondsAway / rate);
            }

            if (total > 0)
            {
                PendingOfflineGems = total;
                SaveSystem.Instance.Data.totalGems += total;
            }

            // Reset reference point so re-entering Garden mid-session doesn't re-award
            SaveSystem.Instance.Data.lastGardenExitTime = DateTime.UtcNow.ToBinary();
            SaveSystem.Instance.Save();
        }

        public bool HasOfflineReward() => PendingOfflineGems > 0;

        public void ClaimOfflineReward()
        {
            PendingOfflineGems = 0;
        }
    }
}
