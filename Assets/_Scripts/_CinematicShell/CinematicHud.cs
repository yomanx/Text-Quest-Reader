using System.Collections.Generic;
using TextQuestReader.Cinematic.Procedural;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Top HUD strip showing each quest parameter as a labelled progress bar
    /// with current value, gradient fill, and a critical-state warning blink.
    /// </summary>
    public class CinematicHud : MonoBehaviour
    {
        private struct ParamUi
        {
            public Parameter parameter;
            public RectTransform host;
            public TextMeshProUGUI label;
            public TextMeshProUGUI value;
            public Image fill;
            public Image fillBg;
            public Image warningGlow;
        }

        private GamePanel gamePanel;
        private RectTransform host;
        private readonly List<ParamUi> paramUis = new List<ParamUi>();
        private Quest lastQuest;
        private float warningPhase;

        public static CinematicHud Create(RectTransform parent, GamePanel gamePanel)
        {
            GameObject go = new GameObject("HudStrip", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            CinematicHud hud = go.AddComponent<CinematicHud>();
            hud.gamePanel = gamePanel;
            hud.host = rt;
            hud.BuildBackdrop();
            return hud;
        }

        private void BuildBackdrop()
        {
            CinematicGlassPanel.GlassConfig config = CinematicGlassPanel.GlassConfig.Default;
            config.BaseColor = new Color(0.02f, 0.04f, 0.10f, 0.78f);
            config.OutlineColor = new Color(0.20f, 0.65f, 0.95f, 0.55f);
            config.InnerGlowColor = new Color(0.20f, 0.65f, 0.95f, 0.15f);
            config.ScanlineOpacity = 0.06f;
            config.CornerRadius = 14;
            GameObject content = CinematicGlassPanel.Build("HudGlass", host, config);
            RectTransform contentRect = (RectTransform)content.transform;

            GameObject group = new GameObject("Slots", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            group.transform.SetParent(contentRect, false);
            RectTransform grt = (RectTransform)group.transform;
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = Vector2.zero;
            grt.offsetMax = Vector2.zero;
            HorizontalLayoutGroup hlg = group.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14f;
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            host = grt;
        }

        public void Refresh(Quest quest)
        {
            if (host == null) return;
            if (quest == null) { ClearSlots(); lastQuest = null; return; }

            if (lastQuest != quest)
            {
                BuildSlotsFor(quest);
                lastQuest = quest;
            }

            for (int i = 0; i < paramUis.Count; i++)
                UpdateSlot(paramUis[i]);
        }

        private void ClearSlots()
        {
            for (int i = host.childCount - 1; i >= 0; i--)
                Destroy(host.GetChild(i).gameObject);
            paramUis.Clear();
        }

        private void BuildSlotsFor(Quest quest)
        {
            ClearSlots();
            foreach (Parameter p in quest.parameters)
            {
                if (!p.isActive || p.isHidden) continue;
                BuildSlot(p);
            }
        }

        private void BuildSlot(Parameter parameter)
        {
            GameObject slot = new GameObject("Slot_" + parameter.workingName, typeof(RectTransform), typeof(LayoutElement));
            slot.transform.SetParent(host, false);
            LayoutElement le = slot.GetComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.preferredWidth = 180f;
            le.preferredHeight = 70f;

            RectTransform srt = (RectTransform)slot.transform;

            GameObject lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(srt, false);
            TextMeshProUGUI label = lblGo.AddComponent<TextMeshProUGUI>();
            label.rectTransform.anchorMin = new Vector2(0f, 1f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.pivot = new Vector2(0f, 1f);
            label.rectTransform.offsetMin = new Vector2(0f, -22f);
            label.rectTransform.offsetMax = new Vector2(0f, 0f);
            label.text = parameter.workingName.ToUpperInvariant();
            label.fontSize = 12f;
            label.color = new Color(0.55f, 0.85f, 1f, 0.85f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontStyle = FontStyles.Bold;
            label.characterSpacing = 4f;
            label.raycastTarget = false;

            GameObject valGo = new GameObject("Value", typeof(RectTransform));
            valGo.transform.SetParent(srt, false);
            TextMeshProUGUI value = valGo.AddComponent<TextMeshProUGUI>();
            value.rectTransform.anchorMin = new Vector2(0f, 1f);
            value.rectTransform.anchorMax = new Vector2(1f, 1f);
            value.rectTransform.pivot = new Vector2(0f, 1f);
            value.rectTransform.offsetMin = new Vector2(0f, -46f);
            value.rectTransform.offsetMax = new Vector2(0f, -22f);
            value.fontSize = 18f;
            value.color = new Color(0.95f, 0.95f, 1f, 1f);
            value.alignment = TextAlignmentOptions.MidlineLeft;
            value.fontStyle = FontStyles.Bold;
            value.raycastTarget = false;

            GameObject fillBgGo = new GameObject("BarBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillBgGo.transform.SetParent(srt, false);
            RectTransform brt = (RectTransform)fillBgGo.transform;
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.offsetMin = new Vector2(0f, 0f);
            brt.offsetMax = new Vector2(0f, 12f);
            Image bg = fillBgGo.GetComponent<Image>();
            bg.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(Color.white, 4, 16);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.10f, 0.15f, 0.20f, 1f);
            bg.raycastTarget = false;

            GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(brt, false);
            RectTransform frt = (RectTransform)fillGo.transform;
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(0f, 1f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.offsetMin = new Vector2(0f, 0f);
            frt.offsetMax = new Vector2(0f, 0f);
            frt.sizeDelta = new Vector2(0f, 0f);
            Image fillImg = fillGo.GetComponent<Image>();
            fillImg.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(Color.white, 4, 16);
            fillImg.type = Image.Type.Sliced;
            fillImg.color = ResolveColorFor(parameter);
            fillImg.raycastTarget = false;

            GameObject warnGo = new GameObject("WarnGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            warnGo.transform.SetParent(brt, false);
            RectTransform wrt = (RectTransform)warnGo.transform;
            wrt.anchorMin = new Vector2(0f, 0f);
            wrt.anchorMax = new Vector2(1f, 1f);
            wrt.offsetMin = new Vector2(-6f, -4f);
            wrt.offsetMax = new Vector2(6f, 4f);
            Image warn = warnGo.GetComponent<Image>();
            warn.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(1f, 0.2f, 0.2f, 1f), 6, 24);
            warn.type = Image.Type.Sliced;
            warn.color = new Color(1f, 0.2f, 0.2f, 0f);
            warn.raycastTarget = false;
            wrt.SetAsFirstSibling();

            paramUis.Add(new ParamUi
            {
                parameter = parameter,
                host = srt,
                label = label,
                value = value,
                fill = fillImg,
                fillBg = bg,
                warningGlow = warn
            });
        }

        private Color ResolveColorFor(Parameter p)
        {
            switch (p.paramType)
            {
                case ParamType.Successful: return new Color(0.40f, 0.95f, 0.55f, 1f);
                case ParamType.Failed: return new Color(0.30f, 0.85f, 0.95f, 1f);
                default: return new Color(0.95f, 0.80f, 0.30f, 1f);
            }
        }

        private bool IsNearCritical(Parameter p)
        {
            if (p.paramType == ParamType.Usual) return false;
            int range = Mathf.Max(1, p.maxValue - p.minValue);
            int threshold = Mathf.Max(1, range / 5);
            return p.isCriticMax
                ? p.value >= p.maxValue - threshold
                : p.value <= p.minValue + threshold;
        }

        private void UpdateSlot(ParamUi ui)
        {
            float range = Mathf.Max(1, ui.parameter.maxValue - ui.parameter.minValue);
            float normalized = Mathf.Clamp01((float)(ui.parameter.value - ui.parameter.minValue) / range);
            ui.fill.rectTransform.anchorMax = new Vector2(normalized, 1f);
            ui.value.text = ui.parameter.value.ToString() + "<size=10><color=#88aaccaa>  / " + ui.parameter.maxValue + "</color></size>";
            bool nearCritical = IsNearCritical(ui.parameter);
            if (nearCritical)
            {
                float a = 0.40f + 0.30f * Mathf.Abs(Mathf.Sin(warningPhase));
                ui.warningGlow.color = new Color(1f, 0.20f, 0.20f, a);
                ui.fill.color = Color.Lerp(ResolveColorFor(ui.parameter), new Color(1f, 0.30f, 0.30f, 1f), 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(warningPhase)));
            }
            else
            {
                ui.warningGlow.color = new Color(1f, 0.20f, 0.20f, 0f);
                ui.fill.color = ResolveColorFor(ui.parameter);
            }
        }

        private void Update()
        {
            warningPhase += Time.deltaTime * 5f;
            for (int i = 0; i < paramUis.Count; i++)
                UpdateSlot(paramUis[i]);
        }
    }
}
