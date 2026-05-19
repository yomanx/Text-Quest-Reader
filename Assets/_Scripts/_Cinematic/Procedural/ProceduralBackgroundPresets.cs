using System.Collections.Generic;
using UnityEngine;

namespace TextQuestReader.Cinematic.Procedural
{
    /// <summary>
    /// Static catalog of named procedural background presets. Quests reference
    /// these by string (image name in quest.json or via `<bg name bg>` tag).
    /// Unknown names fall back to DeepSpace.
    /// </summary>
    public static class ProceduralBackgroundPresets
    {
        private static Dictionary<string, BackgroundPreset> presets;

        public static BackgroundPreset Get(string id)
        {
            EnsureBuilt();
            if (string.IsNullOrEmpty(id)) return presets["deep_space"];
            string key = id.ToLowerInvariant();
            if (presets.TryGetValue(key, out BackgroundPreset preset))
                return preset;

            string aliased = AliasOf(key);
            if (aliased != null && presets.TryGetValue(aliased, out BackgroundPreset alias))
                return alias;

            return presets["deep_space"];
        }

        public static bool Has(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            EnsureBuilt();
            string key = id.ToLowerInvariant();
            return presets.ContainsKey(key) || AliasOf(key) != null;
        }

        private static string AliasOf(string key)
        {
            switch (key)
            {
                case "preview":
                case "main_hub":         return "terminal_room";
                case "cryo_pod":         return "cryo_pod";
                case "corridor":         return "neon_corridor";
                case "bridge":           return "deep_space";
                case "medbay":           return "med_bay";
                case "reactor":          return "reactor_core";
                case "comms":            return "comms_array";
                case "storage":          return "storage_dim";
                case "hidden_lab":       return "hidden_lab";
                case "server_room":      return "server_room";
                case "cargo":            return "cargo_hold";
                case "observation":      return "observation_deck";
                case "quarters":         return "quarters_warm";
                case "engine_bay":       return "reactor_core";
                case "airlock":          return "alarm_state";
                case "signal_sent":     return "victory_scene";
                case "suffocation":      return "black_void";
                case "truth":            return "secret_signal";
            }
            return null;
        }

        private static void EnsureBuilt()
        {
            if (presets != null) return;
            presets = new Dictionary<string, BackgroundPreset>();
            Register(BuildDeepSpace());
            Register(BuildCryoPod());
            Register(BuildNeonCorridor());
            Register(BuildTerminalRoom());
            Register(BuildReactorCore());
            Register(BuildMedBay());
            Register(BuildCommsArray());
            Register(BuildStorageDim());
            Register(BuildHiddenLab());
            Register(BuildServerRoom());
            Register(BuildCargoHold());
            Register(BuildObservationDeck());
            Register(BuildQuartersWarm());
            Register(BuildAlarmState());
            Register(BuildBlackVoid());
            Register(BuildVictoryScene());
            Register(BuildSecretSignal());
            Register(BuildFailureScene());
        }

        private static void Register(BackgroundPreset preset)
        {
            presets[preset.Id.ToLowerInvariant()] = preset;
        }

        private static BackgroundPreset BuildDeepSpace() => new BackgroundPreset
        {
            Id = "deep_space",
            Label = "DEEP SPACE",
            TopColor = new Color(0.02f, 0.03f, 0.08f, 1f),
            BottomColor = new Color(0.04f, 0.05f, 0.16f, 1f),
            Stars = true, StarCount = 140, StarColor = new Color(0.85f, 0.92f, 1f, 0.9f), StarTwinkleSpeed = 0.7f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.55f),
            Scanlines = true, ScanlineOpacity = 0.05f
        };

        private static BackgroundPreset BuildCryoPod() => new BackgroundPreset
        {
            Id = "cryo_pod",
            Label = "CRYO POD — UNIT 7",
            TopColor = new Color(0.06f, 0.10f, 0.18f, 1f),
            BottomColor = new Color(0.04f, 0.06f, 0.10f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(0.45f, 0.75f, 1f, 0.45f), RadialGlowRadius = 0.7f, RadialGlowOffset = new Vector2(0f, 0.05f),
            Pulse = true, PulseColor = new Color(0.8f, 0.10f, 0.10f, 0.18f), PulsePeriod = 1.8f,
            Scanlines = true, ScanlineOpacity = 0.08f,
            Sparkle = true, SparkleColor = new Color(0.7f, 0.85f, 1f, 0.7f),
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.55f)
        };

