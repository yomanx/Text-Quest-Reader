using UnityEngine;

namespace TextQuestReader.Cinematic.Procedural
{
    /// <summary>
    /// Declarative description of a procedural scene background. Built from
    /// composable UI layers (gradient, stars, scanlines, fog, glow, beams,
    /// pulsing tint). No external assets required.
    /// </summary>
    public struct BackgroundPreset
    {
        public string Id;
        public string Label;

        public Color TopColor;
        public Color BottomColor;

        public bool Stars;
        public int StarCount;
        public Color StarColor;
        public float StarTwinkleSpeed;

        public bool Scanlines;
        public float ScanlineOpacity;

        public bool Fog;
        public Color FogColor;

        public bool Pulse;
        public Color PulseColor;
        public float PulsePeriod;

        public bool RadialGlow;
        public Color RadialGlowColor;
        public float RadialGlowRadius;
        public Vector2 RadialGlowOffset;

        public bool LightBeams;
        public Color BeamColor;
        public int BeamCount;

        public bool Grid;
        public Color GridColor;
        public int GridSpacing;

        public bool Sparkle;
        public Color SparkleColor;

        public bool Vignette;
        public Color VignetteColor;
    }
}
