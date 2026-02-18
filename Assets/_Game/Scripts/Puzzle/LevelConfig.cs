using UnityEngine;

namespace Bloomquartz.Puzzle
{
    /// Centralised level-scaling data — no ScriptableObjects needed.
    /// All values are derived from the level index (0-based internally).
    public static class LevelConfig
    {
        /// Moves available on level (display number = level + 1).
        public static int GetMoves(int level)     => Mathf.Max(15, 32 - level);

        /// Score required to win.
        public static int GetGoal(int level)      => 800 + level * 400;

        public static int GetStar1(int level)     => GetGoal(level) / 2;
        public static int GetStar2(int level)     => GetGoal(level);
        public static int GetStar3(int level)     => (int)(GetGoal(level) * 1.5f);

        /// Base gem reward (multiplied by stars earned: 1-3).
        public static int GetGemReward(int level) => 40 + level * 20;
    }
}
