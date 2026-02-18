using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bloomquartz.Puzzle;
using Bloomquartz.Core;
using Bloomquartz.Juice;

namespace Bloomquartz.UI
{
    public class WinLoseController : MonoBehaviour
    {
        public static WinLoseController Instance { get; private set; }

        [Header("Blocker")]
        [SerializeField] private GameObject blocker;

        [Header("Win Panel")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private TextMeshProUGUI winScoreText;
        [SerializeField] private Image[] starImages;

        [Header("Lose Panel")]
        [SerializeField] private GameObject losePanel;
        [SerializeField] private TextMeshProUGUI loseScoreText;

        [Header("Goal Config")]
        [SerializeField] private int scoreGoal      = 2000;
        [SerializeField] private int starThreshold1 = 1000;
        [SerializeField] private int starThreshold2 = 2000;
        [SerializeField] private int starThreshold3 = 3500;

        private bool _gameOver;
        public bool IsGameOver => _gameOver;

        private void Awake()
        {
            Instance = this;
            winPanel?.SetActive(false);
            losePanel?.SetActive(false);
        }

        private void Start()
        {
            // Override Inspector values with level-scaled config
            int level      = SaveSystem.Instance?.Data.highestLevel ?? 0;
            scoreGoal      = LevelConfig.GetGoal(level);
            starThreshold1 = LevelConfig.GetStar1(level);
            starThreshold2 = LevelConfig.GetStar2(level);
            starThreshold3 = LevelConfig.GetStar3(level);

            HUDController.Instance?.SetGoalText($"Level {level + 1}  |  Goal: {scoreGoal:N0}");
        }

        public void CheckEndCondition()
        {
            if (_gameOver) return;
            if (ScoreManager.Instance.Score >= scoreGoal) { TriggerWin(); return; }
            if (ScoreManager.Instance.MovesLeft <= 0) TriggerLose();
        }

        private void TriggerWin()
        {
            _gameOver = true;
            blocker?.SetActive(true);
            HapticFeedback.Heavy();
            StartCoroutine(ShowWinPanel());
        }

        private void TriggerLose()
        {
            _gameOver = true;
            blocker?.SetActive(true);
            HapticFeedback.Medium();
            StartCoroutine(ShowLosePanel());
        }

        private IEnumerator ShowWinPanel()
        {
            yield return new WaitForSeconds(0.5f);

            int level     = SaveSystem.Instance?.Data.highestLevel ?? 0;
            int stars     = ScoreManager.Instance.GetStars(starThreshold1, starThreshold2, starThreshold3);
            int gemReward = LevelConfig.GetGemReward(level) * Mathf.Max(1, stars);

            winPanel?.SetActive(true);
            if (winScoreText != null)
                winScoreText.text = $"Score: {ScoreManager.Instance.Score:N0}\n+{gemReward} gems";

            // Reset all stars to dim
            if (starImages != null)
                foreach (var s in starImages) if (s != null) s.color = new Color(1f, 1f, 1f, 0.2f);

            // Animate stars in one by one
            for (int i = 0; i < stars && starImages != null && i < starImages.Length; i++)
            {
                yield return new WaitForSeconds(0.35f);
                if (starImages[i] != null)
                {
                    starImages[i].color = new Color(1f, 0.88f, 0.1f);
                    HapticFeedback.Light();
                    StartCoroutine(PunchScale(starImages[i].transform));
                }
            }

            SaveProgress(gemReward);
        }

        private IEnumerator ShowLosePanel()
        {
            yield return new WaitForSeconds(0.5f);
            losePanel?.SetActive(true);
            if (loseScoreText != null)
                loseScoreText.text = $"Score: {ScoreManager.Instance.Score:N0}";
        }

        private void SaveProgress(int gemReward)
        {
            if (SaveSystem.Instance == null) return;
            var data = SaveSystem.Instance.Data;
            data.totalGems += gemReward;
            data.highestLevel++;          // advance to next level
            SaveSystem.Instance.Save();
        }

        private IEnumerator PunchScale(Transform t)
        {
            Vector3 original = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float s = 1f + 0.4f * Mathf.Sin(elapsed / 0.3f * Mathf.PI);
                t.localScale = original * s;
                yield return null;
            }
            t.localScale = original;
        }

        public void OnRetryPressed() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        public void OnMenuPressed()
        {
            if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
            else UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        public void OnNextPressed()
        {
            if (GameManager.Instance != null) GameManager.Instance.GoToGarden();
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Garden");
        }
    }
}
