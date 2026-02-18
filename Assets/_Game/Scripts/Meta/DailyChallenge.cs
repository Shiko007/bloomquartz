using System;
using UnityEngine;
using Bloomquartz.Core;

namespace Bloomquartz.Meta
{
    public class DailyChallenge : MonoBehaviour
    {
        public static DailyChallenge Instance { get; private set; }

        private const string LastClaimKey = "DailyChallenge_LastClaim";

        public bool IsAvailable { get; private set; }
        public ChallengeType TodaysChallenge { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            CheckAvailability();
            GenerateTodaysChallenge();
        }

        private void CheckAvailability()
        {
            string lastClaim = PlayerPrefs.GetString(LastClaimKey, "");
            if (string.IsNullOrEmpty(lastClaim))
            {
                IsAvailable = true;
                return;
            }

            DateTime last = DateTime.Parse(lastClaim);
            IsAvailable = DateTime.UtcNow.Date > last.Date;
        }

        private void GenerateTodaysChallenge()
        {
            int dayOfYear = DateTime.UtcNow.DayOfYear;
            int count = Enum.GetValues(typeof(ChallengeType)).Length;
            TodaysChallenge = (ChallengeType)(dayOfYear % count);
        }

        public void ClaimReward(int gemReward)
        {
            if (!IsAvailable) return;

            SaveSystem.Instance.Data.totalGems += gemReward;
            SaveSystem.Instance.Save();

            PlayerPrefs.SetString(LastClaimKey, DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();

            IsAvailable = false;
        }
    }

    public enum ChallengeType
    {
        MakeMatchesOfFive,
        ClearRedGems,
        ScoreChain,
        ClearCorners,
        SpeedRound
    }
}
