using UnityEngine;
using Bloomquartz.Core;

namespace Bloomquartz.Meta
{
    public class WorldMap : MonoBehaviour
    {
        [Header("Area Buttons")]
        [SerializeField] private AreaButton[] areaButtons;

        private void Start()
        {
            RefreshAreas();
        }

        private void RefreshAreas()
        {
            bool[] unlocked = SaveSystem.Instance.Data.unlockedAreas;

            for (int i = 0; i < areaButtons.Length; i++)
            {
                bool isUnlocked = i < unlocked.Length && unlocked[i];
                areaButtons[i].SetState(isUnlocked);
            }
        }

        public void OnAreaClicked(int areaIndex)
        {
            bool[] unlocked = SaveSystem.Instance.Data.unlockedAreas;
            if (areaIndex < unlocked.Length && unlocked[areaIndex])
            {
                GameManager.Instance.GoToPuzzle(areaIndex);
            }
        }
    }

    [System.Serializable]
    public class AreaButton
    {
        public UnityEngine.UI.Button button;
        public GameObject lockIcon;

        public void SetState(bool unlocked)
        {
            button.interactable = unlocked;
            if (lockIcon != null) lockIcon.SetActive(!unlocked);
        }
    }
}
