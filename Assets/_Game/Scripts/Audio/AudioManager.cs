using System.Collections.Generic;
using UnityEngine;
using Bloomquartz.Juice;

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

        [Header("SFX (leave empty to use procedural audio)")]
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

            // Auto-create AudioSources if not assigned in the Inspector
            if (musicSource == null)
            {
                musicSource        = gameObject.AddComponent<AudioSource>();
                musicSource.loop   = true;
                musicSource.volume = 0.6f;
            }
            if (sfxSource == null)
            {
                sfxSource        = gameObject.AddComponent<AudioSource>();
                sfxSource.volume = 1f;
            }

            _sfxMap = new Dictionary<string, AudioClip>
            {
                { "gemPop",    gemPopSfx },
                { "gemSpark",  gemSparkSfx },
                { "swap",      swapSfx },
                { "evolution", evolutionSfx },
                { "uiTap",     uiTapSfx }
            };

            // Fill any missing SFX with procedurally generated clips
            FillProceduralFallbacks();
        }

        private void FillProceduralFallbacks()
        {
            if (!HasClip("gemPop"))    _sfxMap["gemPop"]    = ProceduralAudio.CreatePop(660f);
            if (!HasClip("gemSpark"))  _sfxMap["gemSpark"]  = ProceduralAudio.CreateTone(880f, 0.25f, 0.3f);
            if (!HasClip("swap"))      _sfxMap["swap"]      = ProceduralAudio.CreatePop(440f);
            if (!HasClip("evolution")) _sfxMap["evolution"] = ProceduralAudio.CreateChime(440f, 4, 0.12f);
            if (!HasClip("uiTap"))     _sfxMap["uiTap"]     = ProceduralAudio.CreatePop(550f);
        }

        private bool HasClip(string key) =>
            _sfxMap.TryGetValue(key, out var c) && c != null;

        public void PlayMusic(string sceneName)
        {
            AudioClip clip = sceneName switch
            {
                "MainMenu"   => mainMenuMusic,
                "PuzzleBoard" => puzzleMusic,
                "Garden"     => gardenMusic,
                _            => null
            };

            if (clip == null || musicSource == null || musicSource.clip == clip) return;

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
