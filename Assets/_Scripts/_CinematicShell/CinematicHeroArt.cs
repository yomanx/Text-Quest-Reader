using TextQuestReader.Cinematic.Procedural;
using UnityEngine;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Procedural hero-art generator. Produces a unique-looking Sprite for any
    /// quest by composing simple geometry (station silhouette, asteroid ring,
    /// nebula, terminal frame). The sprite is rendered into a Texture2D at
    /// build time and reused.
    /// </summary>
    public static class CinematicHeroArt
    {
        public static Sprite BuildPreviewFor(QuestShort quest, int width = 512, int height = 320)
        {
            string keyword = (quest?.QuestName ?? "default").ToLowerInvariant();

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[width * height];

            HeroStyle style = ResolveStyle(keyword);
            PaintGradient(pixels, width, height, style.TopColor, style.BottomColor);
            PaintNebula(pixels, width, height, style.NebulaColor, style.NebulaSeed);
            PaintStarfield(pixels, width, height, style.StarCount, style.StarColor);
            PaintRing(pixels, width, height, style);
            PaintSilhouette(pixels, width, height, style);
            PaintGrid(pixels, width, height, style);
            PaintScanlines(pixels, width, height, style);
            PaintVignette(pixels, width, height);

            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private struct HeroStyle
        {
            public Color TopColor, BottomColor;
            public Color NebulaColor;
            public int NebulaSeed;
            public Color StarColor;
            public int StarCount;
            public bool HasRing;
            public Color RingColor;
            public bool HasStation;
            public Color StationColor;
            public bool HasGrid;
            public Color GridColor;
            public bool HasScanlines;
        }

        private static HeroStyle ResolveStyle(string keyword)
        {
            if (keyword.Contains("asteroid") || keyword.Contains("neon"))
                return new HeroStyle
                {
                    TopColor = new Color(0.04f, 0.02f, 0.10f, 1f),
                    BottomColor = new Color(0.12f, 0.04f, 0.20f, 1f),
                    NebulaColor = new Color(0.85f, 0.20f, 0.85f, 0.45f),
                    NebulaSeed = 11,
                    StarColor = new Color(0.85f, 0.95f, 1f, 1f),
                    StarCount = 180,
                    HasRing = true,
                    RingColor = new Color(0.95f, 0.45f, 0.95f, 0.85f),
                    HasStation = true,
                    StationColor = new Color(0.12f, 0.06f, 0.16f, 1f),
                    HasGrid = false,
                    GridColor = new Color(0.5f, 0.85f, 1f, 0.10f),
                    HasScanlines = true
                };
            if (keyword.Contains("space"))
                return new HeroStyle
                {
                    TopColor = new Color(0.02f, 0.05f, 0.10f, 1f),
                    BottomColor = new Color(0.04f, 0.10f, 0.18f, 1f),
                    NebulaColor = new Color(0.30f, 0.70f, 1f, 0.35f),
                    NebulaSeed = 27,
                    StarColor = new Color(0.95f, 0.95f, 1f, 1f),
                    StarCount = 220,
                    HasRing = false,
                    RingColor = Color.clear,
                    HasStation = true,
                    StationColor = new Color(0.04f, 0.10f, 0.18f, 1f),
                    HasGrid = false,
                    GridColor = Color.clear,
                    HasScanlines = false
                };
            return new HeroStyle
            {
                TopColor = new Color(0.04f, 0.06f, 0.10f, 1f),
                BottomColor = new Color(0.02f, 0.04f, 0.08f, 1f),
                NebulaColor = new Color(0.35f, 0.80f, 0.95f, 0.30f),
                NebulaSeed = 42,
                StarColor = new Color(0.75f, 0.90f, 1f, 1f),
                StarCount = 80,
                HasRing = false,
                RingColor = Color.clear,
                HasStation = false,
                StationColor = Color.clear,
                HasGrid = true,
                GridColor = new Color(0.30f, 0.80f, 0.95f, 0.18f),
                HasScanlines = true
            };
        }

        private static void PaintGradient(Color[] pixels, int w, int h, Color top, Color bottom)
        {
            for (int y = 0; y < h; y++)
            {
                float t = 1f - (float)y / (h - 1);
                Color c = Color.Lerp(bottom, top, t);
                for (int x = 0; x < w; x++) pixels[y * w + x] = c;
            }
        }

        private static void PaintNebula(Color[] pixels, int w, int h, Color color, int seed)
        {
            float scale = 0.008f;
            float ox = seed * 0.13f;
            float oy = seed * 0.21f;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float n = Mathf.PerlinNoise(x * scale + ox, y * scale + oy);
                    n = Mathf.Pow(n, 2.2f);
                    Color baseColor = pixels[y * w + x];
                    pixels[y * w + x] = Color.Lerp(baseColor, color, n * color.a);
                }
            }
        }

        private static void PaintStarfield(Color[] pixels, int w, int h, int count, Color color)
        {
            for (int i = 0; i < count; i++)
            {
                int x = Random.Range(0, w);
                int y = Random.Range(0, h);
                float intensity = Random.Range(0.35f, 1f);
                int size = Random.value < 0.85f ? 1 : 2;
                for (int dy = -size; dy <= size; dy++)
                {
                    for (int dx = -size; dx <= size; dx++)
                    {
                        int px = x + dx;
                        int py = y + dy;
                        if (px < 0 || px >= w || py < 0 || py >= h) continue;
                        float falloff = 1f - Mathf.Min(1f, Mathf.Sqrt(dx * dx + dy * dy) / (size + 0.001f));
                        Color baseColor = pixels[py * w + px];
                        Color star = new Color(color.r, color.g, color.b, color.a * intensity * falloff);
                        pixels[py * w + px] = Color.Lerp(baseColor, color, star.a);
                    }
                }
            }
        }

        private static void PaintRing(Color[] pixels, int w, int h, HeroStyle style)
        {
            if (!style.HasRing) return;
            float cx = w * 0.5f;
            float cy = h * 0.55f;
            float radiusX = w * 0.42f;
            float radiusY = h * 0.18f;
            float tilt = -0.18f;
            float innerThickness = 4f;
            float outerThickness = 14f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / radiusX;
                    float dyRaw = (y - cy);
                    float dyTilt = dyRaw + (x - cx) * tilt;
                    float dy = dyTilt / radiusY;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float band = Mathf.Abs(d - 1f);
                    float t = Mathf.SmoothStep(0f, 1f, 1f - Mathf.Clamp01(band * 20f));
                    if (t <= 0f) continue;
                    float alpha = style.RingColor.a * t;
                    Color baseColor = pixels[y * w + x];
                    pixels[y * w + x] = Color.Lerp(baseColor, style.RingColor, alpha);
                }
            }
        }

        private static void PaintSilhouette(Color[] pixels, int w, int h, HeroStyle style)
        {
            if (!style.HasStation) return;
            float baseY = h * 0.52f;
            float centerX = w * 0.5f;
            float bodyHalfW = w * 0.13f;
            float bodyHalfH = h * 0.05f;

            DrawRect(pixels, w, h, (int)(centerX - bodyHalfW), (int)(baseY - bodyHalfH), (int)(bodyHalfW * 2), (int)(bodyHalfH * 2), style.StationColor);
            DrawRect(pixels, w, h, (int)(centerX - bodyHalfW * 1.6f), (int)(baseY - bodyHalfH * 0.4f), (int)(bodyHalfW * 0.6f), (int)(bodyHalfH * 0.8f), style.StationColor);
            DrawRect(pixels, w, h, (int)(centerX + bodyHalfW), (int)(baseY - bodyHalfH * 0.4f), (int)(bodyHalfW * 0.6f), (int)(bodyHalfH * 0.8f), style.StationColor);
            DrawRect(pixels, w, h, (int)(centerX - 2), (int)(baseY - bodyHalfH * 2.6f), 4, (int)(bodyHalfH * 1.6f), style.StationColor);

            Color highlight = new Color(0.95f, 0.55f, 0.20f, 1f);
            DrawRect(pixels, w, h, (int)(centerX - bodyHalfW * 0.5f), (int)(baseY - 2), (int)(bodyHalfW), 2, highlight);
        }

        private static void DrawRect(Color[] pixels, int w, int h, int x, int y, int rectW, int rectH, Color color)
        {
            for (int yi = y; yi < y + rectH; yi++)
            {
                if (yi < 0 || yi >= h) continue;
                for (int xi = x; xi < x + rectW; xi++)
                {
                    if (xi < 0 || xi >= w) continue;
                    pixels[yi * w + xi] = color;
                }
            }
        }

        private static void PaintGrid(Color[] pixels, int w, int h, HeroStyle style)
        {
            if (!style.HasGrid) return;
            for (int y = 0; y < h; y++)
            {
                if (y % 48 != 0) continue;
                for (int x = 0; x < w; x++)
                {
                    Color baseColor = pixels[y * w + x];
                    pixels[y * w + x] = Color.Lerp(baseColor, style.GridColor, style.GridColor.a);
                }
            }
            for (int x = 0; x < w; x++)
            {
                if (x % 48 != 0) continue;
                for (int y = 0; y < h; y++)
                {
                    Color baseColor = pixels[y * w + x];
                    pixels[y * w + x] = Color.Lerp(baseColor, style.GridColor, style.GridColor.a);
                }
            }
        }

        private static void PaintScanlines(Color[] pixels, int w, int h, HeroStyle style)
        {
            if (!style.HasScanlines) return;
            for (int y = 0; y < h; y++)
            {
                if (y % 2 != 0) continue;
                for (int x = 0; x < w; x++)
                {
                    Color baseColor = pixels[y * w + x];
                    pixels[y * w + x] = new Color(baseColor.r * 0.85f, baseColor.g * 0.85f, baseColor.b * 0.85f, baseColor.a);
                }
            }
        }

        private static void PaintVignette(Color[] pixels, int w, int h)
        {
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float maxD = Mathf.Sqrt(cx * cx + cy * cy);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx);
                    float dy = (y - cy);
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / maxD;
                    float strength = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.55f) / 0.45f));
                    Color baseColor = pixels[y * w + x];
                    pixels[y * w + x] = Color.Lerp(baseColor, Color.black, strength * 0.55f);
                }
            }
        }
    }
}
