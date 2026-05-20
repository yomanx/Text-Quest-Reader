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
                // Legacy keys
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
                // SpaceStation_en theme-pass keys
                case "alarm_soft":
                case "corridor_hum":
                case "crate_rattle":
                case "gas_hiss":
                case "airlock_idle":
                case "terminal_buzz":
                case "psychic_dread":
                case "scrape_near":
                case "body_discovery":
                case "med_scan":
                case "bunk_hum":
                case "sleep_muffle":
                case "crate_crash":
                case "dull_clunk":
                case "weapon_find":
                case "paper_pickup":
                case "decompression":
                case "rescue_open":
                case "ammo_pickup":
                case "seal_hiss":
                case "keycard_pickup":
                case "map_beep":
                case "monster_roar":
                case "cabinet_unlock":
                case "heal_wrap":
                case "radio_contact":
                case "hope_rise":
                case "radio_cut":
                    return true;
            }
            return false;
        }

        private static AudioClip CreateForName(string key)
        {
            switch (key)
            {
                // === Legacy keys (already in upstream) ===
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

                // === SpaceStation_en theme-pass keys ===
                // All amplitudes kept <= 0.45 peak to avoid clipping.

                // Atmosphere — long, dim, never startling
                case "alarm_soft":
                    return BuildAlarmClip(durationSeconds: 1.1f, centerHz: 420f, swingHz: 140f, lfoHz: 2.2f, gain: 0.32f);
                case "corridor_hum":
                    return BuildToneClip(frequencyHz: 70f, durationSeconds: 1.0f, attack: 0.10f, release: 0.55f, noiseAmount: 0.04f);
                case "airlock_idle":
                    return BuildToneClip(frequencyHz: 55f, durationSeconds: 1.4f, attack: 0.12f, release: 0.70f, noiseAmount: 0.03f);
                case "terminal_buzz":
                    return BuildToneClip(frequencyHz: 130f, durationSeconds: 0.6f, attack: 0.04f, release: 0.30f, noiseAmount: 0.10f);
                case "psychic_dread":
                    return BuildDreadClip(durationSeconds: 1.8f);
                case "bunk_hum":
                    return BuildToneClip(frequencyHz: 80f, durationSeconds: 0.9f, attack: 0.12f, release: 0.60f, noiseAmount: 0.03f);
                case "sleep_muffle":
                    return BuildHissClip(durationSeconds: 0.9f, noiseAmount: 0.08f);

                // Discovery / interaction beats
                case "crate_rattle":
                    return BuildRattleClip(durationSeconds: 0.55f);
                case "gas_hiss":
                    return BuildHissClip(durationSeconds: 0.8f, noiseAmount: 0.32f);
                case "scrape_near":
                    return BuildScrapeClip(durationSeconds: 0.5f);
                case "body_discovery":
                    return BuildSweepClip(startHz: 220f, endHz: 90f, durationSeconds: 0.7f);
                case "med_scan":
                    return BuildBeepClip(frequencyHz: 660f, durationSeconds: 0.10f, beepGap: 0.06f, beeps: 2);
                case "crate_crash":
                    return BuildImpactClip(durationSeconds: 0.55f);
                case "dull_clunk":
                    return BuildImpactClip(durationSeconds: 0.30f);
                case "weapon_find":
                    return BuildSweepClip(startHz: 280f, endHz: 760f, durationSeconds: 0.30f);
                case "paper_pickup":
                    return BuildHissClip(durationSeconds: 0.30f, noiseAmount: 0.15f);
                case "ammo_pickup":
                    return BuildBeepClip(frequencyHz: 920f, durationSeconds: 0.08f, beepGap: 0.05f, beeps: 2);
                case "seal_hiss":
                    return BuildHissClip(durationSeconds: 0.35f, noiseAmount: 0.28f);
                case "keycard_pickup":
                    return BuildBeepClip(frequencyHz: 1320f, durationSeconds: 0.08f, beepGap: 0f, beeps: 1);
                case "map_beep":
                    return BuildBeepClip(frequencyHz: 1050f, durationSeconds: 0.09f, beepGap: 0f, beeps: 1);
                case "cabinet_unlock":
                    return BuildChordClip(new[] { 660f, 880f }, durationSeconds: 0.28f);
                case "heal_wrap":
                    return BuildSweepClip(startHz: 320f, endHz: 540f, durationSeconds: 0.6f);

                // Climax beats — louder but still under 0.45
                case "decompression":
                    return BuildDecompressionClip(durationSeconds: 1.5f);
                case "monster_roar":
                    return BuildRoarClip(durationSeconds: 0.9f);

                // Comms finale
                case "radio_contact":
                    return BuildContactClip();
                case "hope_rise":
                    return BuildSweepClip(startHz: 280f, endHz: 880f, durationSeconds: 1.0f);
                case "radio_cut":
                    return BuildSweepClip(startHz: 600f, endHz: 60f, durationSeconds: 0.55f);

                // Victory / rescue
                case "rescue_open":
                    return BuildChordClip(new[] { 523f, 659f, 784f }, durationSeconds: 0.65f);
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
            return BuildAlarmClip(1.0f, 600f, 280f, 4.5f, 0.45f);
        }

        private static AudioClip BuildAlarmClip(float durationSeconds, float centerHz, float swingHz, float lfoHz, float gain)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                float freq = centerHz + swingHz * Mathf.Sin(2f * Mathf.PI * lfoHz * t);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float envelope = Envelope(i, total, 0.04f, 0.30f, sampleRate);
                data[i] = wave * envelope * gain;
            }
            AudioClip clip = AudioClip.Create("alarm_" + centerHz, total, 1, sampleRate, false);
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

        // ----- additional builders for the SpaceStation theme pass ----- //

        // Slow, detuned low-frequency drone with subtle wobble. Used for
        // psychic_dread — eerie but not loud (peak 0.35).
        private static AudioClip BuildDreadClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                float baseFreq = 68f + 6f * Mathf.Sin(2f * Mathf.PI * 0.3f * t);
                float wave = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.55f
                             + Mathf.Sin(2f * Mathf.PI * baseFreq * 1.5f * t) * 0.18f;
                float noise = (Random.value - 0.5f) * 0.08f;
                float envelope = Envelope(i, total, 0.20f, 0.50f, sampleRate);
                data[i] = (wave + noise) * envelope * 0.35f;
            }
            AudioClip clip = AudioClip.Create("dread", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Quick metallic rattle — 3-4 short noise bursts in rapid succession.
        private static AudioClip BuildRattleClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            int bursts = 4;
            int burstLen = total / (bursts * 2);
            float prev = 0f;
            for (int b = 0; b < bursts; b++)
            {
                int start = b * (burstLen * 2);
                for (int i = 0; i < burstLen && start + i < total; i++)
                {
                    float noise = (Random.value - 0.5f) * 2f * 0.6f;
                    prev = Mathf.Lerp(prev, noise, 0.7f);
                    float envelope = Envelope(i, burstLen, 0.002f, 0.04f, sampleRate);
                    float metallic = Mathf.Sin(2f * Mathf.PI * 1800f * ((float)i / sampleRate)) * 0.15f;
                    data[start + i] = (prev + metallic) * envelope * 0.40f;
                }
            }
            AudioClip clip = AudioClip.Create("rattle", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Slow wood-on-metal scrape — filtered noise modulated by a low tone.
        private static AudioClip BuildScrapeClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            float prev = 0f;
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                float noise = (Random.value - 0.5f) * 2f * 0.45f;
                prev = Mathf.Lerp(prev, noise, 0.25f);
                float wobble = Mathf.Sin(2f * Mathf.PI * 18f * t);
                float envelope = Envelope(i, total, 0.05f, 0.20f, sampleRate);
                data[i] = prev * (0.6f + 0.4f * wobble) * envelope * 0.42f;
            }
            AudioClip clip = AudioClip.Create("scrape", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Short impact — heavy low rumble + tight noise transient.
        private static AudioClip BuildImpactClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            float prev = 0f;
            for (int i = 0; i < total; i++)
            {
                float k = (float)i / total;
                float noise = (Random.value - 0.5f) * 2f * (1f - k);
                prev = Mathf.Lerp(prev, noise, 0.55f);
                float rumble = Mathf.Sin(2f * Mathf.PI * 55f * ((float)i / sampleRate)) * 0.35f * (1f - k);
                float envelope = Envelope(i, total, 0.002f, 0.35f, sampleRate);
                data[i] = (prev * 0.6f + rumble) * envelope * 0.45f;
            }
            AudioClip clip = AudioClip.Create("impact", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Long, heavy "the hull opens" — low rumble + descending noise band.
        private static AudioClip BuildDecompressionClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            float prev = 0f;
            for (int i = 0; i < total; i++)
            {
                float k = (float)i / total;
                float t = (float)i / sampleRate;
                float rumbleFreq = Mathf.Lerp(60f, 25f, k);
                float rumble = Mathf.Sin(2f * Mathf.PI * rumbleFreq * t) * 0.35f;
                float noise = (Random.value - 0.5f) * 2f * Mathf.Lerp(0.50f, 0.20f, k);
                prev = Mathf.Lerp(prev, noise, 0.30f);
                float envelope = Envelope(i, total, 0.05f, 0.60f, sampleRate);
                data[i] = (rumble + prev) * envelope * 0.45f;
            }
            AudioClip clip = AudioClip.Create("decompression", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Descending growl — low, gravelly. Peak well below 0.45.
        private static AudioClip BuildRoarClip(float durationSeconds)
        {
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            float phase = 0f;
            for (int i = 0; i < total; i++)
            {
                float k = (float)i / total;
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(180f, 80f, k);
                phase += 2f * Mathf.PI * freq / sampleRate;
                float wobble = Mathf.Sin(2f * Mathf.PI * 12f * t);
                float wave = Mathf.Sin(phase) * (0.55f + 0.20f * wobble);
                float noise = (Random.value - 0.5f) * 0.20f * (1f - 0.4f * k);
                float envelope = Envelope(i, total, 0.04f, 0.35f, sampleRate);
                data[i] = (wave + noise) * envelope * 0.42f;
            }
            AudioClip clip = AudioClip.Create("roar", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Short ping + brief noise tail — "the radio caught something".
        private static AudioClip BuildContactClip()
        {
            int sampleRate = 44100;
            int total = (int)(0.55f * sampleRate);
            float[] data = new float[total];
            int pingLen = (int)(0.10f * sampleRate);
            for (int i = 0; i < pingLen && i < total; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Envelope(i, pingLen, 0.005f, 0.07f, sampleRate);
                data[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * envelope * 0.42f;
            }
            float prev = 0f;
            for (int i = pingLen; i < total; i++)
            {
                float noise = (Random.value - 0.5f) * 2f * 0.30f;
                prev = Mathf.Lerp(prev, noise, 0.3f);
                float envelope = Envelope(i - pingLen, total - pingLen, 0.02f, 0.25f, sampleRate);
                data[i] = prev * envelope * 0.35f;
            }
            AudioClip clip = AudioClip.Create("contact", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Stack of sinusoidal notes summed and normalised — short triumphant
        // chord. Used for rescue_open and cabinet_unlock.
        private static AudioClip BuildChordClip(float[] notes, float durationSeconds)
        {
            if (notes == null || notes.Length == 0) return null;
            int sampleRate = 44100;
            int total = Mathf.Max(1, (int)(durationSeconds * sampleRate));
            float[] data = new float[total];
            float perNote = 1f / notes.Length;
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                float sum = 0f;
                for (int n = 0; n < notes.Length; n++)
                    sum += Mathf.Sin(2f * Mathf.PI * notes[n] * t) * perNote;
                float envelope = Envelope(i, total, 0.01f, 0.30f, sampleRate);
                data[i] = sum * envelope * 0.45f;
            }
            AudioClip clip = AudioClip.Create("chord_" + notes.Length, total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
