using TextQuestReader.Cinematic.Procedural;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic
{
    public enum ChoiceMood
    {
        Normal,
        Danger,
        Reward,
        Story,
        Locked
    }

    /// <summary>
    /// Runtime visual enhancer that lifts a vanilla button-based QuestionCell
    /// into a "choice card" look — accent bar, mood icon, colored border, slight
    /// drop shadow, hover lift animation. Attaches to an existing QuestionCell
    /// without needing prefab edits.
    /// </summary>
    public class ChoiceVisualState : MonoBehaviour
    {
        private Image accentBar;
        private Image moodIcon;
        private Image glowFringe;
        private float glowPhase;
        private ChoiceMood mood = ChoiceMood.Normal;
        private bool isHovered;

        // Palette tuned for the polish pass:
        //   Normal -> cyan-blue, slightly more saturated than the source so
        //             the bar reads as "active interactive control".
        //   Danger -> warm red (slightly less crimson, more orange-warm so
        //             it doesn't fight with the reactor scenes).
        //   Reward -> warm gold (unchanged — already strong).
        //   Story  -> calmer violet (lowered saturation so it doesn't compete
        //             with Danger when shown side-by-side).
        //   Locked -> low-contrast slate so the disabled state is obvious.
        private static readonly Color NormalColor = new Color(0.40f, 0.78f, 0.98f, 1f);
        private static readonly Color DangerColor = new Color(0.95f, 0.40f, 0.30f, 1f);
        private static readonly Color RewardColor = new Color(0.97f, 0.80f, 0.32f, 1f);
        private static readonly Color StoryColor = new Color(0.62f, 0.50f, 0.92f, 1f);
        private static readonly Color LockedColor = new Color(0.40f, 0.43f, 0.50f, 1f);

        public ChoiceMood Mood => mood;

        public void EnsureBuilt()
        {
            if (accentBar != null && moodIcon != null && glowFringe != null) return;

            RectTransform host = (RectTransform)transform;

            if (accentBar == null)
            {
                GameObject bar = new GameObject("AccentBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bar.transform.SetParent(host, false);
                RectTransform rt = (RectTransform)bar.transform;
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                // Inset 2px from top/bottom so the bar reads as a marker
                // line rather than a wall — works at any cell height.
                rt.anchoredPosition = new Vector2(0f, 0f);
                rt.offsetMin = new Vector2(0f, 2f);
                rt.offsetMax = new Vector2(0f, -2f);
                // 5px wide — readable but still inside the original hit area.
                rt.sizeDelta = new Vector2(5f, 0f);

                accentBar = bar.GetComponent<Image>();
                accentBar.raycastTarget = false;
                accentBar.color = NormalColor;
                rt.SetAsFirstSibling();
            }

            if (moodIcon == null)
            {
                GameObject icon = new GameObject("MoodIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                icon.transform.SetParent(host, false);
                RectTransform rt = (RectTransform)icon.transform;
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-10f, 0f);
                rt.sizeDelta = new Vector2(14f, 14f);

                moodIcon = icon.GetComponent<Image>();
                moodIcon.raycastTarget = false;
                moodIcon.sprite = ProceduralTextureFactory.CreateRadialGlowSprite(NormalColor, 1.8f, 64);
                moodIcon.color = Color.white;
            }

            if (glowFringe == null)
            {
                GameObject fringe = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fringe.transform.SetParent(host, false);
                RectTransform rt = (RectTransform)fringe.transform;
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.offsetMin = new Vector2(-22f, -8f);
                rt.offsetMax = new Vector2(22f, 8f);
                glowFringe = fringe.GetComponent<Image>();
                glowFringe.raycastTarget = false;
                glowFringe.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(NormalColor, 28, 96);
                glowFringe.color = new Color(NormalColor.r, NormalColor.g, NormalColor.b, 0f);
                glowFringe.type = Image.Type.Sliced;
                rt.SetAsFirstSibling();
            }
        }

        private void Update()
        {
            if (glowFringe == null) return;
            if (mood == ChoiceMood.Locked) return;

            // Hover speeds up + brightens the breathing glow so the active
            // choice is unambiguously "live"; idle stays subtle.
            float speed = isHovered ? 3.2f : 1.4f;
            glowPhase += Time.deltaTime * speed;
            float wave = 0.5f + 0.5f * Mathf.Sin(glowPhase);

            Color c = MoodToColor(mood);
            float baseAlpha = mood switch
            {
                ChoiceMood.Danger => 0.34f,
                ChoiceMood.Reward => 0.32f,
                ChoiceMood.Story => 0.26f,
                _ => 0.18f
            };
            if (isHovered) baseAlpha *= 1.8f;

            glowFringe.color = new Color(c.r, c.g, c.b, baseAlpha * wave + (isHovered ? 0.18f : 0.08f));

            // Accent bar tracks hover too: more saturated/brighter on hover.
            if (accentBar != null)
            {
                float k = isHovered ? 1f : 0.85f;
                accentBar.color = new Color(c.r * k + (1f - k) * 0.15f,
                                            c.g * k + (1f - k) * 0.15f,
                                            c.b * k + (1f - k) * 0.15f,
                                            isHovered ? 1f : 0.85f);
            }
        }

        public void SetMood(ChoiceMood newMood)
        {
            EnsureBuilt();
            mood = newMood;
            Color c = MoodToColor(mood);
            if (accentBar != null)
            {
                accentBar.color = c;
                // For Locked, also dim and thin the bar so disabled choices
                // visually recede instead of competing with active ones.
                RectTransform rt = (RectTransform)accentBar.transform;
                if (mood == ChoiceMood.Locked)
                {
                    rt.sizeDelta = new Vector2(3f, 0f);
                    accentBar.color = new Color(c.r, c.g, c.b, 0.45f);
                }
                else
                {
                    rt.sizeDelta = new Vector2(5f, 0f);
                }
            }
            if (moodIcon != null)
            {
                moodIcon.color = mood == ChoiceMood.Locked ? new Color(c.r, c.g, c.b, 0.35f) : c;
            }
            if (glowFringe != null)
            {
                if (mood == ChoiceMood.Locked)
                    glowFringe.color = new Color(c.r, c.g, c.b, 0f);
            }
        }

        public void SetHoverState(bool hover)
        {
            // Locked cells ignore hover — they have no live response.
            if (mood == ChoiceMood.Locked) { isHovered = false; return; }
            isHovered = hover;
            // Reset phase on hover-on so the first wave is a clean ramp-up,
            // not whatever angle the idle sine happened to be at.
            if (hover) glowPhase = 0f;
        }

        public static ChoiceMood InferFromText(string text)
        {
            if (string.IsNullOrEmpty(text)) return ChoiceMood.Normal;
            string lower = text.ToLowerInvariant();

            if (lower.Contains("[danger]") || lower.Contains("[risk]")) return ChoiceMood.Danger;
            if (lower.Contains("[reward]") || lower.Contains("[loot]")) return ChoiceMood.Reward;
            if (lower.Contains("[story]") || lower.Contains("[lore]")) return ChoiceMood.Story;
            if (lower.Contains("[locked]")) return ChoiceMood.Locked;
            return ChoiceMood.Normal;
        }

        public static string StripMoodTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace("[danger]", "").Replace("[risk]", "")
                .Replace("[reward]", "").Replace("[loot]", "")
                .Replace("[story]", "").Replace("[lore]", "")
                .Replace("[locked]", "")
                .Trim();
        }

        private static Color MoodToColor(ChoiceMood m)
        {
            switch (m)
            {
                case ChoiceMood.Danger: return DangerColor;
                case ChoiceMood.Reward: return RewardColor;
                case ChoiceMood.Story: return StoryColor;
                case ChoiceMood.Locked: return LockedColor;
                default: return NormalColor;
            }
        }
    }
}
