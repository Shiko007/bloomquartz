using UnityEngine;

namespace Bloomquartz.Juice
{
    /// Generates simple sine-wave audio clips at runtime — no audio files needed.
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateTone(float frequency, float duration,
            float volume = 0.4f, bool fadeOut = true)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            var clip    = AudioClip.Create("tone", samples, 1, SampleRate, false);
            var data    = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t     = (float)i / SampleRate;
                float fade  = fadeOut ? 1f - (t / duration) : 1f;
                data[i]     = Mathf.Sin(2 * Mathf.PI * frequency * t) * volume * fade;
            }

            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateChime(float baseFreq, int steps, float stepDuration)
        {
            int totalSamples = Mathf.CeilToInt(SampleRate * stepDuration * steps);
            var clip         = AudioClip.Create("chime", totalSamples, 1, SampleRate, false);
            var data         = new float[totalSamples];
            int stepSamples  = Mathf.CeilToInt(SampleRate * stepDuration);

            for (int s = 0; s < steps; s++)
            {
                float freq  = baseFreq * Mathf.Pow(1.25f, s); // rising scale
                int   start = s * stepSamples;
                for (int i = 0; i < stepSamples && start + i < totalSamples; i++)
                {
                    float t    = (float)i / SampleRate;
                    float fade = 1f - (float)i / stepSamples;
                    data[start + i] = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.35f * fade;
                }
            }

            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreatePop(float frequency = 440f)
        {
            float duration = 0.08f;
            int   samples  = Mathf.CeilToInt(SampleRate * duration);
            var   clip     = AudioClip.Create("pop", samples, 1, SampleRate, false);
            var   data     = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t    = (float)i / SampleRate;
                float freq = frequency * (1f - t / duration * 0.4f); // pitch drop
                float fade = 1f - t / duration;
                data[i]    = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.5f * fade;
            }

            clip.SetData(data, 0);
            return clip;
        }
    }
}
