using System.Collections.Generic;
using TextQuestReader.Cinematic.Procedural;
using TextQuestReader.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Catalog screen — fullscreen sci-fi terminal launcher. Layout:
    ///   ┌─────────────────────────────────────────┐
    ///   │  HEADER · TEXT-QUEST READER             │
    ///   │                                          │
    ///   │  ┌────────────┐  ┌──── HERO ────────┐   │
    ///   │  │  CARD A    │  │  Featured quest  │   │
    ///   │  │            │  │  Big preview     │   │
    ///   │  │  CARD B    │  │  Description     │   │
    ///   │  │            │  │  [ START ]       │   │
    ///   │  │  CARD C    │  │                  │   │
    ///   │  └────────────┘  └──────────────────┘   │
    ///   │  Local · Remote · Settings              │
    ///   └─────────────────────────────────────────┘
    /// </summary>
    public class CinematicCatalogScreen : MonoBehaviour
    {
        private GamePanel gamePanel;
        private CinematicBackgroundLayer background;
        private RectTransform self;
        private CanvasGroup canvasGroup;

        private RectTransform cardListContent;
        private RectTransform heroPanelContent;
        private TextMeshProUGUI heroTitle;
        private TextMeshProUGUI heroSubtitle;
        private TextMeshProUGUI heroDescription;
        private TextMeshProUGUI heroPrice;
        private TextMeshProUGUI heroAccess;
        private Image heroPreviewImage;
        private Button heroStartButton;
        private TextMeshProUGUI heroStartLabel;
        private Image heroStartGlow;

        private readonly List<CinematicQuestCard> cards = new List<CinematicQuestCard>();
        private QuestShort currentSelected;
        private QuestAccessState currentSelectedAccess;
        private ProductDefinition currentSelectedProduct;
        private GamePanel.Source currentSource = GamePanel.Source.Local;
        private float heroStartPulse;

        public static CinematicCatalogScreen Create(RectTransform shellRoot, GamePanel gamePanel, CinematicBackgroundLayer background)
        {
            GameObject go = new GameObject("CinematicCatalog", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(shellRoot, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            CinematicCatalogScreen screen = go.AddComponent<CinematicCatalogScreen>();
            screen.gamePanel = gamePanel;
            screen.background = background;
            screen.self = rt;
            screen.canvasGroup = go.GetComponent<CanvasGroup>();
            screen.Build();
            return screen;
        }

        private void Build()
        {
            BuildHeader();
            BuildCardList();
            BuildHeroPanel();
            BuildBottomBar();
        }

        public void Hide(bool instant = false)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Show(List<CatalogEntry> entries, GamePanel.Source source, string preselectName)
        {
            currentSource = source;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            foreach (CinematicQuestCard card in cards)
                if (card != null) Destroy(card.gameObject);
            cards.Clear();

            CatalogEntry pre = null;
            for (int i = 0; i < entries.Count; i++)
            {
                CatalogEntry entry = entries[i];
                CinematicQuestCard card = CinematicQuestCard.Create(cardListContent, entry, OnCardClicked);
                cards.Add(card);
                if (pre == null && (!string.IsNullOrEmpty(preselectName) ? entry.QuestShort.QuestName == preselectName : i == 0))
                    pre = entry;
            }

            if (pre != null)
                ApplyHero(pre);
        }

        public void UpdateSelection(QuestShort quest)
        {
            if (quest == null) return;
            for (int i = 0; i < cards.Count; i++)
                cards[i].SetSelected(cards[i].Entry.QuestShort.QuestName == quest.QuestName);

            CatalogEntry match = FindEntry(quest.QuestName);
            if (match != null)
                ApplyHero(match);
        }

        private CatalogEntry FindEntry(string questName)
        {
            for (int i = 0; i < cards.Count; i++)
                if (cards[i].Entry.QuestShort.QuestName == questName)
                    return cards[i].Entry;
            return null;
        }

        private void OnCardClicked(CatalogEntry entry)
        {
            if (entry == null || gamePanel == null) return;
            gamePanel.SelectQuest(entry.QuestShort);
        }

        private void BuildHeader()
        {
            GameObject hdr = new GameObject("Header", typeof(RectTransform));
            hdr.transform.SetParent(self, false);
            RectTransform rt = (RectTransform)hdr.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(40f, -90f);
            rt.offsetMax = new Vector2(-40f, -20f);

            GameObject titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(rt, false);
            TextMeshProUGUI title = titleGo.AddComponent<TextMeshProUGUI>();
            title.rectTransform.anchorMin = new Vector2(0f, 0f);
            title.rectTransform.anchorMax = new Vector2(0.6f, 1f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.text = "TEXT-QUEST READER · LIBRARY";
            title.fontSize = 24f;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Left;
            title.color = new Color(0.85f, 0.95f, 1f, 1f);
            title.characterSpacing = 6f;
            title.raycastTarget = false;

            GameObject subGo = new GameObject("Subtitle", typeof(RectTransform));
            subGo.transform.SetParent(rt, false);
            TextMeshProUGUI sub = subGo.AddComponent<TextMeshProUGUI>();
            sub.rectTransform.anchorMin = new Vector2(0f, 0f);
            sub.rectTransform.anchorMax = new Vector2(0.6f, 1f);
            sub.rectTransform.offsetMin = new Vector2(0f, -34f);
            sub.rectTransform.offsetMax = new Vector2(0f, -34f);
            sub.rectTransform.pivot = new Vector2(0f, 0.5f);
            sub.text = "// LINK STABLE · SIGNAL OK · " + System.DateTime.Now.Year + " //";
            sub.fontSize = 13f;
            sub.alignment = TextAlignmentOptions.MidlineLeft;
            sub.color = new Color(0.5f, 0.85f, 1f, 0.65f);
            sub.characterSpacing = 4f;
            sub.raycastTarget = false;

            GameObject statusGo = new GameObject("Status", typeof(RectTransform));
            statusGo.transform.SetParent(rt, false);
            TextMeshProUGUI status = statusGo.AddComponent<TextMeshProUGUI>();
            status.rectTransform.anchorMin = new Vector2(0.6f, 0f);
            status.rectTransform.anchorMax = new Vector2(1f, 1f);
            status.rectTransform.offsetMin = Vector2.zero;
            status.rectTransform.offsetMax = Vector2.zero;
            status.text = "OPERATOR · YOU\nSTATUS · NOMINAL";
            status.fontSize = 13f;
            status.alignment = TextAlignmentOptions.Right;
            status.color = new Color(0.55f, 0.85f, 1f, 0.75f);
            status.characterSpacing = 3f;
            status.raycastTarget = false;
        }

        private void BuildCardList()
        {
            GameObject panel = new GameObject("CardListPanel", typeof(RectTransform));
            panel.transform.SetParent(self, false);
            RectTransform prt = (RectTransform)panel.transform;
            prt.anchorMin = new Vector2(0f, 0f);
            prt.anchorMax = new Vector2(0.35f, 1f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.offsetMin = new Vector2(40f, 100f);
            prt.offsetMax = new Vector2(0f, -110f);

            GameObject host = CinematicGlassPanel.Build("ListHost", prt, CinematicGlassPanel.GlassConfig.Default);
            RectTransform contentArea = (RectTransform)host.transform;

            GameObject scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(CanvasRenderer), typeof(Image));
            scroll.transform.SetParent(contentArea, false);
            RectTransform srt = (RectTransform)scroll.transform;
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            Image scrollBg = scroll.GetComponent<Image>();
            scrollBg.color = new Color(0f, 0f, 0f, 0.001f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            RectTransform vrt = (RectTransform)viewport.transform;
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero;
            vrt.offsetMax = Vector2.zero;
            Image vimg = viewport.GetComponent<Image>();
            vimg.color = new Color(0f, 0f, 0f, 0.001f);
            vimg.raycastTarget = true;
            Mask mask = viewport.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform crt = (RectTransform)content.transform;
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = scroll.GetComponent<ScrollRect>();
            sr.viewport = vrt;
            sr.content = crt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 22f;

            cardListContent = crt;
        }

        private void BuildHeroPanel()
        {
            GameObject panel = new GameObject("HeroPanel", typeof(RectTransform));
            panel.transform.SetParent(self, false);
            RectTransform prt = (RectTransform)panel.transform;
            prt.anchorMin = new Vector2(0.35f, 0f);
            prt.anchorMax = new Vector2(1f, 1f);
            prt.offsetMin = new Vector2(20f, 100f);
            prt.offsetMax = new Vector2(-40f, -110f);

            CinematicGlassPanel.GlassConfig config = CinematicGlassPanel.GlassConfig.Default;
            config.BaseColor = new Color(0.04f, 0.06f, 0.10f, 0.82f);
            config.OutlineColor = new Color(0.95f, 0.55f, 0.20f, 0.65f);
            config.InnerGlowColor = new Color(0.95f, 0.55f, 0.20f, 0.10f);
            GameObject host = CinematicGlassPanel.Build("HeroHost", prt, config);
            heroPanelContent = (RectTransform)host.transform;

            BuildHeroPreview();
            BuildHeroTextBlock();
            BuildHeroStartButton();
        }

        private void BuildHeroPreview()
        {
            GameObject preview = new GameObject("Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            preview.transform.SetParent(heroPanelContent, false);
            RectTransform rt = (RectTransform)preview.transform;
            rt.anchorMin = new Vector2(0f, 0.32f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            heroPreviewImage = preview.GetComponent<Image>();
            heroPreviewImage.raycastTarget = false;
            heroPreviewImage.color = Color.white;
            heroPreviewImage.preserveAspect = false;

            GameObject scanGo = new GameObject("PreviewScanlines", typeof(RectTransform));
            scanGo.transform.SetParent(rt, false);
            RectTransform sgt = (RectTransform)scanGo.transform;
            sgt.anchorMin = Vector2.zero;
            sgt.anchorMax = Vector2.one;
            sgt.offsetMin = Vector2.zero;
            sgt.offsetMax = Vector2.zero;
            ScanlinesLayer scan = scanGo.AddComponent<ScanlinesLayer>();
            scan.Apply(0.10f);

            GameObject corners = new GameObject("CornerBrackets", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            corners.transform.SetParent(rt, false);
            RectTransform crt = (RectTransform)corners.transform;
            crt.anchorMin = new Vector2(0f, 0f);
            crt.anchorMax = new Vector2(0f, 0f);
            crt.pivot = new Vector2(0f, 0f);
            crt.sizeDelta = new Vector2(36f, 36f);
            crt.anchoredPosition = new Vector2(6f, 6f);
            Image cimg = corners.GetComponent<Image>();
            cimg.raycastTarget = false;
            cimg.sprite = ProceduralTextureFactory.CreateCornerBracketSprite(new Color(0.95f, 0.55f, 0.20f, 1f), 3, 24, 36);
            cimg.color = Color.white;
        }

        private void BuildHeroTextBlock()
        {
            GameObject host = new GameObject("HeroText", typeof(RectTransform));
            host.transform.SetParent(heroPanelContent, false);
            RectTransform hrt = (RectTransform)host.transform;
            hrt.anchorMin = new Vector2(0f, 0f);
            hrt.anchorMax = new Vector2(1f, 0.32f);
            hrt.offsetMin = new Vector2(0f, 70f);
            hrt.offsetMax = new Vector2(0f, -8f);

            GameObject titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(hrt, false);
            heroTitle = titleGo.AddComponent<TextMeshProUGUI>();
            heroTitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            heroTitle.rectTransform.anchorMax = new Vector2(0.7f, 1f);
            heroTitle.rectTransform.pivot = new Vector2(0f, 1f);
            heroTitle.rectTransform.offsetMin = Vector2.zero;
            heroTitle.rectTransform.offsetMax = Vector2.zero;
            heroTitle.rectTransform.sizeDelta = new Vector2(0f, 44f);
            heroTitle.text = "—";
            heroTitle.fontSize = 32f;
            heroTitle.fontStyle = FontStyles.Bold;
            heroTitle.color = new Color(0.95f, 0.85f, 0.40f, 1f);
            heroTitle.alignment = TextAlignmentOptions.TopLeft;
            heroTitle.characterSpacing = 4f;
            heroTitle.enableAutoSizing = false;
            heroTitle.raycastTarget = false;

            GameObject subGo = new GameObject("Subtitle", typeof(RectTransform));
            subGo.transform.SetParent(hrt, false);
            heroSubtitle = subGo.AddComponent<TextMeshProUGUI>();
            heroSubtitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            heroSubtitle.rectTransform.anchorMax = new Vector2(0.7f, 1f);
            heroSubtitle.rectTransform.pivot = new Vector2(0f, 1f);
            heroSubtitle.rectTransform.offsetMin = new Vector2(0f, -52f);
            heroSubtitle.rectTransform.offsetMax = new Vector2(0f, -52f);
            heroSubtitle.rectTransform.sizeDelta = new Vector2(0f, 22f);
            heroSubtitle.text = "by — · [EN]";
            heroSubtitle.fontSize = 14f;
            heroSubtitle.color = new Color(0.55f, 0.85f, 1f, 0.75f);
            heroSubtitle.alignment = TextAlignmentOptions.TopLeft;
            heroSubtitle.characterSpacing = 4f;
            heroSubtitle.raycastTarget = false;

            GameObject descGo = new GameObject("Description", typeof(RectTransform));
            descGo.transform.SetParent(hrt, false);
            heroDescription = descGo.AddComponent<TextMeshProUGUI>();
            heroDescription.rectTransform.anchorMin = new Vector2(0f, 0f);
            heroDescription.rectTransform.anchorMax = new Vector2(1f, 1f);
            heroDescription.rectTransform.offsetMin = new Vector2(0f, 0f);
            heroDescription.rectTransform.offsetMax = new Vector2(-180f, -82f);
            heroDescription.text = "";
            heroDescription.fontSize = 16f;
            heroDescription.color = new Color(0.85f, 0.92f, 1f, 0.95f);
            heroDescription.alignment = TextAlignmentOptions.TopLeft;
#pragma warning disable CS0618
            heroDescription.enableWordWrapping = true;
#pragma warning restore CS0618
            heroDescription.raycastTarget = false;

            GameObject priceGo = new GameObject("Price", typeof(RectTransform));
            priceGo.transform.SetParent(hrt, false);
            heroPrice = priceGo.AddComponent<TextMeshProUGUI>();
            heroPrice.rectTransform.anchorMin = new Vector2(1f, 1f);
            heroPrice.rectTransform.anchorMax = new Vector2(1f, 1f);
            heroPrice.rectTransform.pivot = new Vector2(1f, 1f);
            heroPrice.rectTransform.offsetMin = new Vector2(-200f, -34f);
            heroPrice.rectTransform.offsetMax = new Vector2(0f, 0f);
            heroPrice.text = "";
            heroPrice.fontSize = 22f;
            heroPrice.fontStyle = FontStyles.Bold;
            heroPrice.color = new Color(0.95f, 0.55f, 0.20f, 1f);
            heroPrice.alignment = TextAlignmentOptions.TopRight;
            heroPrice.raycastTarget = false;

            GameObject accessGo = new GameObject("Access", typeof(RectTransform));
            accessGo.transform.SetParent(hrt, false);
            heroAccess = accessGo.AddComponent<TextMeshProUGUI>();
            heroAccess.rectTransform.anchorMin = new Vector2(1f, 1f);
            heroAccess.rectTransform.anchorMax = new Vector2(1f, 1f);
            heroAccess.rectTransform.pivot = new Vector2(1f, 1f);
            heroAccess.rectTransform.offsetMin = new Vector2(-200f, -66f);
            heroAccess.rectTransform.offsetMax = new Vector2(0f, -36f);
            heroAccess.text = "";
            heroAccess.fontSize = 13f;
            heroAccess.fontStyle = FontStyles.Bold;
            heroAccess.color = new Color(0.55f, 0.85f, 1f, 1f);
            heroAccess.alignment = TextAlignmentOptions.TopRight;
            heroAccess.characterSpacing = 4f;
            heroAccess.raycastTarget = false;
        }

        private void BuildHeroStartButton()
        {
            GameObject btnGo = new GameObject("StartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(heroPanelContent, false);
            RectTransform brt = (RectTransform)btnGo.transform;
            brt.anchorMin = new Vector2(0.5f, 0f);
            brt.anchorMax = new Vector2(0.5f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.sizeDelta = new Vector2(320f, 64f);
            brt.anchoredPosition = new Vector2(0f, 0f);

            Image bg = btnGo.GetComponent<Image>();
            bg.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(0.95f, 0.55f, 0.20f, 1f), 22, 96);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.95f, 0.55f, 0.20f, 1f);

            Button btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(OnStartClicked);
            heroStartButton = btn;

            GameObject glowGo = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowGo.transform.SetParent(brt, false);
            RectTransform grt = (RectTransform)glowGo.transform;
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = new Vector2(-26f, -16f);
            grt.offsetMax = new Vector2(26f, 16f);
            heroStartGlow = glowGo.GetComponent<Image>();
            heroStartGlow.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(1f, 0.65f, 0.30f, 1f), 30, 96);
            heroStartGlow.type = Image.Type.Sliced;
            heroStartGlow.color = new Color(1f, 0.65f, 0.30f, 0.30f);
            heroStartGlow.raycastTarget = false;
            grt.SetAsFirstSibling();

            GameObject lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(btnGo.transform, false);
            heroStartLabel = lblGo.AddComponent<TextMeshProUGUI>();
            heroStartLabel.rectTransform.anchorMin = Vector2.zero;
            heroStartLabel.rectTransform.anchorMax = Vector2.one;
            heroStartLabel.rectTransform.offsetMin = Vector2.zero;
            heroStartLabel.rectTransform.offsetMax = Vector2.zero;
            heroStartLabel.text = "▶  ENTER QUEST";
            heroStartLabel.fontSize = 22f;
            heroStartLabel.fontStyle = FontStyles.Bold;
            heroStartLabel.color = Color.white;
            heroStartLabel.alignment = TextAlignmentOptions.Center;
            heroStartLabel.characterSpacing = 8f;
            heroStartLabel.raycastTarget = false;
        }

        private void BuildBottomBar()
        {
            GameObject bar = new GameObject("BottomBar", typeof(RectTransform));
            bar.transform.SetParent(self, false);
            RectTransform brt = (RectTransform)bar.transform;
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.offsetMin = new Vector2(40f, 20f);
            brt.offsetMax = new Vector2(-40f, 80f);

            GameObject txtGo = new GameObject("HintLabel", typeof(RectTransform));
            txtGo.transform.SetParent(brt, false);
            TextMeshProUGUI txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = Vector2.zero;
            txt.rectTransform.offsetMax = Vector2.zero;
            txt.text = "← →  CHANGE SOURCE     ↑ ↓  SELECT QUEST     ENTER  START     ESC  EXIT QUEST";
            txt.fontSize = 13f;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = new Color(0.45f, 0.75f, 0.95f, 0.55f);
            txt.characterSpacing = 4f;
            txt.raycastTarget = false;
        }

        private void ApplyHero(CatalogEntry entry)
        {
            if (entry == null) return;
            currentSelected = entry.QuestShort;
            currentSelectedAccess = entry.AccessState;
            currentSelectedProduct = entry.Product;

            QuestShort q = entry.QuestShort;
            string title = string.IsNullOrEmpty(q.DisplayName) ? q.QuestName : q.DisplayName;
            heroTitle.text = title.ToUpperInvariant();

            string subline = "";
            if (!string.IsNullOrEmpty(q.Author)) subline = "BY " + q.Author.ToUpperInvariant();
            if (!string.IsNullOrEmpty(q.Lang)) subline += (subline.Length == 0 ? "" : "  ·  ") + "[" + q.Lang.ToUpperInvariant() + "]";
            subline += "  ·  ORDER " + q.Order.ToString();
            heroSubtitle.text = subline;

            heroDescription.text = q.Description ?? "";

            if (heroPreviewImage != null)
            {
                if (heroPreviewImage.sprite != null && heroPreviewImage.sprite.texture != null)
                {
                    Texture2D old = heroPreviewImage.sprite.texture;
                    Destroy(heroPreviewImage.sprite);
                    if (old != null) Destroy(old);
                }
                heroPreviewImage.sprite = CinematicHeroArt.BuildPreviewFor(q, 768, 432);
            }

            switch (entry.AccessState)
            {
                case QuestAccessState.FreeOpen:
                    heroAccess.text = entry.IsFeatured ? "★ FEATURED" : "FREE";
                    heroAccess.color = entry.IsFeatured ? new Color(0.55f, 0.85f, 1f, 1f) : new Color(0.85f, 0.95f, 0.85f, 0.85f);
                    heroPrice.text = "";
                    heroStartLabel.text = "▶  ENTER QUEST";
                    break;
                case QuestAccessState.OwnedPremium:
                    heroAccess.text = "✓ OWNED";
                    heroAccess.color = new Color(0.4f, 0.95f, 0.5f, 1f);
                    heroPrice.text = "";
                    heroStartLabel.text = "▶  ENTER QUEST";
                    break;
                case QuestAccessState.LockedPremium:
                    heroAccess.text = "★ PREMIUM";
                    heroAccess.color = new Color(0.95f, 0.85f, 0.40f, 1f);
                    heroPrice.text = entry.Product != null ? entry.Product.price : "";
                    heroStartLabel.text = "★  UNLOCK · " + (entry.Product != null ? entry.Product.price : "");
                    break;
            }
        }

        private void OnStartClicked()
        {
            if (currentSelected == null || gamePanel == null) return;
            gamePanel.StartQuest();
        }

        private void Update()
        {
            if (heroStartGlow == null || heroStartButton == null) return;
            heroStartPulse += Time.deltaTime * 2.4f;
            float wave = 0.45f + 0.20f * Mathf.Sin(heroStartPulse);
            Color c = heroStartGlow.color;
            heroStartGlow.color = new Color(c.r, c.g, c.b, wave);
        }
    }
}
