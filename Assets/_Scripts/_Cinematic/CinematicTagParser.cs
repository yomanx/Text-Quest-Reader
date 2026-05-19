using System.Collections.Generic;
using UnityEngine;

namespace TextQuestReader.Cinematic
{
    public enum CinematicEffectKind
    {
        Unknown,
        Fade,
        Shake,
        Flash,
        Vignette,
        Pulse,
        Glitch,
        Tint,
        Overlay,
        Stop
    }

    public class CinematicTagInfo
    {
        public CinematicEffectKind Kind;
        public string RawValue;
        public List<string> Args = new List<string>();
        public string Sub => Args.Count > 0 ? Args[0] : string.Empty;
        public float NumberAt(int index, float fallback)
        {
            if (index < 0 || index >= Args.Count) return fallback;
            return float.TryParse(Args[index], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : fallback;
        }

        public string StringAt(int index, string fallback = "")
        {
            return index >= 0 && index < Args.Count ? Args[index] : fallback;
        }

        public Color ColorAt(int index, Color fallback)
        {
            if (index < 0 || index >= Args.Count) return fallback;
            return ColorUtility.TryParseHtmlString(Args[index].StartsWith("#") ? Args[index] : "#" + Args[index], out Color c) ? c : fallback;
        }
    }

    public static class CinematicTagParser
    {
        private static readonly string[] supportedTags = { "fx", "flash", "shake", "vignette", "pulse", "glitch", "tint", "overlay", "fade" };

        public static List<CinematicTagInfo> ExtractFromText(ref string text, TextParser textParser)
        {
            List<CinematicTagInfo> result = new List<CinematicTagInfo>();

            if (string.IsNullOrEmpty(text) || textParser == null)
                return result;

            foreach (string tag in supportedTags)
            {
                List<string> values = textParser.ExtractAllTagValues(ref text, tag);

                foreach (string raw in values)
                {
                    CinematicTagInfo info = ParseValue(tag, raw);

                    if (info != null)
                        result.Add(info);
                }
            }

            return result;
        }

        private static CinematicTagInfo ParseValue(string tag, string raw)
        {
            CinematicEffectKind kind = MapTagToKind(tag);

            string[] parts = raw.Split(new[] { ' ', ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return null;

            CinematicTagInfo info = new CinematicTagInfo
            {
                Kind = kind,
                RawValue = raw
            };

            int argStart = 0;

            if (kind == CinematicEffectKind.Unknown)
            {
                CinematicEffectKind inner = MapTagToKind(parts[0].ToLowerInvariant());
                if (inner != CinematicEffectKind.Unknown)
                {
                    info.Kind = inner;
                    argStart = 1;
                }
                else if (parts[0].Equals("stop", System.StringComparison.OrdinalIgnoreCase))
                {
                    info.Kind = CinematicEffectKind.Stop;
                    argStart = 1;
                }
                else
                {
                    return null;
                }
            }

            for (int i = argStart; i < parts.Length; i++)
                info.Args.Add(parts[i]);

            return info;
        }

        private static CinematicEffectKind MapTagToKind(string tag)
        {
            switch (tag)
            {
                case "fx": return CinematicEffectKind.Unknown;
                case "shake": return CinematicEffectKind.Shake;
                case "flash": return CinematicEffectKind.Flash;
                case "vignette": return CinematicEffectKind.Vignette;
                case "pulse": return CinematicEffectKind.Pulse;
                case "glitch": return CinematicEffectKind.Glitch;
                case "tint": return CinematicEffectKind.Tint;
                case "overlay": return CinematicEffectKind.Overlay;
                case "fade": return CinematicEffectKind.Fade;
                default: return CinematicEffectKind.Unknown;
            }
        }
    }
}
