using System.Collections;
using System.Collections.Generic;
using TextQuestReader.Cinematic;
using TextQuestReader.Cinematic.Procedural;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Fullscreen cinematic reading mode. Layout:
    ///   ┌─────────────────────────────────────────────────────┐
    ///   │ ┌── HUD ────────────────────────────────────────┐  │
    ///   │ │ OXY  SAN  PWR  TRU  SGN                       │  │
    ///   │ └────────────────────────────────────────────────┘  │
    ///   │                                                     │
    ///   │ ┌────────────────────────────────┐  ┌──── HOLO ──┐ │
    ///   │ │                                │  │            │ │
    ///   │ │   GLASS TEXT PANEL             │  │  PREVIEW   │ │
    ///   │ │   (typewriter)                 │  │  MONITOR   │ │
    ///   │ │                                │  │            │ │
    ///   │ └────────────────────────────────┘  └────────────┘ │
    ///   │                                                     │
    ///   │ ┌── CHOICES ──────────────────────────────────────┐│
    ///   │ │ [  Card 1  ]  [  Card 2  ]  [  Card 3  ]       ││
    ///   │ └─────────────────────────────────────────────────┘│
    ///   └─────────────────────────────────────────────────────┘
    /// </summary>
    public class CinematicReaderScreen : MonoBehaviour
    {
        private GamePanel gamePanel;
        private CinematicBackgroundLayer background;
        private RectTransform self;
        private CanvasGroup canvasGroup;

        private CinematicHud hud;
        private TextMeshProUGUI mainText;
        private RectTransform choicesContent;
        private Image holoMonitor;
        private TextMeshProUGUI holoLabel;
        private TextMeshProUGUI locationLabel;
        private Button exitButton;

        private bool isTyping;
        private string currentValue = string.Empty;
        private Coroutine typeRoutine;
        private float defaultCharsPerSecond = 220f;

        public static CinematicReaderScreen Create(RectTransform shellRoot, GamePanel gamePanel, CinematicBackgroundLayer background)
        {
            GameObject go = new GameObject("CinematicReader", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(shellRoot, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            CinematicReaderScreen screen = go.AddComponent<CinematicReaderScreen>();
            screen.gamePanel = gamePanel;
            screen.background = background;
            screen.self = rt;
            screen.canvasGroup = go.GetComponent<CanvasGroup>();
            screen.Build();
            return screen;
        }

        public void Show(bool instant = false)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            if (background != null) background.SetVeilStrength(0.40f);
        }

        public void Hide(bool instant = false)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            if (background != null) background.SetVeilStrength(0.25f);
        }

        public void ApplyLocation(Location location)
        {
            if (location == null) return;
            if (locationLabel != null)
                locationLabel.text = "// LOCATION " + location.id.ToString("D2") + " //";
            if (hud != null && gamePanel != null && gamePanel.HasActivePlayer && gamePanel.Player != null && gamePanel.Player.quest != null)
                hud.Refresh(gamePanel.Player.quest);
        }

        public void SetMainText(string text)
        {
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            currentValue = text ?? string.Empty;
            isTyping = true;
            typeRoutine = StartCoroutine(TypewriterRoutine(currentValue));
            if (hud != null && gamePanel != null && gamePanel.HasActivePlayer && gamePanel.Player != null && gamePanel.Player.quest != null)
                hud.Refresh(gamePanel.Player.quest);
        }

        public void SetChoices(List<PassageInfo> choices)
        {
            ClearChoices();
            if (choices == null) return;
            for (int i = 0; i < choices.Count; i++)
            {
                PassageInfo info = choices[i];
                bool disabled = !info.isAllConditions && info.pass.alwaysShow;
                CinematicChoiceCard.Create(choicesContent, info.pass, disabled, OnChoiceClicked);
            }
        }

        public void SetSingleNext(Passage next)
        {
            ClearChoices();
            if (next == null) return;
            CinematicChoiceCard.CreateNext(choicesContent, next, OnNextClicked);
        }

        public void PrepareForEnding(bool victory)
        {
            ClearChoices();
            if (background != null)
                background.ShowPreset(victory ? "victory_scene" : "failure_scene");
        }

        private void OnChoiceClicked(Passage pass)
        {
            if (gamePanel == null) return;
            if (mainText != null && isTyping)
            {
                FinishImmediately();
                return;
            }
            gamePanel.TriggerPassageById(pass.id);
        }

        private void OnNextClicked()
        {
            if (gamePanel == null) return;
            if (mainText != null && isTyping)
            {
                FinishImmediately();
                return;
            }
            gamePanel.TriggerNextSinglePassage();
        }

        private void Update()
        {
            if (canvasGroup == null || canvasGroup.alpha < 0.99f) return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping) FinishImmediately();
            }
            if (Input.GetMouseButtonDown(0) && isTyping)
            {
                FinishImmediately();
            }
        }

        private void FinishImmediately()
        {
            if (!isTyping || mainText == null) return;
            mainText.text = currentValue;
            mainText.maxVisibleCharacters = currentValue.Length;
            isTyping = false;
        }

        private IEnumerator TypewriterRoutine(string value)
        {
            mainText.text = value;
            mainText.maxVisibleCharacters = 0;
            float timer = 0f;
            int idx = 0;
            float speed = defaultCharsPerSecond;
            while (idx < value.Length)
            {
                if (!isTyping) break;
                timer += Time.deltaTime;
                int chars = Mathf.FloorToInt(timer * speed);
                if (chars > 0)
                {
                    timer -= chars / speed;
                    idx = Mathf.Min(value.Length, idx + chars);
                    mainText.maxVisibleCharacters = idx;
                }
                yield return null;
            }
            mainText.maxVisibleCharacters = value.Length;
            isTyping = false;
            typeRoutine = null;
        }

        private void ClearChoices()
        {
            if (choicesContent == null) return;
            for (int i = choicesContent.childCount - 1; i >= 0; i--)
                Destroy(choicesContent.GetChild(i).gameObject);
        }

        private void Build()
        {
            BuildHud();
            BuildTextPanel();
            BuildHoloMonitor();
            BuildChoicesPanel();
            BuildExitButton();
        }

        private void BuildHud()
        {
            GameObject hudHost = new GameObject("HudHost", typeof(RectTransform));
            hudHost.transform.SetParent(self, false);
            RectTransform hrt = (RectTransform)hudHost.transform;
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.offsetMin = new Vector2(30f, -120f);
            hrt.offsetMax = new Vector2(-30f, -20f);
            hud = CinematicHud.Create(hrt, gamePanel);
        }

        private void BuildTextPanel()
        {
            GameObject panelHost = new GameObject("TextPanel", typeof(RectTransform));
            panelHost.transform.SetParent(self, false);
            RectTransform rt = (RectTransform)panelHost.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0.65f, 1f);
            rt.offsetMin = new Vector2(30f, 180f);
            rt.offsetMax = new Vector2(0f, -140f);

            CinematicGlassPanel.GlassConfig config = CinematicGlassPanel.GlassConfig.Default;
            config.BaseColor = new Color(0.02f, 0.04f, 0.08f, 0.88f);
            config.OutlineColor = new Color(0.55f, 0.85f, 1f, 0.65f);
            config.InnerGlowColor = new Color(0.15f, 0.45f, 0.85f, 0.18f);
            GameObject host = CinematicGlassPanel.Build("TextHost", rt, config);
            RectTransform contentRect = (RectTransform)host.transform;

            GameObject labelGo = new GameObject("LocationLabel", typeof(RectTransform));
            labelGo.transform.SetParent(contentRect, false);
            locationLabel = labelGo.AddComponent<TextMeshProUGUI>();
            locationLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            locationLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            locationLabel.rectTransform.pivot = new Vector2(0f, 1f);
            locationLabel.rectTransform.offsetMin = new Vector2(0f, -26f);
            locationLabel.rectTransform.offsetMax = new Vector2(0f, 0f);
            locationLabel.text = "// LOCATION //";
            locationLabel.fontSize = 13f;
            locationLabel.color = new Color(0.55f, 0.85f, 1f, 0.65f);
            locationLabel.alignment = TextAlignmentOptions.TopLeft;
            locationLabel.fontStyle = FontStyles.Bold;
            locationLabel.characterSpacing = 4f;
            locationLabel.raycastTarget = false;

            GameObject textGo = new GameObject("MainText", typeof(RectTransform));
            textGo.transform.SetParent(contentRect, false);
            mainText = textGo.AddComponent<TextMeshProUGUI>();
            mainText.rectTransform.anchorMin = Vector2.zero;
            mainText.rectTransform.anchorMax = Vector2.one;
            mainText.rectTransform.offsetMin = new Vector2(0f, 0f);
            mainText.rectTransform.offsetMax = new Vector2(0f, -34f);
            mainText.fontSize = 22f;
            mainText.color = new Color(0.95f, 0.96f, 1f, 1f);
            mainText.alignment = TextAlignmentOptions.TopLeft;
