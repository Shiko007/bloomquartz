using UnityEngine;

namespace Bloomquartz.Plants
{
    [CreateAssetMenu(fileName = "PlantData", menuName = "Bloomquartz/Plant Data")]
    public class PlantData : ScriptableObject
    {
        [Header("Identity")]
        public string plantId;
        public string displayName;
        [TextArea] public string description;

        [Header("Production")]
        public float gemProductionIntervalSeconds = 300f;
        public Gems.GemType producedGemType;

        [Header("Evolutions")]
        public PlantEvolutionStage[] evolutions;
    }

    [System.Serializable]
    public class PlantEvolutionStage
    {
        public string stageName;
        public Sprite sprite;
        public int gemsRequiredToEvolve;
        public float productionMultiplier;
    }
}
