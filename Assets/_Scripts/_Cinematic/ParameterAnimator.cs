using System.Collections;
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
