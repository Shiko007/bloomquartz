using UnityEngine;

namespace Bloomquartz.Gems
{
    [CreateAssetMenu(fileName = "GemData", menuName = "Bloomquartz/Gem Data")]
    public class GemData : ScriptableObject
    {
        public GemType gemType;
        public string displayName;
        public Sprite sprite;
        public Color color;
        public int baseValue;
        public bool isRare;
    }
}
