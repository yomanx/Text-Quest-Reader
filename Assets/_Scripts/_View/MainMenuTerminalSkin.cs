using TextQuestReader.Cinematic.Procedural;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.View
{
    /// <summary>
    /// Code-built sci-fi terminal frame that sits BEHIND every existing UI
    /// element on the main canvas. Adds animated starfield, faint grid,
    /// corner brackets, header label, and dim scanlines. No prefab edits.
    /// </summary>
    public class MainMenuTerminalSkin : MonoBehaviour
    {
        public static MainMenuTerminalSkin Attach(RectTransform canvas)
        {
            if (canvas == null) return null;
            MainMenuTerminalSkin existing = canvas.GetComponentInChildren<MainMenuTerminalSkin>(true);
            if (existing != null) return existing;

            GameObject root = new GameObject("MainMenuTerminalSkin", typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            RectTransform rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.SetAsFirstSibling();

            MainMenuTerminalSkin skin = root.AddComponent<MainMenuTerminalSkin>();
            skin.Build();
            return skin;
        }

        private TextMeshProUGUI headerLabel;
        private float headerPhase;

        private void Build()
        {
            RectTransform host = (RectTransform)transform;

            AddSubtleGradient(host);
            AddSubtleGrid(host);
            AddCornerBrackets(host);
            AddHeaderLabel(host);
            AddFooterLabel(host);
            AddOuterScanlines(host);
        }

        private void AddSubtleGradient(RectTransform parent)
        {
            GameObject go = new GameObject("BackdropGradient", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.sprite = ProceduralTextureFactory.CreateVerticalGradientSprite(new Color(0.02f, 0.04f, 0.08f, 0.40f), new Color(0.01f, 0.02f, 0.05f, 0.55f), 128);
            img.type = Image.Type.Simple;
        }

        private void AddSubtleGrid(RectTransform parent)
        {
            GameObject go = new GameObject("BackdropGrid", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            GridLayer layer = go.AddComponent<GridLayer>();
            layer.Apply(new Color(0.30f, 0.65f, 0.95f, 0.07f), 60);
        }

        private void AddCornerBrackets(RectTransform parent)
        {
            Color color = new Color(0.30f, 0.85f, 1f, 0.65f);
            Sprite shared = ProceduralTextureFactory.CreateCornerBracketSprite(color, 3, 32, 48);

            AddBracket(parent, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, 0f, shared);
            AddBracket(parent, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, 90f, shared);
            AddBracket(parent, new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, 180f, shared);
            AddBracket(parent, new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, 270f, shared);
        }

        private void AddBracket(RectTransform parent, Vector2 anchor, Vector2 pivot, Vector2 offset, float rotation, Sprite sprite)
        {
            GameObject go = new GameObject("CornerBracket", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = new Vector2(48f, 48f);
            rt.anchoredPosition = new Vector2(20f * (anchor.x < 0.5f ? 1f : -1f) + offset.x, 20f * (anchor.y < 0.5f ? 1f : -1f) + offset.y);
            rt.localEulerAngles = new Vector3(0f, 0f, rotation);
            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.sprite = sprite;
            img.color = Color.white;
        }

        private void AddHeaderLabel(RectTransform parent)
        {
            GameObject go = new GameObject("HeaderLabel", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            headerLabel = go.AddComponent<TextMeshProUGUI>();
            RectTransform rt = headerLabel.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(80f, -38f);
            rt.offsetMax = new Vector2(-80f, -8f);
            headerLabel.text = "[ TEXT-QUEST READER ] · TERMINAL ACCESS · LOCAL";
            headerLabel.fontSize = 13f;
            headerLabel.alignment = TextAlignmentOptions.Center;
            headerLabel.color = new Color(0.45f, 0.85f, 1f, 0.70f);
            headerLabel.characterSpacing = 6f;
            headerLabel.fontStyle = FontStyles.Bold;
            headerLabel.raycastTarget = false;
        }

        private void AddFooterLabel(RectTransform parent)
        {
            GameObject go = new GameObject("FooterLabel", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(80f, 8f);
            rt.offsetMax = new Vector2(-80f, 32f);
            tmp.text = "// SIGNAL OK · LINK STABLE · " + System.DateTime.Now.Year + " //";
            tmp.fontSize = 11f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.40f, 0.75f, 0.95f, 0.55f);
            tmp.characterSpacing = 4f;
            tmp.raycastTarget = false;
        }

        private void AddOuterScanlines(RectTransform parent)
        {
            GameObject go = new GameObject("OuterScanlines", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            ScanlinesLayer layer = go.AddComponent<ScanlinesLayer>();
            layer.Apply(0.05f);
        }

        private void Update()
        {
            if (headerLabel == null) return;
            headerPhase += Time.deltaTime * 1.8f;
            float a = 0.55f + 0.20f * Mathf.Sin(headerPhase);
            Color c = headerLabel.color;
            headerLabel.color = new Color(c.r, c.g, c.b, a);
        }
    }
}
