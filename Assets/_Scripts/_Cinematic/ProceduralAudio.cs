using System.Collections.Generic;
using UnityEngine;

namespace TextQuestReader.Cinematic
{
    /// <summary>
    /// Generates short procedural AudioClips at runtime — beeps, clicks,
    /// hisses, alarms. Used as a fallback when a quest references an SFX
    /// file that doesn't exist on disk, so cinematic moments still have
    /// audible punctuation.
    /// </summary>
    public static class ProceduralAudio
    {
        private static readonly Dictionary<string, AudioClip> cache = new();

        public static AudioClip GetClip(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            string key = name.ToLowerInvariant();
            if (cache.TryGetValue(key, out AudioClip cached) && cached != null)
                return cached;

            AudioClip clip = CreateForName(key);
            if (clip != null) cache[key] = clip;
            return clip;
        }

        public static bool HasMappingFor(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string key = name.ToLowerInvariant();
            switch (key)
            {
                case "alarm":
                case "warning_klaxon":
                case "door_hiss":
                case "console_hum":
                case "heart_monitor":
                case "radio_static":
                case "paper_rustle":
                case "med_inject":
                case "explosion_small":
                case "soft_static":
                case "click":
                case "beep":
                case "tada":
                    return true;
            }
            return false;
        }

        private static AudioClip CreateForName(string key)
        {
            switch (key)
            {
                case "alarm":
                case "warning_klaxon":
                    return BuildAlarmClip();
                case "door_hiss":
                    return BuildHissClip(durationSeconds: 0.9f, noiseAmount: 0.25f);
                case "console_hum":
                    return BuildToneClip(frequencyHz: 110f, durationSeconds: 0.6f, attack: 0.05f, release: 0.4f, noiseAmount: 0.05f);
                case "heart_monitor":
                    return BuildBeepClip(frequencyHz: 880f, durationSeconds: 0.18f, beepGap: 0.0f, beeps: 1);
                case "radio_static":
                case "soft_static":
                    return BuildHissClip(durationSeconds: 1.2f, noiseAmount: 0.35f);
                case "paper_rustle":
                    return BuildHissClip(durationSeconds: 0.4f, noiseAmount: 0.18f);
                case "med_inject":
                    return BuildSweepClip(startHz: 220f, endHz: 880f, durationSeconds: 0.35f);
                case "explosion_small":
                    return BuildExplosionClip();
                case "click":
                    return BuildBeepClip(frequencyHz: 1400f, durationSeconds: 0.05f, beepGap: 0f, beeps: 1);
                case "beep":
                    return BuildBeepClip(frequencyHz: 1200f, durationSeconds: 0.10f, beepGap: 0f, beeps: 1);
                case "tada":
                    return BuildTadaClip();
            }
            return null;
        }

        private static AudioClip BuildBeepClip(float frequencyHz, float durationSeconds, float beepGap, int beeps)
        {
            int sampleRate = 44100;
            int beepSamples = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            int gapSamples = Mathf.Max(0, (int)(beepGap * sampleRate));
            int total = (beepSamples + gapSamples) * beeps;
            float[] data = new float[total];
            int index = 0;
            for (int b = 0; b < beeps; b++)
            {
                for (int i = 0; i < beepSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Envelope(i, beepSamples, attack: 0.005f, release: 0.05f, sampleRate);
                    float wave = Mathf.Sin(2f * Mathf.PI * frequencyHz * t);
                    data[index++] = wave * envelope * 0.45f;
                }
                for (int g = 0; g < gapSamples && index < total; g++)
                    data[index++] = 0f;
            }
            AudioClip clip = AudioClip.Create("beep_" + frequencyHz, total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildToneClip(float frequencyHz, float durationSeconds, float attack, float release, float noiseAmount)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * frequencyHz * t) * 0.5f
                             + Mathf.Sin(2f * Mathf.PI * frequencyHz * 2f * t) * 0.2f;
                float noise = (Random.value - 0.5f) * 2f * noiseAmount;
                float envelope = Envelope(i, total, attack, release, sampleRate);
                data[i] = (wave + noise) * envelope * 0.45f;
            }
            AudioClip clip = AudioClip.Create("tone_" + frequencyHz, total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildHissClip(float durationSeconds, float noiseAmount)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            float prev = 0f;
            for (int i = 0; i < total; i++)
            {
                float noise = (Random.value - 0.5f) * 2f * noiseAmount;
                prev = Mathf.Lerp(prev, noise, 0.3f);
                float envelope = Envelope(i, total, 0.02f, 0.2f, sampleRate);
                data[i] = prev * envelope;
            }
            AudioClip clip = AudioClip.Create("hiss", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildAlarmClip()
        {
            int sampleRate = 44100;
            int total = sampleRate;
            float[] data = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                float freq = 600f + 280f * Mathf.Sin(2f * Mathf.PI * 4.5f * t);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float envelope = Envelope(i, total, 0.02f, 0.3f, sampleRate);
                data[i] = wave * envelope * 0.45f;
            }
            AudioClip clip = AudioClip.Create("alarm", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildSweepClip(float startHz, float endHz, float durationSeconds)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            float phase = 0f;
            for (int i = 0; i < total; i++)
            {
                float k = (float)i / total;
                float freq = Mathf.Lerp(startHz, endHz, k);
                phase += 2f * Mathf.PI * freq / sampleRate;
                float envelope = Envelope(i, total, 0.01f, 0.1f, sampleRate);
                data[i] = Mathf.Sin(phase) * envelope * 0.5f;
            }
            AudioClip clip = AudioClip.Create("sweep", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildExplosionClip()
        {
            int sampleRate = 44100;
            int total = (int)(0.7f * sampleRate);
            float[] data = new float[total];
            float prev = 0f;
            for (int i = 0; i < total; i++)
            {
                float k = (float)i / total;
                float noise = (Random.value - 0.5f) * 2f * (1f - k);
                prev = Mathf.Lerp(prev, noise, 0.5f);
                float rumble = Mathf.Sin(2f * Mathf.PI * 70f * ((float)i / sampleRate)) * 0.5f * (1f - k);
                float envelope = Envelope(i, total, 0.005f, 0.5f, sampleRate);
                data[i] = (prev + rumble) * envelope * 0.6f;
            }
            AudioClip clip = AudioClip.Create("explosion", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildTadaClip()
        {
            int sampleRate = 44100;
            int total = (int)(0.9f * sampleRate);
            float[] data = new float[total];
            float[] notes = { 523f, 659f, 784f, 1047f };
            int notesCount = notes.Length;
            int per = total / notesCount;
            int index = 0;
            for (int n = 0; n < notesCount; n++)
            {
                for (int i = 0; i < per && index < total; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Envelope(i, per, 0.005f, 0.15f, sampleRate);
                    data[index++] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * envelope * 0.45f;
                }
            }
            AudioClip clip = AudioClip.Create("tada", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Envelope(int sample, int total, float attack, float release, int sampleRate)
        {
            float t = (float)sample / sampleRate;
            float lifetime = (float)total / sampleRate;
            float attackEnv = Mathf.Clamp01(t / Mathf.Max(attack, 0.0005f));
            float remaining = lifetime - t;
            float releaseEnv = Mathf.Clamp01(remaining / Mathf.Max(release, 0.001f));
            return Mathf.Min(attackEnv, releaseEnv);
        }
    }
}
