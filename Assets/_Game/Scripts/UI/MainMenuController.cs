using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Bloomquartz.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RectTransform titleRect;
        [SerializeField] private CanvasGroup buttonsGroup;
        [SerializeField] private TextMeshProUGUI offlineRewardText;
        [SerializeField] private GameObject offlineRewardPanel;

        private void Start()
        {
            StartCoroutine(IntroAnimation());
            // Offline reward is shown in the Garden, not here.
            offlineRewardPanel?.SetActive(false);
        }

        private IEnumerator IntroAnimation()
        {
            // Title drops in from above
            if (titleRect != null)
            {
                Vector2 endPos = titleRect.anchoredPosition;
                titleRect.anchoredPosition = endPos + new Vector2(0, 300f);
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / 0.6f;
                    float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                    titleRect.anchoredPosition = Vector2.Lerp(
                        endPos + new Vector2(0, 300f), endPos, ease);
                    yield return null;
                }
                titleRect.anchoredPosition = endPos;
            }

            // Buttons fade in
            if (buttonsGroup != null)
            {
                buttonsGroup.alpha = 0f;
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / 0.4f;
                    buttonsGroup.alpha = Mathf.Clamp01(t);
                    yield return null;
                }
                buttonsGroup.alpha = 1f;
            }
        }

        private void CheckOfflineReward()
        {
            if (offlineRewardPanel == null) return;
            var idle = Bloomquartz.Idle.IdleManager.Instance;
            if (idle != null && idle.HasOfflineReward())
            {
                offlineRewardPanel.SetActive(true);
                if (offlineRewardText != null)
                    offlineRewardText.text = $"+{idle.PendingOfflineGems} gems while you were away!";
            }
            else
            {
                offlineRewardPanel?.SetActive(false);
            }
        }

        public void OnPlayPressed()
        {
            Juice.HapticFeedback.Light();
            SceneManager.LoadScene("PuzzleBoard");
        }

        public void OnGardenPressed()
        {
            Juice.HapticFeedback.Light();
            SceneManager.LoadScene("Garden");
        }

        public void OnClaimOfflineReward()
        {
            Bloomquartz.Idle.IdleManager.Instance?.ClaimOfflineReward();
            offlineRewardPanel?.SetActive(false);
        }
    }
}
