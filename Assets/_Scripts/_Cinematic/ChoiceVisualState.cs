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

        private static readonly Color NormalColor = new Color(0.45f, 0.55f, 0.75f, 1f);
        private static readonly Color DangerColor = new Color(0.85f, 0.25f, 0.25f, 1f);
        private static readonly Color RewardColor = new Color(0.95f, 0.78f, 0.30f, 1f);
        private static readonly Color StoryColor = new Color(0.55f, 0.45f, 0.85f, 1f);
        private static readonly Color LockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

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
                rt.anchoredPosition = new Vector2(0f, 0f);
                rt.sizeDelta = new Vector2(4f, 0f);

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
            glowPhase += Time.deltaTime * 1.4f;
            float wave = 0.5f + 0.5f * Mathf.Sin(glowPhase);
            Color c = MoodToColor(mood);
            float baseAlpha = mood switch
            {
                ChoiceMood.Danger => 0.30f,
                ChoiceMood.Reward => 0.30f,
                ChoiceMood.Story => 0.25f,
                _ => 0.16f
            };
            glowFringe.color = new Color(c.r, c.g, c.b, baseAlpha * wave + 0.08f);
        }

        public void SetMood(ChoiceMood newMood)
        {
            EnsureBuilt();
            mood = newMood;
            Color c = MoodToColor(mood);
            if (accentBar != null) accentBar.color = c;
            if (moodIcon != null) moodIcon.color = c;
            if (glowFringe != null)
            {
                if (mood == ChoiceMood.Locked)
                    glowFringe.color = new Color(c.r, c.g, c.b, 0f);
            }
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
