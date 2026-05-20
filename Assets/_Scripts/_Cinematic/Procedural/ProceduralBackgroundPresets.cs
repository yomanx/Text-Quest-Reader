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
            // Deeper top-to-bottom gradient (more red mass at the top reads
            // as "ceiling emergency strip", darker floor). Sparkle adds
            // restrained red embers drifting in the air.
            TopColor = new Color(0.14f, 0.03f, 0.05f, 1f),
            BottomColor = new Color(0.02f, 0.005f, 0.01f, 1f),
            Pulse = true, PulseColor = new Color(0.98f, 0.18f, 0.22f, 0.22f), PulsePeriod = 1.3f,
            Scanlines = true, ScanlineOpacity = 0.11f,
            LightBeams = true, BeamColor = new Color(1f, 0.22f, 0.28f, 0.16f), BeamCount = 6,
            Sparkle = true, SparkleColor = new Color(1f, 0.45f, 0.30f, 0.55f),
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.62f)
        };

        private static BackgroundPreset BuildTerminalRoom() => new BackgroundPreset
        {
            Id = "terminal_room",
            Label = "MAIN HUB · HOLO MAP",
            // Slightly warmer-blue top, deeper navy bottom — more depth and
            // sky-vs-floor separation. Grid spacing tightened from 36 to 32
            // so the perspective reads denser. Radial glow gets a tiny
            // upward offset to feel like a holo projector hanging above.
            TopColor = new Color(0.03f, 0.07f, 0.14f, 1f),
            BottomColor = new Color(0.005f, 0.02f, 0.06f, 1f),
            Grid = true, GridColor = new Color(0.25f, 0.70f, 1f, 0.22f), GridSpacing = 32,
            RadialGlow = true, RadialGlowColor = new Color(0.25f, 0.72f, 1f, 0.42f), RadialGlowRadius = 0.62f, RadialGlowOffset = new Vector2(0f, 0.08f),
            Scanlines = true, ScanlineOpacity = 0.09f,
            Sparkle = true, SparkleColor = new Color(0.40f, 0.85f, 1f, 0.70f),
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.50f)
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
            // Sterile teal-green gradient, slightly cooler than before so it
            // doesn't look swampy. Added sparkle (drifting med-particles)
            // and stronger scanlines for "diagnostic monitor" feel.
            TopColor = new Color(0.05f, 0.12f, 0.10f, 1f),
            BottomColor = new Color(0.01f, 0.04f, 0.03f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(0.50f, 0.98f, 0.75f, 0.34f), RadialGlowRadius = 0.68f, RadialGlowOffset = new Vector2(0f, 0.04f),
            Pulse = true, PulseColor = new Color(0.35f, 1f, 0.60f, 0.11f), PulsePeriod = 2.2f,
            Sparkle = true, SparkleColor = new Color(0.55f, 1f, 0.85f, 0.55f),
            Scanlines = true, ScanlineOpacity = 0.10f,
            Vignette = true, VignetteColor = new Color(0f, 0.02f, 0.02f, 0.54f)
        };

        private static BackgroundPreset BuildCommsArray() => new BackgroundPreset
        {
            Id = "comms_array",
            Label = "COMMS ARRAY · NOISE",
            // Bumped star count and added sparkle for "signal motes" feel.
            // Slightly more glow so the wall of receivers reads as 'lit
            // racks of equipment' rather than a flat dark room.
            TopColor = new Color(0.03f, 0.05f, 0.08f, 1f),
            BottomColor = new Color(0.005f, 0.015f, 0.025f, 1f),
            Stars = true, StarCount = 120, StarColor = new Color(0.75f, 0.88f, 1f, 0.58f), StarTwinkleSpeed = 1.6f,
            RadialGlow = true, RadialGlowColor = new Color(0.40f, 0.65f, 0.95f, 0.22f), RadialGlowRadius = 0.55f, RadialGlowOffset = new Vector2(0f, 0.05f),
            Sparkle = true, SparkleColor = new Color(0.55f, 0.80f, 1f, 0.50f),
            Scanlines = true, ScanlineOpacity = 0.18f,
            LightBeams = true, BeamColor = new Color(0.55f, 0.78f, 1f, 0.12f), BeamCount = 4,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.62f)
        };

        private static BackgroundPreset BuildStorageDim() => new BackgroundPreset
        {
            Id = "storage_dim",
            Label = "STORAGE · SCRUBBER",
            // Warmer dim top, deeper bottom — feels like a low single-bulb
            // lamp in the upper-left rather than ambient grey. Added a
            // single warm light beam for depth, and very low sparkle for
            // floating dust.
            TopColor = new Color(0.10f, 0.08f, 0.05f, 1f),
            BottomColor = new Color(0.015f, 0.012f, 0.008f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(0.90f, 0.65f, 0.30f, 0.24f), RadialGlowRadius = 0.55f, RadialGlowOffset = new Vector2(-0.10f, 0.05f),
            LightBeams = true, BeamColor = new Color(0.90f, 0.65f, 0.30f, 0.06f), BeamCount = 1,
            Sparkle = true, SparkleColor = new Color(0.95f, 0.80f, 0.55f, 0.30f),
            Scanlines = true, ScanlineOpacity = 0.05f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.70f)
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
            // Tighter grid (22 -> 18) reads as denser rack-of-blades. Top
            // gets a touch more cyan presence for "rack indicator light"
            // ambience. Sparkle bumped slightly — looks like idle drive
            // LEDs still ticking on the few remaining shelves.
            TopColor = new Color(0.025f, 0.06f, 0.10f, 1f),
            BottomColor = new Color(0.005f, 0.015f, 0.035f, 1f),
            Grid = true, GridColor = new Color(0.32f, 0.62f, 0.88f, 0.22f), GridSpacing = 18,
            Scanlines = true, ScanlineOpacity = 0.20f,
            Pulse = true, PulseColor = new Color(0.32f, 0.62f, 0.88f, 0.11f), PulsePeriod = 1.5f,
            Sparkle = true, SparkleColor = new Color(0.50f, 0.80f, 1f, 0.72f),
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.62f)
        };

        private static BackgroundPreset BuildCargoHold() => new BackgroundPreset
        {
            Id = "cargo_hold",
            Label = "CARGO HOLD · SAMPLE CASE OPEN",
            // More saturated industrial amber. Glow shifted slightly closer
            // to centre for "lit container" look. Added sparkle (warm sparks
            // from heavy machinery) and very faint pulse for the breathing
            // emergency lamp.
            TopColor = new Color(0.12f, 0.08f, 0.03f, 1f),
            BottomColor = new Color(0.02f, 0.014f, 0.005f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(1f, 0.58f, 0.18f, 0.34f), RadialGlowRadius = 0.50f, RadialGlowOffset = new Vector2(0.08f, -0.10f),
            Pulse = true, PulseColor = new Color(0.95f, 0.50f, 0.15f, 0.09f), PulsePeriod = 2.6f,
            Sparkle = true, SparkleColor = new Color(1f, 0.70f, 0.30f, 0.55f),
            Scanlines = true, ScanlineOpacity = 0.06f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.68f)
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
            // Reading-lamp warm gradient. Subtle sparkle (drifting dust in
            // the lampbeam) + one slow soft beam for depth. Pulse period
            // bumped to 3.6 s so it feels like a sleeping breath, not a
            // medical monitor.
            TopColor = new Color(0.12f, 0.08f, 0.05f, 1f),
            BottomColor = new Color(0.03f, 0.015f, 0.01f, 1f),
            RadialGlow = true, RadialGlowColor = new Color(1f, 0.65f, 0.32f, 0.28f), RadialGlowRadius = 0.52f, RadialGlowOffset = new Vector2(-0.18f, 0.12f),
            Pulse = true, PulseColor = new Color(0.95f, 0.55f, 0.25f, 0.07f), PulsePeriod = 3.6f,
            LightBeams = true, BeamColor = new Color(1f, 0.65f, 0.30f, 0.05f), BeamCount = 1,
            Sparkle = true, SparkleColor = new Color(1f, 0.78f, 0.45f, 0.32f),
            Scanlines = true, ScanlineOpacity = 0.04f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.68f)
        };

        private static BackgroundPreset BuildAlarmState() => new BackgroundPreset
        {
            Id = "alarm_state",
            Label = "AIRLOCK · PRESSURE CRITICAL",
            // Stronger contrast top-to-bottom (top reads as overhead
            // emergency strobe, floor stays murky). Added a tight red
            // sparkle for sparks / hot debris. Restrained beam count
            // increase (6 -> 7) keeps the strobe feel without overdraw.
            TopColor = new Color(0.22f, 0.02f, 0.04f, 1f),
            BottomColor = new Color(0.025f, 0.00f, 0.005f, 1f),
            Pulse = true, PulseColor = new Color(1f, 0.12f, 0.12f, 0.52f), PulsePeriod = 0.7f,
            LightBeams = true, BeamColor = new Color(1f, 0.14f, 0.14f, 0.20f), BeamCount = 7,
            Sparkle = true, SparkleColor = new Color(1f, 0.35f, 0.25f, 0.65f),
            Scanlines = true, ScanlineOpacity = 0.13f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.58f)
        };

        private static BackgroundPreset BuildBlackVoid() => new BackgroundPreset
        {
            Id = "black_void",
            Label = "—",
            // A handful of very-dim distant stars give scale ("you died,
            // but the universe continues"). Pulse stays slow and deep red,
            // a heart that is still trying.
            TopColor = new Color(0.005f, 0.005f, 0.01f, 1f),
            BottomColor = Color.black,
            Stars = true, StarCount = 30, StarColor = new Color(0.55f, 0.55f, 0.70f, 0.35f), StarTwinkleSpeed = 0.4f,
            Pulse = true, PulseColor = new Color(0.55f, 0.0f, 0.0f, 0.14f), PulsePeriod = 3.4f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.88f)
        };

        private static BackgroundPreset BuildVictoryScene() => new BackgroundPreset
        {
            Id = "victory_scene",
            Label = "ANVIL RELAY · INCOMING",
            // Brighter background gradient (more visible "dawn" feel),
            // bumped star count and brightness, more beams (5 -> from 4),
            // brighter sparkles. Vignette softened so the corners don't
            // crush the hopeful palette.
            TopColor = new Color(0.06f, 0.12f, 0.22f, 1f),
            BottomColor = new Color(0.025f, 0.05f, 0.12f, 1f),
            Stars = true, StarCount = 240, StarColor = new Color(1f, 0.97f, 0.92f, 0.95f), StarTwinkleSpeed = 0.55f,
            RadialGlow = true, RadialGlowColor = new Color(1f, 0.88f, 0.45f, 0.60f), RadialGlowRadius = 0.65f, RadialGlowOffset = new Vector2(0f, 0.12f),
            LightBeams = true, BeamColor = new Color(1f, 0.88f, 0.45f, 0.22f), BeamCount = 5,
            Sparkle = true, SparkleColor = new Color(1f, 0.95f, 0.62f, 0.95f),
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.24f)
        };

        private static BackgroundPreset BuildSecretSignal() => new BackgroundPreset
        {
            Id = "secret_signal",
            Label = "TRUTH · V. LINDGREN'S WAVELENGTH",
            // Deeper violet at the top, slightly more saturated. Beam count
            // up (3 -> 4) and brighter glow so "the signal is alive". Tiny
            // scanline veil added — the truth is being decoded on a CRT
            // somewhere, not floating in pure space.
            TopColor = new Color(0.06f, 0.025f, 0.14f, 1f),
            BottomColor = new Color(0.015f, 0.008f, 0.04f, 1f),
            Stars = true, StarCount = 200, StarColor = new Color(0.90f, 0.78f, 1f, 0.97f), StarTwinkleSpeed = 0.95f,
            RadialGlow = true, RadialGlowColor = new Color(0.78f, 0.50f, 1f, 0.55f), RadialGlowRadius = 0.58f, RadialGlowOffset = new Vector2(0f, 0.06f),
            Pulse = true, PulseColor = new Color(0.78f, 0.50f, 1f, 0.20f), PulsePeriod = 2.2f,
            Sparkle = true, SparkleColor = new Color(0.92f, 0.72f, 1f, 0.95f),
            LightBeams = true, BeamColor = new Color(0.80f, 0.55f, 1f, 0.18f), BeamCount = 4,
            Scanlines = true, ScanlineOpacity = 0.06f,
            Vignette = true, VignetteColor = new Color(0f, 0f, 0f, 0.32f)
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
