using System.Collections;
using TextQuestReader.Cinematic.Procedural;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic
{
    /// <summary>
    /// Animates a parameter row: highlights on change, pulses on critical
    /// values, shows a brief floating +N / -N delta label when the value
    /// shifts, and lifts text/border contrast so low-oxygen / low-health
    /// states are immediately legible. Attaches at runtime to the
    /// GameObject the ParameterService spawns.
    /// </summary>
    public class ParameterAnimator : MonoBehaviour
    {
        // Transform / canvas group cached state
        private Vector3 originalScale = Vector3.one;
        private bool originalCaptured;
        private CanvasGroup canvasGroup;

        // Frame visuals — inner rounded frame + softer outer halo
        private Image hudFrame;
        private Image outerHalo;
        private float hudBlinkPhase;
        private bool hudCritical;

        // Floating delta label (+N / -N) shown briefly on value change
        private TMP_Text deltaLabel;
        private Coroutine deltaRoutine;

        // Pulse animation on the cell scale
        private Coroutine pulseRoutine;

        // Cached reference to the cell's text component for outline-tweak only
        private TMP_Text cellText;
        private bool cellTextTouched;

        private static readonly Color FrameNormalColor   = new Color(0.20f, 0.60f, 0.95f, 1f);
        private static readonly Color FrameCriticalColor = new Color(1.00f, 0.30f, 0.30f, 1f);
        private static readonly Color DeltaUpColor       = new Color(0.45f, 0.95f, 0.55f, 1f);
        private static readonly Color DeltaDownColor     = new Color(1.00f, 0.45f, 0.45f, 1f);

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (!originalCaptured)
            {
                originalScale = transform.localScale;
                originalCaptured = true;
            }

            EnsureHudFrame();
            EnsureCellTextOutline();
        }

        private void EnsureHudFrame()
        {
            // Inner rounded frame — always present. Reused on repeated refresh.
            if (hudFrame == null)
            {
                GameObject go = new GameObject("HudFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(transform, false);
                go.transform.SetAsFirstSibling();

                RectTransform rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(-6f, -3f);
                rt.offsetMax = new Vector2(6f, 3f);

                hudFrame = go.GetComponent<Image>();
                hudFrame.raycastTarget = false;
                hudFrame.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(Color.white, 12, 48);
                hudFrame.type = Image.Type.Sliced;
                hudFrame.color = new Color(FrameNormalColor.r, FrameNormalColor.g, FrameNormalColor.b, 0.14f);
            }

            // Outer halo — wider, dimmer, behind the frame. Off in normal state,
            // pulses softly when critical. Adds depth + readability.
            if (outerHalo == null)
            {
                GameObject go = new GameObject("HudOuterHalo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(transform, false);
                go.transform.SetAsFirstSibling();   // even further back than frame

                RectTransform rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(-14f, -8f);
                rt.offsetMax = new Vector2(14f, 8f);

                outerHalo = go.GetComponent<Image>();
                outerHalo.raycastTarget = false;
                outerHalo.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(Color.white, 18, 60);
                outerHalo.type = Image.Type.Sliced;
                outerHalo.color = new Color(FrameNormalColor.r, FrameNormalColor.g, FrameNormalColor.b, 0f);
            }
        }

        private void EnsureCellTextOutline()
        {
            // Add a very thin outline / softer face shadow to the cell text so
            // the value digits stay legible on busy backgrounds (reactor, alarm,
            // shake/flash). This is a one-time per-cell tweak — guard so we
            // don't stack outlines if the animator is rebound.
            if (cellTextTouched) return;
            if (cellText == null) cellText = GetComponent<TMP_Text>();
            if (cellText == null) return;
            try
            {
                cellText.outlineColor = new Color32(0, 0, 0, 200);
                cellText.outlineWidth = 0.18f;
                cellTextTouched = true;
            }
            catch
            {
                // Material may not expose outline keywords; ignore silently —
                // text stays at default contrast.
                cellTextTouched = true;
            }
        }

        public void SetCriticalState(bool critical)
        {
            EnsureCanvasGroup();
            hudCritical = critical;
            if (hudFrame != null)
            {
                hudFrame.color = critical
                    ? new Color(FrameCriticalColor.r, FrameCriticalColor.g, FrameCriticalColor.b, 0.28f)
                    : new Color(FrameNormalColor.r,   FrameNormalColor.g,   FrameNormalColor.b,   0.14f);
            }
            if (outerHalo != null && !critical)
                outerHalo.color = new Color(FrameNormalColor.r, FrameNormalColor.g, FrameNormalColor.b, 0f);
        }

        private void Update()
        {
            if (!hudCritical || hudFrame == null) return;

            // Heart-beat pulse: pow(|sin|, 0.35) gives sharp peaks and longer
            // 'rests' instead of a flat sin wave. Period ~0.9s feels organic.
            hudBlinkPhase += Time.deltaTime * (2f * Mathf.PI / 0.9f);
            float beat = Mathf.Pow(Mathf.Abs(Mathf.Sin(hudBlinkPhase)), 0.35f);

            // Inner frame: 0.28 baseline, peaks at 0.55. Still tasteful — the
            // text behind always reads clearly.
            hudFrame.color = new Color(FrameCriticalColor.r, FrameCriticalColor.g, FrameCriticalColor.b, 0.28f + 0.27f * beat);

            // Outer halo: softer, dimmer, peaks at 0.22. Adds depth without
            // washing out the cell.
            if (outerHalo != null)
                outerHalo.color = new Color(FrameCriticalColor.r, FrameCriticalColor.g, FrameCriticalColor.b, 0.06f + 0.16f * beat);
        }

        public void PlayChange(int delta, bool isCritical)
        {
            EnsureCanvasGroup();

            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseRoutine(delta, isCritical));

            if (delta != 0)
                SpawnDeltaLabel(delta);
        }

        public void PlayAppear()
        {
            EnsureCanvasGroup();
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(AppearRoutine());
        }

        private IEnumerator AppearRoutine()
        {
            transform.localScale = originalScale * 0.85f;
            canvasGroup.alpha = 0f;

            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.25f);
                transform.localScale = Vector3.Lerp(originalScale * 0.85f, originalScale, k);
                canvasGroup.alpha = k;
                yield return null;
            }

            transform.localScale = originalScale;
            canvasGroup.alpha = 1f;
            pulseRoutine = null;
        }

        private IEnumerator PulseRoutine(int delta, bool isCritical)
        {
            Vector3 peak = originalScale * (1f + (isCritical ? 0.22f : 0.12f));
            float duration = isCritical ? 0.55f : 0.35f;
            int pulses = isCritical ? 3 : 1;

            for (int p = 0; p < pulses; p++)
            {
                float t = 0f;
                while (t < duration * 0.5f)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / (duration * 0.5f));
                    transform.localScale = Vector3.Lerp(originalScale, peak, k);
                    yield return null;
                }

                t = 0f;
                while (t < duration * 0.5f)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / (duration * 0.5f));
                    transform.localScale = Vector3.Lerp(peak, originalScale, k);
                    yield return null;
                }
            }

            transform.localScale = originalScale;
            pulseRoutine = null;
        }

        private void EnsureDeltaLabel()
        {
            if (deltaLabel != null) return;

            GameObject go = new GameObject("DeltaLabel", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(60f, 24f);
            rt.anchoredPosition = new Vector2(8f, 0f);

            deltaLabel = go.AddComponent<TextMeshProUGUI>();
            deltaLabel.fontSize = 14f;
            deltaLabel.fontStyle = FontStyles.Bold;
            deltaLabel.alignment = TextAlignmentOptions.MidlineLeft;
            deltaLabel.raycastTarget = false;
            try
            {
                deltaLabel.outlineColor = new Color32(0, 0, 0, 220);
                deltaLabel.outlineWidth = 0.20f;
            }
            catch
            {
                // outline shader keywords not available — fall through
            }
        }

        private void SpawnDeltaLabel(int delta)
        {
            EnsureDeltaLabel();
            if (deltaLabel == null) return;

            if (deltaRoutine != null) StopCoroutine(deltaRoutine);

            string sign = delta > 0 ? "+" : "";
            deltaLabel.text = sign + delta.ToString();
            deltaLabel.color = delta > 0 ? DeltaUpColor : DeltaDownColor;

            deltaRoutine = StartCoroutine(DeltaFloatRoutine());
        }

        private IEnumerator DeltaFloatRoutine()
        {
            RectTransform rt = deltaLabel.rectTransform;
            Vector2 start = new Vector2(8f, 0f);
            Vector2 end = new Vector2(8f, 22f);
            rt.anchoredPosition = start;
            Color c = deltaLabel.color;
            deltaLabel.color = new Color(c.r, c.g, c.b, 1f);
            rt.localScale = Vector3.one * 1.15f;

            // Quick scale-in (0..0.12s), float-and-fade (0.12..1.20s)
            float t = 0f;
            while (t < 0.12f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.12f);
                rt.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, k);
                yield return null;
            }

            float total = 1.08f;
            t = 0f;
            while (t < total)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / total);
                rt.anchoredPosition = Vector2.Lerp(start, end, k);
                deltaLabel.color = new Color(c.r, c.g, c.b, 1f - k);
                yield return null;
            }

            deltaLabel.color = new Color(c.r, c.g, c.b, 0f);
            rt.anchoredPosition = start;
            deltaRoutine = null;
        }
    }
}
