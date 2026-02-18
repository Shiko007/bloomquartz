using UnityEngine;
using Bloomquartz.UI;

namespace Bloomquartz.Puzzle
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int Score       { get; private set; }
        public int MovesLeft   { get; private set; }
        public int GemsMatched { get; private set; }

        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnMovesChanged;
        public event System.Action<int> OnGemsMatched;

        private int   _basePointsPerGem = 50;
        private int   _comboMultiplier  = 1;
        private float _evolutionBonus   = 1f; // +10% per total evo level across all plants

        private void Awake()
        {
            Instance = this;
        }

        /// Call once at start of puzzle with the sum of all plant evolution levels.
        public void SetEvolutionBonus(int totalEvoLevels)
        {
            _evolutionBonus = 1f + totalEvoLevels * 0.10f;
        }

        public void AddMoves(int count)
        {
            MovesLeft += count;
            OnMovesChanged?.Invoke(MovesLeft);
        }

        public void Init(int moves)
        {
            Score          = 0;
            MovesLeft      = moves;
            GemsMatched    = 0;
            _comboMultiplier = 1;
            OnScoreChanged?.Invoke(Score);
            OnMovesChanged?.Invoke(MovesLeft);
        }

        public void RegisterMatch(int gemCount, bool isCascade)
        {
            if (isCascade)
                _comboMultiplier++;
            else
                _comboMultiplier = 1;

            int points = Mathf.RoundToInt(gemCount * _basePointsPerGem * _comboMultiplier * _evolutionBonus);
            Score       += points;
            GemsMatched += gemCount;

            OnScoreChanged?.Invoke(Score);
            OnGemsMatched?.Invoke(GemsMatched);

            if (_comboMultiplier > 1)
                HUDController.Instance?.ShowComboText(_comboMultiplier);
        }

        public bool UseMove()
        {
            if (MovesLeft <= 0) return false;
            MovesLeft--;
            OnMovesChanged?.Invoke(MovesLeft);
            return true;
        }

        public int GetStars(int threshold1, int threshold2, int threshold3)
        {
            if (Score >= threshold3) return 3;
            if (Score >= threshold2) return 2;
            if (Score >= threshold1) return 1;
            return 0;
        }
    }
}
