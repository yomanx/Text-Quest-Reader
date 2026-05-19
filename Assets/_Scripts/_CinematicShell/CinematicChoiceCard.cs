using System;
using TextQuestReader.Cinematic;
using TextQuestReader.Cinematic.Procedural;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Large interactive choice card used by CinematicReaderScreen. Visualises
    /// mood (danger/reward/story/normal/locked), shows pulsing glow on hover,
    /// strips mood markers from the displayed text.
    /// </summary>
    public class CinematicChoiceCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Action onClickedAction;
        private Image bg;
        private Image glow;
        private Image accent;
        private Image moodIcon;
        private TextMeshProUGUI label;
        private TextMeshProUGUI moodLabel;
        private bool hovering;
        private float pulse;
        private ChoiceMood mood;
        private bool disabled;

        public static CinematicChoiceCard Create(RectTransform parent, Passage passage, bool disabled, Action<Passage> onClicked)
        {
            ChoiceMood m = disabled ? ChoiceMood.Locked : ChoiceVisualState.InferFromText(passage.question);
            string text = ChoiceVisualState.StripMoodTags(passage.question);
            CinematicChoiceCard card = CreateRaw(parent, text, m, disabled);
            card.onClickedAction = () => onClicked?.Invoke(passage);
            return card;
        }

        public static CinematicChoiceCard CreateNext(RectTransform parent, Passage next, Action onClicked)
        {
            string text = "▶  " + (next != null ? next.question : "Continue");
            CinematicChoiceCard card = CreateRaw(parent, text, ChoiceMood.Story, false);
            card.onClickedAction = onClicked;
            return card;
        }

        private static CinematicChoiceCard CreateRaw(RectTransform parent, string text, ChoiceMood mood, bool disabled)
        {
            GameObject go = new GameObject("ChoiceCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            LayoutElement le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 300f;
            le.preferredHeight = 110f;

            CinematicChoiceCard card = go.AddComponent<CinematicChoiceCard>();
            card.bg = go.GetComponent<Image>();
            card.mood = mood;
            card.disabled = disabled;
            card.Build(text);

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = card.bg;
            btn.interactable = !disabled;
            btn.onClick.AddListener(card.OnClicked);
            return card;
        }

        private void Build(string text)
        {
            RectTransform rt = (RectTransform)transform;
            Color accentColor = MoodColor();

            bg.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(0.04f, 0.08f, 0.14f, 1f), 18, 96);
            bg.type = Image.Type.Sliced;
            bg.color = disabled
                ? new Color(0.05f, 0.06f, 0.08f, 0.65f)
                : new Color(0.04f, 0.08f, 0.14f, 0.92f);

            GameObject glowGo = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowGo.transform.SetParent(rt, false);
            RectTransform grt = (RectTransform)glowGo.transform;
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = new Vector2(-12f, -10f);
            grt.offsetMax = new Vector2(12f, 10f);
            grt.SetAsFirstSibling();
            glow = glowGo.GetComponent<Image>();
            glow.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(accentColor, 22, 96);
            glow.type = Image.Type.Sliced;
            glow.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0f);
            glow.raycastTarget = false;

            GameObject accentGo = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accentGo.transform.SetParent(rt, false);
            RectTransform art = (RectTransform)accentGo.transform;
            art.anchorMin = new Vector2(0f, 0f);
            art.anchorMax = new Vector2(0f, 1f);
            art.pivot = new Vector2(0f, 0.5f);
            art.offsetMin = new Vector2(0f, 10f);
            art.offsetMax = new Vector2(0f, -10f);
            art.sizeDelta = new Vector2(6f, 0f);
            accent = accentGo.GetComponent<Image>();
            accent.color = accentColor;
            accent.raycastTarget = false;

            GameObject iconGo = new GameObject("MoodIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(rt, false);
            RectTransform irt = (RectTransform)iconGo.transform;
            irt.anchorMin = new Vector2(0f, 1f);
            irt.anchorMax = new Vector2(0f, 1f);
            irt.pivot = new Vector2(0f, 1f);
            irt.offsetMin = new Vector2(18f, -32f);
            irt.offsetMax = new Vector2(18f, -32f);
            irt.sizeDelta = new Vector2(16f, 16f);
            moodIcon = iconGo.GetComponent<Image>();
            moodIcon.sprite = ProceduralTextureFactory.CreateRadialGlowSprite(accentColor, 1.6f, 32);
            moodIcon.color = Color.white;
            moodIcon.raycastTarget = false;

            GameObject moodLblGo = new GameObject("MoodLabel", typeof(RectTransform));
            moodLblGo.transform.SetParent(rt, false);
            moodLabel = moodLblGo.AddComponent<TextMeshProUGUI>();
            moodLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            moodLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            moodLabel.rectTransform.pivot = new Vector2(0f, 1f);
            moodLabel.rectTransform.offsetMin = new Vector2(40f, -32f);
            moodLabel.rectTransform.offsetMax = new Vector2(-12f, -16f);
            moodLabel.text = MoodLabel().ToUpperInvariant();
            moodLabel.fontSize = 10f;
            moodLabel.color = accentColor;
            moodLabel.alignment = TextAlignmentOptions.TopLeft;
            moodLabel.fontStyle = FontStyles.Bold;
            moodLabel.characterSpacing = 4f;
            moodLabel.raycastTarget = false;

            GameObject lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(rt, false);
            label = lblGo.AddComponent<TextMeshProUGUI>();
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(18f, 14f);
            label.rectTransform.offsetMax = new Vector2(-18f, -36f);
            label.text = text;
            label.fontSize = 15f;
            label.fontStyle = FontStyles.Normal;
            label.color = disabled ? new Color(0.55f, 0.55f, 0.65f, 0.85f) : new Color(0.95f, 0.96f, 1f, 1f);
            label.alignment = TextAlignmentOptions.TopLeft;
#pragma warning disable CS0618
            label.enableWordWrapping = true;
#pragma warning restore CS0618
            label.raycastTarget = false;
        }

        private Color MoodColor()
        {
            switch (mood)
            {
                case ChoiceMood.Danger: return new Color(0.95f, 0.30f, 0.30f, 1f);
                case ChoiceMood.Reward: return new Color(0.95f, 0.80f, 0.30f, 1f);
                case ChoiceMood.Story: return new Color(0.65f, 0.55f, 0.95f, 1f);
                case ChoiceMood.Locked: return new Color(0.45f, 0.50f, 0.60f, 1f);
                default: return new Color(0.40f, 0.75f, 0.95f, 1f);
            }
        }

        private string MoodLabel()
        {
            switch (mood)
            {
                case ChoiceMood.Danger: return "Danger";
                case ChoiceMood.Reward: return "Reward";
                case ChoiceMood.Story: return "Story";
                case ChoiceMood.Locked: return "Locked";
                default: return "Choice";
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
            transform.localScale = Vector3.one * 1.025f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (glow == null || disabled) return;
            pulse += Time.deltaTime * (hovering ? 4f : 1.4f);
            float baseA = hovering ? 0.55f : 0.18f;
            float a = baseA + 0.18f * Mathf.Sin(pulse);
            Color c = MoodColor();
            glow.color = new Color(c.r, c.g, c.b, a);
        }

        private void OnClicked()
        {
            if (disabled) return;
            onClickedAction?.Invoke();
        }
    }
}
