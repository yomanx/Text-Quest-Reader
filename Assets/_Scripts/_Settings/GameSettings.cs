using System;
using UnityEngine;

namespace TextQuestReader.Settings
{
    public static class GameSettings
    {
        public const float DefaultMasterVolume = 1f;
        public const float DefaultMusicVolume = 0.6f;
        public const float DefaultSfxVolume = 0.8f;
        public const float DefaultTextSpeed = 1f;
        public const float MinTextSpeed = 0.25f;
        public const float MaxTextSpeed = 4f;

        public static event Action AudioVolumesChanged;
        public static event Action TextSpeedChanged;
        public static event Action VisualEffectsChanged;

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(SettingsKeys.MasterVolume, DefaultMasterVolume);
            set
            {
                PlayerPrefs.SetFloat(SettingsKeys.MasterVolume, Mathf.Clamp01(value));
                PlayerPrefs.Save();
                AudioVolumesChanged?.Invoke();
            }
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(SettingsKeys.MusicVolume, DefaultMusicVolume);
            set
            {
                PlayerPrefs.SetFloat(SettingsKeys.MusicVolume, Mathf.Clamp01(value));
                PlayerPrefs.Save();
                AudioVolumesChanged?.Invoke();
            }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SettingsKeys.SfxVolume, DefaultSfxVolume);
            set
            {
                PlayerPrefs.SetFloat(SettingsKeys.SfxVolume, Mathf.Clamp01(value));
                PlayerPrefs.Save();
                AudioVolumesChanged?.Invoke();
            }
        }

        public static float TextSpeed
        {
            get => PlayerPrefs.GetFloat(SettingsKeys.TextSpeed, DefaultTextSpeed);
            set
            {
                PlayerPrefs.SetFloat(SettingsKeys.TextSpeed, Mathf.Clamp(value, MinTextSpeed, MaxTextSpeed));
                PlayerPrefs.Save();
                TextSpeedChanged?.Invoke();
            }
        }

        public static bool TypewriterSfxEnabled
        {
            get => PlayerPrefs.GetInt(SettingsKeys.TypewriterSfxEnabled, 0) == 1;
            set { PlayerPrefs.SetInt(SettingsKeys.TypewriterSfxEnabled, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool ScreenEffectsEnabled
        {
            get => PlayerPrefs.GetInt(SettingsKeys.ScreenEffectsEnabled, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SettingsKeys.ScreenEffectsEnabled, value ? 1 : 0);
                PlayerPrefs.Save();
                VisualEffectsChanged?.Invoke();
            }
        }

        public static bool ScreenShakeEnabled
        {
            get => PlayerPrefs.GetInt(SettingsKeys.ScreenShakeEnabled, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SettingsKeys.ScreenShakeEnabled, value ? 1 : 0);
                PlayerPrefs.Save();
                VisualEffectsChanged?.Invoke();
            }
        }

        public static bool SceneFadeEnabled
        {
            get => PlayerPrefs.GetInt(SettingsKeys.SceneFadeEnabled, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SettingsKeys.SceneFadeEnabled, value ? 1 : 0);
                PlayerPrefs.Save();
                VisualEffectsChanged?.Invoke();
            }
        }

        public static float EffectiveMusicVolume => Mathf.Clamp01(MasterVolume * MusicVolume);
        public static float EffectiveSfxVolume => Mathf.Clamp01(MasterVolume * SfxVolume);
    }
}
