using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bloomquartz.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject loadingScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            State = GameState.MainMenu;
        }

        public void GoToPuzzle(int levelIndex)
        {
            State = GameState.Puzzle;
            SceneManager.LoadScene("PuzzleBoard");
        }

        public void GoToGarden()
        {
            State = GameState.Garden;
            SceneManager.LoadScene("Garden");
        }

        public void GoToMainMenu()
        {
            State = GameState.MainMenu;
            SceneManager.LoadScene("MainMenu");
        }

        public void GoToWorldMap()
        {
            State = GameState.WorldMap;
            SceneManager.LoadScene("WorldMap");
        }
    }
}
