using System;
using UnityEngine;
using Bloomquartz.Core;

namespace Bloomquartz.Gems
{
    /// <summary>
    /// Attached to each plant slot in the Garden.
    /// Produces gems passively over time (idle mechanic).
    /// </summary>
    public class GemProducer : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float productionIntervalSeconds = 300f; // 5 min default

        private PlantSaveEntry _entry;
        private float _timer;

        public void Init(PlantSaveEntry entry)
        {
            _entry = entry;
            CalculateOfflineGems();
        }

        private void CalculateOfflineGems()
        {
            if (_entry.lastHarvestTime == 0) return;

            DateTime lastHarvest = DateTime.FromBinary(_entry.lastHarvestTime);
            double secondsAway = (DateTime.UtcNow - lastHarvest).TotalSeconds;
            int gemsEarned = Mathf.FloorToInt((float)(secondsAway / productionIntervalSeconds));

            if (gemsEarned > 0)
            {
                SaveSystem.Instance.Data.totalGems += gemsEarned;
                Debug.Log($"[Idle] Earned {gemsEarned} gems while away.");
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= productionIntervalSeconds)
            {
                _timer = 0f;
                ProduceGem();
            }
        }

        private void ProduceGem()
        {
            SaveSystem.Instance.Data.totalGems++;
            _entry.lastHarvestTime = DateTime.UtcNow.ToBinary();
            SaveSystem.Instance.Save();

            GemCollector.Instance.OnGemProduced(transform.position);
        }
    }
}
