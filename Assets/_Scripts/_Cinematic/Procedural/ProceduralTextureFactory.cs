using UnityEngine;

namespace TextQuestReader.Cinematic.Procedural
{
    /// <summary>
    /// Builds Texture2D / Sprite assets at runtime — no external PNG required.
    /// Used by layer components for gradients, radial glows, beam masks, etc.
    /// Generated textures are tagged hideFlags = DontSave so the editor stays
    /// clean, and callers are expected to Destroy them on teardown.
    /// </summary>
    public static class ProceduralTextureFactory
    {
        public static Sprite CreateSolidSprite(Color color)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = { color, color, color, color };
            tex.SetPixels(pixels);
            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite CreateVerticalGradientSprite(Color top, Color bottom, int height = 256)
        {
            int width = 4;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float t = 1f - (float)y / (height - 1);
                Color c = Color.Lerp(bottom, top, t);
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = c;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite CreateRadialGlowSprite(Color color, float falloff = 1.5f, int size = 256)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(1f - d);
                    alpha = Mathf.Pow(alpha, falloff);
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite CreateVignetteSprite(Color color, int size = 256)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                    float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.5f) / 0.5f));
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite CreateScanlinesSprite(float opacity, int height = 4)
        {
            int width = 4;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                bool dark = (y % 2) == 0;
                Color c = dark ? new Color(0f, 0f, 0f, opacity) : new Color(0f, 0f, 0f, 0f);
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = c;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        public static Sprite CreateGridSprite(Color color, int spacing, int lineThickness = 1)
        {
            int size = Mathf.Max(8, spacing);
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            Color empty = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isLine = x < lineThickness || y < lineThickness;
                    pixels[y * size + x] = isLine ? color : empty;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        public static Sprite CreateBeamSprite(Color color, int width = 16, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float ty = (float)y / (height - 1);
                float vertical = Mathf.Sin(ty * Mathf.PI);
                for (int x = 0; x < width; x++)
                {
                    float tx = (x - width * 0.5f) / (width * 0.5f);
                    float horizontal = Mathf.Clamp01(1f - Mathf.Abs(tx));
                    horizontal = Mathf.Pow(horizontal, 2.2f);
                    float alpha = vertical * horizontal * color.a;
                    pixels[y * width + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return ToSprite(tex);
        }

        public static Sprite CreateNoiseSprite(float noiseScale, Color tint, int size = 128)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            float seed = Random.Range(0f, 100f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * noiseScale + seed, y * noiseScale + seed);
                    float alpha = tint.a * n;
                    pixels[y * size + x] = new Color(tint.r, tint.g, tint.b, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        public static Sprite CreateRoundedRectSprite(Color color, int radius = 16, int size = 64)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(0f, radius - x);
                    float dy = Mathf.Max(0f, radius - y);
                    float dxR = Mathf.Max(0f, x - (size - 1 - radius));
                    float dyR = Mathf.Max(0f, y - (size - 1 - radius));
                    float fx = Mathf.Max(dx, dxR);
                    float fy = Mathf.Max(dy, dyR);
                    float d = Mathf.Sqrt(fx * fx + fy * fy);
                    float alpha = Mathf.Clamp01(1f - (d / radius));
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        public static Sprite CreateCornerBracketSprite(Color color, int thickness = 3, int length = 32, int size = 48)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.DontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            Color empty = new Color(0f, 0f, 0f, 0f);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = empty;
            for (int i = 0; i < length; i++)
            {
                for (int t = 0; t < thickness; t++)
                {
                    if (i < size && t < size) pixels[t * size + i] = color;
                    if (t < size && i < size) pixels[i * size + t] = color;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0f, 0f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static Sprite ToSprite(Texture2D tex)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }
    }
}
