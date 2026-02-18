using System.Collections;
using UnityEngine;
using TMPro;
using Bloomquartz.Puzzle;

namespace Bloomquartz.UI
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("Score & Moves")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesText;

        [Header("Goals")]
        [SerializeField] private TextMeshProUGUI goalText;

        [Header("Combo")]
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private RectTransform   comboRect;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
            ScoreManager.Instance.OnMovesChanged += UpdateMoves;

            if (comboText != null)
                comboText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance == null) return;
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
            ScoreManager.Instance.OnMovesChanged -= UpdateMoves;
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score\n<size=110%><b>{score}</b></size>";
        }

        private void UpdateMoves(int moves)
        {
            if (movesText != null)
            {
                movesText.text = $"Moves\n<size=110%><b>{moves}</b></size>";
                if (moves <= 5)
                    movesText.color = new Color(1f, 0.35f, 0.2f);
            }
        }

        public void SetGoalText(string text)
        {
            if (goalText != null)
                goalText.text = text;
        }

        public void ShowComboText(int multiplier)
        {
            if (comboText == null) return;
            StopAllCoroutines();
            StartCoroutine(AnimateCombo(multiplier));
        }

        private IEnumerator AnimateCombo(int multiplier)
        {
            comboText.text = $"x{multiplier} COMBO!";
            comboText.gameObject.SetActive(true);

            Vector2 startPos = comboRect != null ? comboRect.anchoredPosition : Vector2.zero;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / 0.8f;
                float ease = 1f - Mathf.Pow(1f - t, 3f);

                if (comboRect != null)
                    comboRect.anchoredPosition = startPos + new Vector2(0, 40f * ease);

                comboText.alpha = t < 0.7f ? 1f : 1f - ((t - 0.7f) / 0.3f);
                yield return null;
            }

            comboText.gameObject.SetActive(false);
            if (comboRect != null)
                comboRect.anchoredPosition = startPos;
        }
    }
}
