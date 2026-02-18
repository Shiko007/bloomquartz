using UnityEngine;
using Bloomquartz.Juice;

namespace Bloomquartz.Gems
{
    public class GemCollector : MonoBehaviour
    {
        public static GemCollector Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void OnGemProduced(Vector3 worldPosition)
        {
            JuiceManager.Instance.PlayGemSparkle(worldPosition);
            HapticFeedback.Light();
        }

        public void CollectAll()
        {
            HapticFeedback.Medium();
        }
    }
}