        private static BackgroundPreset BuildNeonCorridor() => new BackgroundPreset
        {
            Id = "neon_corridor",
            Label = "CORRIDOR · SECTION B",
            TopColor = new Color(0.10f, 0.04f, 0.06f, 1f),
            BottomColor = new Color(0.03f, 0.01f, 0.02f, 1f),
            Pulse = true, PulseColor = new Color(0.95f, 0.15f, 0.20f, 0.25f), PulsePeriod = 1.4f,
            Scanlines = true, ScanlineOpacity = 0.10f,
            LightBeams = true, BeamColor = new Color(1f, 0.20f, 0.25f, 0.18f), BeamCount = 5,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.65f)
        };

        private static BackgroundPreset BuildTerminalRoom() => new BackgroundPreset
        {
            Id = "terminal_room",
            Label = "MAIN HUB · HOLO MAP",
            TopColor = new Color(0.02f, 0.06f, 0.12f, 1f),
            BottomColor = new Color(0.01f, 0.03f, 0.08f, 1f),
            Grid = true, GridColor = new Color(0.20f, 0.65f, 0.95f, 0.20f), GridSpacing = 36,
            RadialGlow = true, RadialGlowColor = new Color(0.20f, 0.65f, 0.95f, 0.40f), RadialGlowRadius = 0.6f, RadialGlowOffset = Vector2.zero,
            Scanlines = true, ScanlineOpacity = 0.08f,
            Sparkle = true, SparkleColor = new Color(0.3f, 0.8f, 1f, 0.6f),
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.45f)
        };

        private static BackgroundPreset BuildReactorCore() => new BackgroundPreset
        {
            Id = "reactor_core",
            Label = "REACTOR CORE — UNSTABLE",
            TopColor = new Color(0.20f, 0.06f, 0.02f, 1f),
            BottomColor = new Color(0.05f, 0.02f, 0.01f, 1f),
            Pulse = true, PulseColor = new Color(1f, 0.45f, 0.10f, 0.40f), PulsePeriod = 0.9f,
            RadialGlow = true, RadialGlowColor = new Color(1f, 0.45f, 0.10f, 0.55f), RadialGlowRadius = 0.55f, RadialGlowOffset = new Vector2(0f, -0.05f),
            Sparkle = true, SparkleColor = new Color(1f, 0.8f, 0.3f, 0.85f),
            Scanlines = true, ScanlineOpacity = 0.08f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.55f)
        };

        private static BackgroundPreset BuildMedBay() => new BackgroundPreset
        {
            Id = "med_bay",
            Label = "MEDBAY · DECK 3",
            TopColor = new Color(0.06f, 0.10f, 0.08f, 1f),
            BottomColor = new Color(0.02f, 0.04f, 0.03f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(0.45f, 0.95f, 0.70f, 0.30f), RadialGlowRadius = 0.65f, RadialGlowOffset = Vector2.zero,
            Pulse = true, PulseColor = new Color(0.30f, 0.95f, 0.55f, 0.10f), PulsePeriod = 2.4f,
            Scanlines = true, ScanlineOpacity = 0.08f,
            Vignette = true, VignetteColor = new Color(0f, 0.02f, 0.02f, 0.50f)
        };

        private static BackgroundPreset BuildCommsArray() => new BackgroundPreset
        {
            Id = "comms_array",
            Label = "COMMS ARRAY · NOISE",
            TopColor = new Color(0.04f, 0.05f, 0.07f, 1f),
            BottomColor = new Color(0.01f, 0.02f, 0.03f, 1f),
            Stars = true, StarCount = 90, StarColor = new Color(0.7f, 0.85f, 1f, 0.55f), StarTwinkleSpeed = 1.4f,
            Scanlines = true, ScanlineOpacity = 0.16f,
            LightBeams = true, BeamColor = new Color(0.55f, 0.75f, 1f, 0.10f), BeamCount = 3,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.60f)
        };

        private static BackgroundPreset BuildStorageDim() => new BackgroundPreset
        {
            Id = "storage_dim",
            Label = "STORAGE · SCRUBBER",
            TopColor = new Color(0.08f, 0.07f, 0.05f, 1f),
            BottomColor = new Color(0.02f, 0.02f, 0.01f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(0.85f, 0.65f, 0.30f, 0.20f), RadialGlowRadius = 0.55f, RadialGlowOffset = new Vector2(0f, -0.1f),
            Scanlines = true, ScanlineOpacity = 0.06f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.65f)
        };

        private static BackgroundPreset BuildHiddenLab() => new BackgroundPreset
        {
            Id = "hidden_lab",
            Label = "HIDDEN LAB · V. LINDGREN",
            TopColor = new Color(0.08f, 0.05f, 0.14f, 1f),
            BottomColor = new Color(0.02f, 0.01f, 0.05f, 1f),
            Grid = true, GridColor = new Color(0.55f, 0.35f, 0.90f, 0.15f), GridSpacing = 28,
            RadialGlow = true, RadialGlowColor = new Color(0.55f, 0.35f, 0.95f, 0.45f), RadialGlowRadius = 0.55f, RadialGlowOffset = Vector2.zero,
            Sparkle = true, SparkleColor = new Color(0.8f, 0.55f, 1f, 0.85f),
            Pulse = true, PulseColor = new Color(0.55f, 0.30f, 0.95f, 0.20f), PulsePeriod = 2.0f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.55f)
        };

        private static BackgroundPreset BuildServerRoom() => new BackgroundPreset
        {
            Id = "server_room",
            Label = "SERVER RACK · DRIVES MISSING",
            TopColor = new Color(0.03f, 0.05f, 0.08f, 1f),
            BottomColor = new Color(0.01f, 0.02f, 0.04f, 1f),
            Grid = true, GridColor = new Color(0.30f, 0.60f, 0.85f, 0.20f), GridSpacing = 22,
            Scanlines = true, ScanlineOpacity = 0.18f,
            Pulse = true, PulseColor = new Color(0.30f, 0.60f, 0.85f, 0.10f), PulsePeriod = 1.6f,
            Sparkle = true, SparkleColor = new Color(0.45f, 0.75f, 1f, 0.65f),
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.60f)
        };

        private static BackgroundPreset BuildCargoHold() => new BackgroundPreset
        {
            Id = "cargo_hold",
            Label = "CARGO HOLD · SAMPLE CASE OPEN",
            TopColor = new Color(0.10f, 0.08f, 0.04f, 1f),
            BottomColor = new Color(0.03f, 0.02f, 0.01f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(0.95f, 0.55f, 0.15f, 0.30f), RadialGlowRadius = 0.50f, RadialGlowOffset = new Vector2(0.10f, -0.20f),
            Scanlines = true, ScanlineOpacity = 0.05f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.65f)
        };

        private static BackgroundPreset BuildObservationDeck() => new BackgroundPreset
        {
            Id = "observation_deck",
            Label = "OBSERVATION DECK · ASTEROID",
            TopColor = new Color(0.02f, 0.02f, 0.06f, 1f),
            BottomColor = new Color(0.05f, 0.02f, 0.10f, 1f),
            Stars = true, StarCount = 220, StarColor = new Color(0.85f, 0.92f, 1f, 0.95f), StarTwinkleSpeed = 0.5f,
            RadialGlow = true, RadialGlowColor = new Color(0.90f, 0.25f, 0.85f, 0.40f), RadialGlowRadius = 0.45f, RadialGlowOffset = new Vector2(0f, -0.30f),
            Pulse = true, PulseColor = new Color(0.95f, 0.25f, 0.90f, 0.15f), PulsePeriod = 1.5f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.45f)
        };

        private static BackgroundPreset BuildQuartersWarm() => new BackgroundPreset
        {
            Id = "quarters_warm",
            Label = "CREW QUARTERS · BUNK 6",
            TopColor = new Color(0.10f, 0.07f, 0.05f, 1f),
            BottomColor = new Color(0.04f, 0.02f, 0.02f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(0.95f, 0.60f, 0.30f, 0.25f), RadialGlowRadius = 0.5f, RadialGlowOffset = new Vector2(-0.15f, 0.10f),
            Pulse = true, PulseColor = new Color(0.95f, 0.55f, 0.25f, 0.08f), PulsePeriod = 3.2f,
            Scanlines = true, ScanlineOpacity = 0.04f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.65f)
        };

        private static BackgroundPreset BuildAlarmState() => new BackgroundPreset
        {
            Id = "alarm_state",
            Label = "AIRLOCK · PRESSURE CRITICAL",
            TopColor = new Color(0.18f, 0.02f, 0.04f, 1f),
            BottomColor = new Color(0.05f, 0.00f, 0.01f, 1f),
            Pulse = true, PulseColor = new Color(1f, 0.10f, 0.10f, 0.50f), PulsePeriod = 0.7f,
            LightBeams = true, BeamColor = new Color(1f, 0.10f, 0.10f, 0.18f), BeamCount = 6,
            Scanlines = true, ScanlineOpacity = 0.12f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.55f)
        };

        private static BackgroundPreset BuildBlackVoid() => new BackgroundPreset
        {
            Id = "black_void",
            Label = "—",
            TopColor = Color.black,
            BottomColor = Color.black,
            Pulse = true, PulseColor = new Color(0.6f, 0.0f, 0.0f, 0.15f), PulsePeriod = 3.0f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.85f)
        };

        private static BackgroundPreset BuildVictoryScene() => new BackgroundPreset
        {
            Id = "victory_scene",
            Label = "ANVIL RELAY · INCOMING",
            TopColor = new Color(0.05f, 0.10f, 0.18f, 1f),
            BottomColor = new Color(0.02f, 0.04f, 0.10f, 1f),
            Stars = true, StarCount = 200, StarColor = new Color(0.95f, 0.95f, 0.90f, 0.95f), StarTwinkleSpeed = 0.6f,
            RadialGlow = true, RadialGlowColor = new Color(0.95f, 0.85f, 0.40f, 0.55f), RadialGlowRadius = 0.6f, RadialGlowOffset = new Vector2(0f, 0.10f),
            LightBeams = true, BeamColor = new Color(0.95f, 0.85f, 0.40f, 0.18f), BeamCount = 4,
            Sparkle = true, SparkleColor = new Color(0.95f, 0.90f, 0.55f, 0.85f),
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.30f)
        };

        private static BackgroundPreset BuildSecretSignal() => new BackgroundPreset
        {
            Id = "secret_signal",
            Label = "TRUTH · V. LINDGREN'S WAVELENGTH",
            TopColor = new Color(0.05f, 0.02f, 0.12f, 1f),
            BottomColor = new Color(0.02f, 0.01f, 0.05f, 1f),
            Stars = true, StarCount = 180, StarColor = new Color(0.85f, 0.75f, 1f, 0.95f), StarTwinkleSpeed = 0.9f,
            RadialGlow = true, RadialGlowColor = new Color(0.70f, 0.45f, 1f, 0.50f), RadialGlowRadius = 0.55f, RadialGlowOffset = Vector2.zero,
            Pulse = true, PulseColor = new Color(0.70f, 0.45f, 1f, 0.18f), PulsePeriod = 2.4f,
            Sparkle = true, SparkleColor = new Color(0.85f, 0.65f, 1f, 0.90f),
            LightBeams = true, BeamColor = new Color(0.75f, 0.50f, 1f, 0.15f), BeamCount = 3,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.35f)
        };

        private static BackgroundPreset BuildFailureScene() => new BackgroundPreset
        {
            Id = "failure_scene",
            Label = "TERMINATED",
            TopColor = new Color(0.08f, 0.02f, 0.02f, 1f),
            BottomColor = new Color(0.02f, 0.00f, 0.00f, 1f),
            Pulse = true, PulseColor = new Color(0.6f, 0.05f, 0.05f, 0.25f), PulsePeriod = 1.6f,
            Scanlines = true, ScanlineOpacity = 0.10f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.85f)
        };
    }
}