#pragma warning disable CS0618
            mainText.enableWordWrapping = true;
#pragma warning restore CS0618
            mainText.lineSpacing = 6f;
            mainText.text = "";
            mainText.raycastTarget = false;
        }

        private void BuildHoloMonitor()
        {
            GameObject panelHost = new GameObject("HoloMonitor", typeof(RectTransform));
            panelHost.transform.SetParent(self, false);
            RectTransform rt = (RectTransform)panelHost.transform;
            rt.anchorMin = new Vector2(0.65f, 0.40f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(20f, 0f);
            rt.offsetMax = new Vector2(-30f, -140f);

            CinematicGlassPanel.GlassConfig config = CinematicGlassPanel.GlassConfig.Default;
            config.BaseColor = new Color(0.02f, 0.04f, 0.10f, 0.85f);
            config.OutlineColor = new Color(0.40f, 0.95f, 1f, 0.80f);
            config.InnerGlowColor = new Color(0.20f, 0.85f, 1f, 0.18f);
            config.ScanlineOpacity = 0.10f;
            GameObject host = CinematicGlassPanel.Build("HoloHost", rt, config);
            RectTransform contentRect = (RectTransform)host.transform;

            GameObject monitorGo = new GameObject("Monitor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            monitorGo.transform.SetParent(contentRect, false);
            RectTransform mrt = (RectTransform)monitorGo.transform;
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.offsetMin = Vector2.zero;
            mrt.offsetMax = new Vector2(0f, -28f);
            holoMonitor = monitorGo.GetComponent<Image>();
            holoMonitor.raycastTarget = false;
            holoMonitor.sprite = ProceduralTextureFactory.CreateRadialGlowSprite(new Color(0.20f, 0.85f, 1f, 0.65f), 1.4f, 192);
            holoMonitor.color = Color.white;

            GameObject scanGo = new GameObject("MonitorScanlines", typeof(RectTransform));
            scanGo.transform.SetParent(mrt, false);
            RectTransform sgt = (RectTransform)scanGo.transform;
            sgt.anchorMin = Vector2.zero;
            sgt.anchorMax = Vector2.one;
            sgt.offsetMin = Vector2.zero;
            sgt.offsetMax = Vector2.zero;
            ScanlinesLayer scan = scanGo.AddComponent<ScanlinesLayer>();
            scan.Apply(0.15f);

            GameObject lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(contentRect, false);
            holoLabel = lblGo.AddComponent<TextMeshProUGUI>();
            holoLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            holoLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            holoLabel.rectTransform.offsetMin = new Vector2(0f, -28f);
            holoLabel.rectTransform.offsetMax = Vector2.zero;
            holoLabel.text = "// HOLO MONITOR · SECTOR FEED //";
            holoLabel.fontSize = 11f;
            holoLabel.color = new Color(0.30f, 0.85f, 1f, 0.85f);
            holoLabel.alignment = TextAlignmentOptions.Center;
            holoLabel.fontStyle = FontStyles.Bold;
            holoLabel.characterSpacing = 4f;
            holoLabel.raycastTarget = false;
        }

        private void BuildChoicesPanel()
        {
            GameObject panelHost = new GameObject("ChoicesPanel", typeof(RectTransform));
            panelHost.transform.SetParent(self, false);
            RectTransform rt = (RectTransform)panelHost.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(30f, 30f);
            rt.offsetMax = new Vector2(-30f, 160f);

            GameObject host = new GameObject("ChoicesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(CanvasRenderer), typeof(Image));
            host.transform.SetParent(rt, false);
            RectTransform hrt = (RectTransform)host.transform;
            hrt.anchorMin = Vector2.zero;
            hrt.anchorMax = Vector2.one;
            hrt.offsetMin = Vector2.zero;
            hrt.offsetMax = Vector2.zero;
            Image hbg = host.GetComponent<Image>();
            hbg.color = new Color(0f, 0f, 0f, 0.001f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(host.transform, false);
            RectTransform vrt = (RectTransform)viewport.transform;
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero;
            vrt.offsetMax = Vector2.zero;
            Image vimg = viewport.GetComponent<Image>();
            vimg.color = new Color(0f, 0f, 0f, 0.001f);
            Mask mask = viewport.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform crt = (RectTransform)content.transform;
            crt.anchorMin = new Vector2(0f, 0f);
            crt.anchorMax = new Vector2(0f, 1f);
            crt.pivot = new Vector2(0f, 0.5f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            crt.sizeDelta = new Vector2(0f, 0f);
            HorizontalLayoutGroup hlg = content.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14f;
            hlg.padding = new RectOffset(8, 8, 8, 8);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = host.GetComponent<ScrollRect>();
            sr.viewport = vrt;
            sr.content = crt;
            sr.horizontal = true;
            sr.vertical = false;
            sr.movementType = ScrollRect.MovementType.Elastic;

            choicesContent = crt;
        }

        private void BuildExitButton()
        {
            GameObject btnGo = new GameObject("ExitButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(self, false);
            RectTransform rt = (RectTransform)btnGo.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(48f, 48f);
            rt.anchoredPosition = new Vector2(-30f, -130f);
            Image bg = btnGo.GetComponent<Image>();
            bg.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(0.10f, 0.05f, 0.05f, 1f), 10, 32);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.10f, 0.05f, 0.05f, 0.85f);
            Button btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => { if (gamePanel != null) gamePanel.AbandonQuest(); });
            exitButton = btn;

            GameObject lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(btnGo.transform, false);
            TextMeshProUGUI lbl = lblGo.AddComponent<TextMeshProUGUI>();
            lbl.rectTransform.anchorMin = Vector2.zero;
            lbl.rectTransform.anchorMax = Vector2.one;
            lbl.rectTransform.offsetMin = Vector2.zero;
            lbl.rectTransform.offsetMax = Vector2.zero;
            lbl.text = "✕";
            lbl.fontSize = 28f;
            lbl.color = new Color(0.95f, 0.55f, 0.55f, 1f);
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.raycastTarget = false;
        }
    }
}
