using System.Collections;
using System.Collections.Generic;
using TextQuestReader.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic
{
    /// <summary>
    /// Central cinematic effect coordinator. Holds and creates runtime overlays
    /// (fade, flash, vignette, tint, particles) and shake transforms.
    /// Designed to be safely auto-bootstrapped — if the user has not attached a
    /// CinematicEffectsService component, GamePanel will look up Instance which
    /// triggers auto-creation on demand.
    /// </summary>
    public class CinematicEffectsService : MonoBehaviour
    {
        public static CinematicEffectsService Instance { get; private set; }

        [SerializeField] private Canvas hostCanvas;
        [SerializeField] private RectTransform shakeRoot;

        private Image fadeImage;
        private Image flashImage;
        private Image vignetteImage;
        private Image tintImage;
        private RectTransform overlayHostRect;

        private Coroutine fadeRoutine;
        private Coroutine flashRoutine;
        private Coroutine vignetteRoutine;
        private Coroutine shakeRoutine;
        private Coroutine tintRoutine;
        private Coroutine pulseRoutine;
        private Coroutine glitchRoutine;

        private readonly Dictionary<string, GameObject> activeOverlays = new();

        public bool IsFading { get; private set; }

        public static CinematicEffectsService Bootstrap(Canvas canvas, RectTransform shakeRoot = null)
        {
            if (Instance != null)
                return Instance;

            if (canvas == null)
                Debug.LogWarning("[CinematicEffectsService] Bootstrap called without a host canvas — effects will no-op until SetHostCanvas() is called.");

            GameObject go = new GameObject("CinematicEffectsService");
            CinematicEffectsService svc = go.AddComponent<CinematicEffectsService>();
            svc.hostCanvas = canvas;
            svc.shakeRoot = shakeRoot;
            svc.EnsureUiBuilt();
            return svc;
        }

        private void Awake()
        {
            Instance = this;
            EnsureUiBuilt();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void EnsureUiBuilt()
        {
            if (hostCanvas == null)
            {
                Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (Canvas c in canvases)
                {
                    if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        hostCanvas = c;
                        break;
                    }
                }
            }

            if (hostCanvas == null)
                return;

            if (overlayHostRect == null)
            {
                GameObject host = new GameObject("CinematicOverlays", typeof(RectTransform));
                host.transform.SetParent(hostCanvas.transform, false);
                overlayHostRect = (RectTransform)host.transform;
                overlayHostRect.anchorMin = Vector2.zero;
                overlayHostRect.anchorMax = Vector2.one;
                overlayHostRect.offsetMin = Vector2.zero;
                overlayHostRect.offsetMax = Vector2.zero;
                overlayHostRect.SetAsLastSibling();
            }

            tintImage = EnsureFullScreenImage(tintImage, "TintOverlay", new Color(0f, 0f, 0f, 0f));
            vignetteImage = EnsureFullScreenImage(vignetteImage, "VignetteOverlay", new Color(0f, 0f, 0f, 0f));
            flashImage = EnsureFullScreenImage(flashImage, "FlashOverlay", new Color(1f, 1f, 1f, 0f));
            fadeImage = EnsureFullScreenImage(fadeImage, "FadeOverlay", new Color(0f, 0f, 0f, 0f));
        }

        private Image EnsureFullScreenImage(Image existing, string name, Color initialColor)
        {
            if (existing != null) return existing;

            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(overlayHostRect, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = initialColor;
            return img;
        }

        public void PlayTag(CinematicTagInfo tag)
        {
            if (tag == null) return;

            if (!GameSettings.ScreenEffectsEnabled && tag.Kind != CinematicEffectKind.Fade && tag.Kind != CinematicEffectKind.Stop)
                return;

            switch (tag.Kind)
            {
                case CinematicEffectKind.Fade: ApplyFade(tag); break;
                case CinematicEffectKind.Shake: ApplyShake(tag); break;
                case CinematicEffectKind.Flash: ApplyFlash(tag); break;
                case CinematicEffectKind.Vignette: ApplyVignette(tag); break;
                case CinematicEffectKind.Tint: ApplyTint(tag); break;
                case CinematicEffectKind.Pulse: ApplyPulse(tag); break;
                case CinematicEffectKind.Glitch: ApplyGlitch(tag); break;
                case CinematicEffectKind.Overlay: ApplyOverlay(tag); break;
                case CinematicEffectKind.Stop: StopAllEffects(); break;
            }
        }

        public void PlayTags(IList<CinematicTagInfo> tags)
        {
            if (tags == null) return;
            foreach (CinematicTagInfo tag in tags)
                PlayTag(tag);
        }

        public void StopAllEffects()
        {
            StopAllCoroutines();
            fadeRoutine = flashRoutine = vignetteRoutine = shakeRoutine = tintRoutine = pulseRoutine = glitchRoutine = null;

            if (fadeImage != null) fadeImage.color = new Color(0f, 0f, 0f, 0f);
            if (flashImage != null) flashImage.color = new Color(1f, 1f, 1f, 0f);
            if (vignetteImage != null) vignetteImage.color = new Color(0f, 0f, 0f, 0f);
            if (tintImage != null) tintImage.color = new Color(0f, 0f, 0f, 0f);
            if (shakeRoot != null) shakeRoot.anchoredPosition = Vector2.zero;

            foreach (var kv in activeOverlays)
            {
                if (kv.Value != null)
                    Destroy(kv.Value);
            }
            activeOverlays.Clear();
        }

        public void FadeOut(float duration, System.Action onComplete)
        {
            if (!GameSettings.SceneFadeEnabled || duration <= 0f)
            {
                onComplete?.Invoke();
                return;
            }

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(0f, 1f, duration, onComplete));
        }

        public void FadeIn(float duration)
        {
            if (!GameSettings.SceneFadeEnabled || duration <= 0f)
            {
                if (fadeImage != null) fadeImage.color = new Color(0f, 0f, 0f, 0f);
                return;
            }

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(1f, 0f, duration, null));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, System.Action onComplete)
        {
            IsFading = true;
            if (fadeImage == null) EnsureUiBuilt();

            fadeImage.color = new Color(0f, 0f, 0f, from);

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                fadeImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, k));
                yield return null;
            }

            fadeImage.color = new Color(0f, 0f, 0f, to);
            IsFading = false;
            fadeRoutine = null;
            onComplete?.Invoke();
        }

        private void ApplyFade(CinematicTagInfo tag)
        {
            string sub = tag.Sub.ToLowerInvariant();
            float duration = tag.NumberAt(1, 0.6f);

            if (sub == "in") FadeIn(duration);
            else if (sub == "out") FadeOut(duration, null);
            else if (sub == "flash") ApplyFlash(tag);
        }

        private void ApplyFlash(CinematicTagInfo tag)
        {
            Color color = tag.ColorAt(1, Color.white);
            float duration = tag.NumberAt(2, 0.35f);
            float peak = tag.NumberAt(3, 0.7f);

            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(color, duration, peak));
        }

        private IEnumerator FlashRoutine(Color color, float duration, float peakAlpha)
        {
            if (flashImage == null) EnsureUiBuilt();

            float half = duration * 0.5f;
            float t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                flashImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0f, peakAlpha, k));
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                flashImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(peakAlpha, 0f, k));
                yield return null;
            }

            flashImage.color = new Color(color.r, color.g, color.b, 0f);
            flashRoutine = null;
        }

        private void ApplyVignette(CinematicTagInfo tag)
        {
            Color color = tag.ColorAt(1, new Color(0f, 0f, 0f, 0.55f));
            float duration = tag.NumberAt(2, 0.6f);
            string sub = tag.Sub.ToLowerInvariant();
            float targetAlpha = sub == "off" ? 0f : Mathf.Clamp01(color.a <= 0f ? 0.5f : color.a);

            if (vignetteRoutine != null) StopCoroutine(vignetteRoutine);
            vignetteRoutine = StartCoroutine(VignetteRoutine(color, targetAlpha, duration));
        }

        private IEnumerator VignetteRoutine(Color color, float targetAlpha, float duration)
        {
            if (vignetteImage == null) EnsureUiBuilt();

            Color start = vignetteImage.color;
            Color target = new Color(color.r, color.g, color.b, targetAlpha);
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                vignetteImage.color = Color.Lerp(start, target, k);
                yield return null;
            }
            vignetteImage.color = target;
            vignetteRoutine = null;
        }

        private void ApplyTint(CinematicTagInfo tag)
        {
            string sub = tag.Sub.ToLowerInvariant();
            if (sub == "off")
            {
                if (tintRoutine != null) StopCoroutine(tintRoutine);
                tintRoutine = StartCoroutine(TintRoutine(new Color(0f, 0f, 0f, 0f), tag.NumberAt(1, 0.35f)));
                return;
            }

            Color color = tag.ColorAt(1, new Color(1f, 0f, 0f, 0.18f));
            float duration = tag.NumberAt(2, 0.35f);

            if (tintRoutine != null) StopCoroutine(tintRoutine);
            tintRoutine = StartCoroutine(TintRoutine(color, duration));
        }

        private IEnumerator TintRoutine(Color target, float duration)
        {
            if (tintImage == null) EnsureUiBuilt();

            Color start = tintImage.color;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                tintImage.color = Color.Lerp(start, target, k);
                yield return null;
            }
            tintImage.color = target;
            tintRoutine = null;
        }

        private void ApplyPulse(CinematicTagInfo tag)
        {
            Color color = tag.ColorAt(1, new Color(0.8f, 0.1f, 0.1f, 0.35f));
            float duration = tag.NumberAt(2, 1.2f);
            int pulses = Mathf.Max(1, Mathf.RoundToInt(tag.NumberAt(3, 3f)));

            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseRoutine(color, duration, pulses));
        }

        private IEnumerator PulseRoutine(Color color, float totalDuration, int pulses)
        {
            if (tintImage == null) EnsureUiBuilt();

            float perPulse = totalDuration / pulses;
            for (int i = 0; i < pulses; i++)
            {
                float half = perPulse * 0.5f;
                float t = 0f;
                while (t < half)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / half);
                    tintImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0f, color.a, k));
                    yield return null;
                }
                t = 0f;
                while (t < half)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / half);
                    tintImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0f, k));
                    yield return null;
                }
            }
            tintImage.color = new Color(color.r, color.g, color.b, 0f);
            pulseRoutine = null;
        }

        private void ApplyGlitch(CinematicTagInfo tag)
        {
            float duration = tag.NumberAt(1, 0.6f);
            float intensity = tag.NumberAt(2, 12f);

            if (glitchRoutine != null) StopCoroutine(glitchRoutine);
            glitchRoutine = StartCoroutine(GlitchRoutine(duration, intensity));
        }

        private IEnumerator GlitchRoutine(float duration, float intensity)
        {
            if (shakeRoot == null) yield break;

            Vector2 origin = shakeRoot.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float decay = 1f - Mathf.Clamp01(t / duration);
                float dx = Random.Range(-intensity, intensity) * decay;
                float dy = Random.Range(-intensity, intensity) * decay;
                shakeRoot.anchoredPosition = origin + new Vector2(dx, dy);
                yield return null;
            }
            shakeRoot.anchoredPosition = origin;
            glitchRoutine = null;
        }

        private void ApplyShake(CinematicTagInfo tag)
        {
            if (!GameSettings.ScreenShakeEnabled) return;
            float duration = tag.NumberAt(1, 0.35f);
            float intensity = tag.NumberAt(2, 18f);

            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeRoutine(duration, intensity));
        }

        private IEnumerator ShakeRoutine(float duration, float intensity)
        {
            if (shakeRoot == null) yield break;

            Vector2 origin = shakeRoot.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float decay = 1f - Mathf.Clamp01(t / duration);
                float dx = Random.Range(-intensity, intensity) * decay;
                float dy = Random.Range(-intensity, intensity) * decay;
                shakeRoot.anchoredPosition = origin + new Vector2(dx, dy);
                yield return null;
            }
            shakeRoot.anchoredPosition = origin;
            shakeRoutine = null;
        }

        private void ApplyOverlay(CinematicTagInfo tag)
        {
            string overlayName = tag.Sub.ToLowerInvariant();
            string action = tag.StringAt(1, "on").ToLowerInvariant();

            if (string.IsNullOrEmpty(overlayName))
                return;

            if (activeOverlays.TryGetValue(overlayName, out GameObject existing) && existing != null)
            {
                if (action == "off" || action == "stop")
                {
                    Destroy(existing);
                    activeOverlays.Remove(overlayName);
                }
                return;
            }

            if (action == "off" || action == "stop")
                return;

            GameObject overlay = OverlayFactory.CreateOverlay(overlayName, overlayHostRect, tag);
            if (overlay != null)
                activeOverlays[overlayName] = overlay;
        }

        public void SetShakeRoot(RectTransform root) => shakeRoot = root;
        public void SetHostCanvas(Canvas canvas) { hostCanvas = canvas; EnsureUiBuilt(); }
    }
}
