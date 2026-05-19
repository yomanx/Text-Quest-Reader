using System;
using TextQuestReader.Cinematic.Procedural;
using TextQuestReader.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// One quest card in the catalog list. Built fully from code — no prefab.
    /// </summary>
    public class CinematicQuestCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public CatalogEntry Entry { get; private set; }
        public bool IsSelected { get; private set; }

        private Image baseImage;
        private Image glow;
        private Image accentBar;
        private TextMeshProUGUI titleLabel;
        private TextMeshProUGUI metaLabel;
        private TextMeshProUGUI badgeLabel;
        private Image badgeBg;
        private float pulse;
        private Action<CatalogEntry> onClicked;

        public static CinematicQuestCard Create(RectTransform parent, CatalogEntry entry, Action<CatalogEntry> onClicked)
        {
            GameObject go = new GameObject("Card_" + entry.QuestShort.QuestName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);

            LayoutElement le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 96f;
            le.flexibleWidth = 1f;

            Image bg = go.GetComponent<Image>();
            bg.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(0.06f, 0.10f, 0.16f, 1f), 14, 64);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.06f, 0.10f, 0.16f, 0.85f);

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = bg;

            CinematicQuestCard card = go.AddComponent<CinematicQuestCard>();
            card.Entry = entry;
            card.onClicked = onClicked;
            card.baseImage = bg;
            card.Build();
            btn.onClick.AddListener(card.OnClicked);
            return card;
        }

        private void Build()
        {
            RectTransform rt = (RectTransform)transform;

            GameObject glowGo = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowGo.transform.SetParent(rt, false);
            RectTransform grt = (RectTransform)glowGo.transform;
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = new Vector2(-10f, -6f);
            grt.offsetMax = new Vector2(10f, 6f);
            grt.SetAsFirstSibling();
            glow = glowGo.GetComponent<Image>();
            glow.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(0.55f, 0.85f, 1f, 1f), 18, 64);
            glow.type = Image.Type.Sliced;
            glow.raycastTarget = false;
            glow.color = new Color(0.55f, 0.85f, 1f, 0f);

            GameObject accentGo = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accentGo.transform.SetParent(rt, false);
            RectTransform art = (RectTransform)accentGo.transform;
            art.anchorMin = new Vector2(0f, 0f);
            art.anchorMax = new Vector2(0f, 1f);
            art.pivot = new Vector2(0f, 0.5f);
            art.offsetMin = new Vector2(0f, 6f);
            art.offsetMax = new Vector2(0f, -6f);
            art.sizeDelta = new Vector2(5f, 0f);
            accentBar = accentGo.GetComponent<Image>();
            accentBar.color = ResolveAccent();
            accentBar.raycastTarget = false;

            GameObject titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(rt, false);
            titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
            titleLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            titleLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleLabel.rectTransform.pivot = new Vector2(0f, 1f);
            titleLabel.rectTransform.offsetMin = new Vector2(20f, -42f);
            titleLabel.rectTransform.offsetMax = new Vector2(-12f, -12f);
            string title = string.IsNullOrEmpty(Entry.QuestShort.DisplayName) ? Entry.QuestShort.QuestName : Entry.QuestShort.DisplayName;
            titleLabel.text = title.ToUpperInvariant();
            titleLabel.fontSize = 16f;
            titleLabel.fontStyle = FontStyles.Bold;
            titleLabel.color = new Color(0.95f, 0.95f, 1f, 1f);
            titleLabel.alignment = TextAlignmentOptions.TopLeft;
            titleLabel.characterSpacing = 3f;
            titleLabel.raycastTarget = false;

            GameObject metaGo = new GameObject("Meta", typeof(RectTransform));
            metaGo.transform.SetParent(rt, false);
            metaLabel = metaGo.AddComponent<TextMeshProUGUI>();
            metaLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            metaLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
            metaLabel.rectTransform.pivot = new Vector2(0f, 0f);
            metaLabel.rectTransform.offsetMin = new Vector2(20f, 10f);
            metaLabel.rectTransform.offsetMax = new Vector2(-12f, 30f);
            string author = string.IsNullOrEmpty(Entry.QuestShort.Author) ? "" : ("by " + Entry.QuestShort.Author);
            string lang = string.IsNullOrEmpty(Entry.QuestShort.Lang) ? "" : "[" + Entry.QuestShort.Lang.ToUpperInvariant() + "]";
            metaLabel.text = (author + "   " + lang).Trim();
            metaLabel.fontSize = 12f;
            metaLabel.color = new Color(0.55f, 0.80f, 0.95f, 0.75f);
            metaLabel.alignment = TextAlignmentOptions.BottomLeft;
            metaLabel.characterSpacing = 2f;
            metaLabel.raycastTarget = false;

            BuildBadge();
            ApplyAccessState();
        }

        private Color ResolveAccent()
        {
            switch (Entry.AccessState)
            {
                case QuestAccessState.LockedPremium: return new Color(0.95f, 0.55f, 0.20f, 1f);
                case QuestAccessState.OwnedPremium: return new Color(0.4f, 0.95f, 0.5f, 1f);
                default: return Entry.IsFeatured ? new Color(0.55f, 0.85f, 1f, 1f) : new Color(0.30f, 0.55f, 0.85f, 1f);
            }
        }

        private void BuildBadge()
        {
            GameObject badgeGo = new GameObject("Badge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            badgeGo.transform.SetParent(transform, false);
            RectTransform brt = (RectTransform)badgeGo.transform;
            brt.anchorMin = new Vector2(1f, 1f);
            brt.anchorMax = new Vector2(1f, 1f);
            brt.pivot = new Vector2(1f, 1f);
            brt.sizeDelta = new Vector2(96f, 24f);
            brt.anchoredPosition = new Vector2(-10f, -10f);
            badgeBg = badgeGo.GetComponent<Image>();
            badgeBg.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(Color.white, 10, 32);
            badgeBg.type = Image.Type.Sliced;
            badgeBg.color = new Color(0f, 0f, 0f, 0.55f);
            badgeBg.raycastTarget = false;

            GameObject lblGo = new GameObject("BadgeLabel", typeof(RectTransform));
            lblGo.transform.SetParent(badgeGo.transform, false);
            badgeLabel = lblGo.AddComponent<TextMeshProUGUI>();
            badgeLabel.rectTransform.anchorMin = Vector2.zero;
            badgeLabel.rectTransform.anchorMax = Vector2.one;
            badgeLabel.rectTransform.offsetMin = Vector2.zero;
            badgeLabel.rectTransform.offsetMax = Vector2.zero;
            badgeLabel.fontSize = 11f;
            badgeLabel.alignment = TextAlignmentOptions.Center;
            badgeLabel.fontStyle = FontStyles.Bold;
            badgeLabel.characterSpacing = 4f;
            badgeLabel.color = new Color(0.95f, 0.95f, 1f, 1f);
            badgeLabel.raycastTarget = false;
        }

        private void ApplyAccessState()
        {
            switch (Entry.AccessState)
            {
                case QuestAccessState.LockedPremium:
                    badgeLabel.text = "PREMIUM";
                    badgeLabel.color = new Color(0.95f, 0.85f, 0.40f, 1f);
                    break;
                case QuestAccessState.OwnedPremium:
                    badgeLabel.text = "OWNED";
                    badgeLabel.color = new Color(0.4f, 0.95f, 0.5f, 1f);
                    break;
                default:
                    if (Entry.IsFeatured)
                    {
                        badgeLabel.text = "FEATURED";
                        badgeLabel.color = new Color(0.55f, 0.85f, 1f, 1f);
                    }
                    else
                    {
                        badgeLabel.text = "FREE";
                        badgeLabel.color = new Color(0.85f, 0.95f, 0.85f, 0.95f);
                    }
                    break;
            }
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (baseImage != null)
                baseImage.color = selected
                    ? new Color(0.12f, 0.18f, 0.26f, 0.95f)
                    : new Color(0.06f, 0.10f, 0.16f, 0.85f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (glow != null)
                glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, 0.45f);
            transform.localScale = Vector3.one * 1.015f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (glow == null) return;
            pulse += Time.deltaTime * 1.6f;
            float baseAlpha = IsSelected ? 0.45f : 0.10f;
            float wave = baseAlpha + 0.10f * Mathf.Sin(pulse);
            Color c = ResolveAccent();
            glow.color = new Color(c.r, c.g, c.b, wave);
        }

        private void OnClicked()
        {
            onClicked?.Invoke(Entry);
        }
    }
}
