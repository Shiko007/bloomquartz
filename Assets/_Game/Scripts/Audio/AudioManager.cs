using System.Collections.Generic;
using UnityEngine;

namespace Bloomquartz.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip puzzleMusic;
        [SerializeField] private AudioClip gardenMusic;

        [Header("SFX")]
        [SerializeField] private AudioClip gemPopSfx;
        [SerializeField] private AudioClip gemSparkSfx;
        [SerializeField] private AudioClip swapSfx;
        [SerializeField] private AudioClip evolutionSfx;
        [SerializeField] private AudioClip uiTapSfx;

        private Dictionary<string, AudioClip> _sfxMap;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxMap = new Dictionary<string, AudioClip>
            {
                { "gemPop",    gemPopSfx },
                { "gemSpark",  gemSparkSfx },
                { "swap",      swapSfx },
                { "evolution", evolutionSfx },
                { "uiTap",     uiTapSfx }
            };
        }

        public void PlayMusic(string sceneName)
        {
            AudioClip clip = sceneName switch
            {
                "MainMenu"   => mainMenuMusic,
                "PuzzleBoard" => puzzleMusic,
                "Garden"     => gardenMusic,
                _            => null
            };

            if (clip == null || musicSource.clip == clip) return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void PlaySFX(string key)
        {
            if (_sfxMap.TryGetValue(key, out AudioClip clip) && clip != null)
                sfxSource.PlayOneShot(clip);
        }

        public void SetMusicVolume(float v) => musicSource.volume = v;
        public void SetSFXVolume(float v)   => sfxSource.volume = v;
    }
}
