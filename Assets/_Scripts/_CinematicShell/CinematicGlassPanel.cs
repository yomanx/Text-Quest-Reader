using TextQuestReader.Cinematic.Procedural;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Builds a glassy sci-fi panel — semi-transparent rounded-rect base,
    /// inner gradient, glowing outline, optional scanline veil, optional
    /// pulsing accent. Used for the text panel, choice cards, hero block,
    /// hud blocks. All procedural — no prefab.
    /// </summary>
    public static class CinematicGlassPanel
    {
        public struct GlassConfig
        {
            public Color BaseColor;
            public Color OutlineColor;
            public Color InnerGlowColor;
            public int CornerRadius;
            public bool Scanlines;
            public float ScanlineOpacity;
            public bool BottomGradient;

            public static GlassConfig Default => new GlassConfig
            {
                BaseColor = new Color(0.05f, 0.07f, 0.12f, 0.78f),
                OutlineColor = new Color(0.55f, 0.85f, 1f, 0.85f),
                InnerGlowColor = new Color(0.20f, 0.55f, 0.85f, 0.18f),
                CornerRadius = 18,
                Scanlines = true,
                ScanlineOpacity = 0.05f,
                BottomGradient = true
            };
        }

        public static GameObject Build(string name, RectTransform parent, GlassConfig config)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            int radius = Mathf.Max(4, config.CornerRadius);
            Sprite roundSprite = ProceduralTextureFactory.CreateRoundedRectSprite(Color.white, radius, radius * 3);

            GameObject baseGo = new GameObject("Base", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            baseGo.transform.SetParent(rt, false);
            RectTransform brt = (RectTransform)baseGo.transform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            Image baseImg = baseGo.GetComponent<Image>();
            baseImg.sprite = roundSprite;
            baseImg.type = Image.Type.Sliced;
            baseImg.color = config.BaseColor;
            baseImg.raycastTarget = true;

            if (config.BottomGradient)
            {
                GameObject grad = new GameObject("InnerGradient", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                grad.transform.SetParent(rt, false);
                RectTransform grt = (RectTransform)grad.transform;
                grt.anchorMin = Vector2.zero;
                grt.anchorMax = Vector2.one;
                grt.offsetMin = new Vector2(2f, 2f);
                grt.offsetMax = new Vector2(-2f, -2f);
                Image gimg = grad.GetComponent<Image>();
                gimg.raycastTarget = false;
                gimg.sprite = ProceduralTextureFactory.CreateVerticalGradientSprite(new Color(config.InnerGlowColor.r, config.InnerGlowColor.g, config.InnerGlowColor.b, 0f), config.InnerGlowColor, 128);
                gimg.color = Color.white;
            }

            if (config.Scanlines && config.ScanlineOpacity > 0f)
            {
                GameObject scan = new GameObject("Scanlines", typeof(RectTransform));
                scan.transform.SetParent(rt, false);
                RectTransform srt = (RectTransform)scan.transform;
                srt.anchorMin = Vector2.zero;
                srt.anchorMax = Vector2.one;
                srt.offsetMin = new Vector2(2f, 2f);
                srt.offsetMax = new Vector2(-2f, -2f);
                ScanlinesLayer scanLayer = scan.AddComponent<ScanlinesLayer>();
                scanLayer.Apply(config.ScanlineOpacity);
            }

            GameObject outline = new GameObject("Outline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            outline.transform.SetParent(rt, false);
            RectTransform ort = (RectTransform)outline.transform;
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.offsetMin = new Vector2(-1f, -1f);
            ort.offsetMax = new Vector2(1f, 1f);
            Image oimg = outline.GetComponent<Image>();
            oimg.sprite = ProceduralTextureFactory.CreateRoundedRectBorderSprite(Color.white, 2, radius, radius * 4);
            oimg.type = Image.Type.Sliced;
            oimg.color = config.OutlineColor;
            oimg.raycastTarget = false;

            GameObject contentHost = new GameObject("Content", typeof(RectTransform));
            contentHost.transform.SetParent(rt, false);
            RectTransform crt = (RectTransform)contentHost.transform;
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(18f, 18f);
            crt.offsetMax = new Vector2(-18f, -18f);

            return contentHost;
        }
    }
}
