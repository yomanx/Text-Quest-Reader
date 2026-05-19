using System.Collections;
using TextQuestReader.Cinematic.Procedural;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic
{
    /// <summary>
    /// Animates a parameter row: highlights on change, pulses on critical values,
    /// fades color toward green when value grew or red when value dropped.
    /// Attaches at runtime to the GameObject the ParameterService spawns.
    /// </summary>
    public class ParameterAnimator : MonoBehaviour
    {
        private Vector3 originalScale = Vector3.one;
        private bool originalCaptured;
        private Coroutine pulseRoutine;
        private CanvasGroup canvasGroup;
        private Image hudFrame;
        private float hudBlinkPhase;
        private bool hudCritical;

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
        }

        private void EnsureHudFrame()
        {
            if (hudFrame != null) return;

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
            hudFrame.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(0.20f, 0.60f, 0.95f, 1f), 12, 48);
            hudFrame.type = Image.Type.Sliced;
            hudFrame.color = new Color(0.20f, 0.60f, 0.95f, 0.12f);
        }

        public void SetCriticalState(bool critical)
        {
            EnsureCanvasGroup();
            hudCritical = critical;
            if (hudFrame == null) return;
            hudFrame.color = critical
                ? new Color(1f, 0.30f, 0.30f, 0.20f)
                : new Color(0.20f, 0.60f, 0.95f, 0.12f);
        }

        private void Update()
        {
            if (hudFrame == null || !hudCritical) return;
            hudBlinkPhase += Time.deltaTime * 6f;
            float a = 0.20f + 0.20f * Mathf.Abs(Mathf.Sin(hudBlinkPhase));
            Color c = hudFrame.color;
            hudFrame.color = new Color(c.r, c.g, c.b, a);
        }

        public void PlayChange(int delta, bool isCritical)
        {
            EnsureCanvasGroup();

            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseRoutine(delta, isCritical));
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
    }
}
