using UnityEngine;

namespace Bloomquartz.Puzzle
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Bloomquartz/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Config")]
        public int levelIndex;
        public string levelName;
        public int worldIndex;

        [Header("Board")]
        public int boardWidth  = 7;
        public int boardHeight = 7;
        public int moveLimit   = 30;

        [Header("Goals")]
        public LevelGoal[] goals;

        [Header("Rewards")]
        public int gemReward;
        public int starThreshold1;
        public int starThreshold2;
        public int starThreshold3;

        [Header("Unlocks")]
        public int[] unlocksAreaIndices;
    }

    [System.Serializable]
    public class LevelGoal
    {
        public GoalType type;
        public Gems.GemType gemType;
        public int amount;
    }

    public enum GoalType
    {
        CollectGems,
        ReachScore,
        ClearTiles,
        MakeChain
    }
}
