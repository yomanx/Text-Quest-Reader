using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic.Procedural
{
    /// <summary>
    /// Mounts inside a target RectTransform and renders the active background
    /// preset as a stack of UI layers. Switching presets crossfades the old
    /// container out and the new one in.
    /// </summary>
    public class ProceduralSceneRenderer : MonoBehaviour
    {
        private RectTransform host;
        private RectTransform currentRoot;
        private RectTransform incomingRoot;
        private string currentPresetId;
        private Coroutine crossfadeRoutine;

        public string CurrentPresetId => currentPresetId;

        public static ProceduralSceneRenderer Attach(RectTransform host, int siblingIndex = 0)
        {
            if (host == null) return null;

            for (int i = 0; i < host.childCount; i++)
            {
                ProceduralSceneRenderer existing = host.GetChild(i).GetComponent<ProceduralSceneRenderer>();
                if (existing != null) return existing;
            }

            GameObject layer = new GameObject("ProceduralBackground", typeof(RectTransform));
            layer.transform.SetParent(host, false);
            RectTransform rt = (RectTransform)layer.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.SetSiblingIndex(siblingIndex);

            ProceduralSceneRenderer svc = layer.AddComponent<ProceduralSceneRenderer>();
            svc.host = (RectTransform)layer.transform;
            return svc;
        }

        public void ShowPreset(string presetId, bool instant = false)
        {
            if (host == null) host = (RectTransform)transform;
            if (!ProceduralBackgroundPresets.Has(presetId) && string.IsNullOrEmpty(presetId)) return;
            if (currentPresetId == presetId && currentRoot != null) return;

            BackgroundPreset preset = ProceduralBackgroundPresets.Get(presetId);
            currentPresetId = preset.Id;

            if (crossfadeRoutine != null)
            {
                StopCoroutine(crossfadeRoutine);
                if (currentRoot != null)
                {
                    Destroy(currentRoot.gameObject);
                    currentRoot = null;
                }
            }

            incomingRoot = BuildLayerStack(preset);
            RectTransform previous = currentRoot;
            currentRoot = incomingRoot;
            incomingRoot = null;
            crossfadeRoutine = StartCoroutine(CrossfadeRoutine(previous, currentRoot, instant ? 0f : 0.55f));
        }

        public void ClearAll()
        {
            if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
            if (currentRoot != null) Destroy(currentRoot.gameObject);
            currentRoot = null;
            currentPresetId = null;
        }

        private RectTransform BuildLayerStack(BackgroundPreset preset)
        {
            GameObject root = new GameObject("Preset_" + (preset.Id ?? "?"), typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            AddFullScreenLayer<GradientLayer>(rt, "Gradient", layer => layer.Apply(preset.TopColor, preset.BottomColor));

            if (preset.Stars && preset.StarCount > 0)
                AddFullScreenLayer<StarsLayer>(rt, "Stars", layer => layer.Apply(preset.StarColor, preset.StarCount, Mathf.Max(0.1f, preset.StarTwinkleSpeed)));

            if (preset.Grid && preset.GridSpacing > 4)
                AddFullScreenLayer<GridLayer>(rt, "Grid", layer => layer.Apply(preset.GridColor, preset.GridSpacing));

            if (preset.RadialGlow && preset.RadialGlowColor.a > 0f)
                AddCenteredLayer<RadialGlowLayer>(rt, "Glow", layer => layer.Apply(preset.RadialGlowColor, preset.RadialGlowRadius, preset.RadialGlowOffset));

            if (preset.LightBeams && preset.BeamCount > 0)
                AddFullScreenLayer<LightBeamsLayer>(rt, "Beams", layer => layer.Apply(preset.BeamColor, preset.BeamCount));

            if (preset.Fog && preset.FogColor.a > 0f)
                AddFogLayer(rt, preset.FogColor);

            if (preset.Sparkle && preset.SparkleColor.a > 0f)
                AddFullScreenLayer<SparkleLayer>(rt, "Sparkles", layer => layer.Apply(preset.SparkleColor, 28));

            if (preset.Scanlines && preset.ScanlineOpacity > 0f)
                AddFullScreenLayer<ScanlinesLayer>(rt, "Scanlines", layer => layer.Apply(preset.ScanlineOpacity));

            if (preset.Pulse && preset.PulseColor.a > 0f)
                AddFullScreenLayer<PulseTintLayer>(rt, "Pulse", layer => layer.Apply(preset.PulseColor, preset.PulsePeriod));

            if (preset.Vignette && preset.VignetteColor.a > 0f)
                AddFullScreenLayer<VignetteLayer>(rt, "Vignette", layer => layer.Apply(preset.VignetteColor));

            if (!string.IsNullOrEmpty(preset.Label))
                AddLabel(rt, preset.Label);

            return rt;
        }

        private void AddFullScreenLayer<T>(RectTransform parent, string name, System.Action<T> applyAction) where T : MonoBehaviour
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            T layer = go.AddComponent<T>();
            applyAction?.Invoke(layer);
        }

        private void AddCenteredLayer<T>(RectTransform parent, string name, System.Action<T> applyAction) where T : MonoBehaviour
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            T layer = go.AddComponent<T>();
            applyAction?.Invoke(layer);
        }

        private void AddFogLayer(RectTransform parent, Color color)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject layer = new GameObject("Fog" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                layer.transform.SetParent(parent, false);
                RectTransform rt = (RectTransform)layer.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                Image img = layer.GetComponent<Image>();
                img.raycastTarget = false;
                img.sprite = ProceduralTextureFactory.CreateNoiseSprite(0.04f + i * 0.02f, new Color(color.r, color.g, color.b, color.a * (0.4f + i * 0.3f)), 128);
                img.color = Color.white;
                img.type = Image.Type.Tiled;
                img.preserveAspect = false;
                img.pixelsPerUnitMultiplier = 1f;
            }
        }

        private void AddLabel(RectTransform parent, string text)
        {
            GameObject labelGo = new GameObject("SceneLabel", typeof(RectTransform));
            labelGo.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(20f, 8f);
            rt.offsetMax = new Vector2(-20f, 40f);
            tmp.text = "// " + text;
            tmp.fontSize = 12f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = new Color(0.5f, 0.85f, 1f, 0.55f);
            tmp.characterSpacing = 4f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;
        }

        private IEnumerator CrossfadeRoutine(RectTransform from, RectTransform to, float duration)
        {
            CanvasGroup fromGroup = from != null ? from.GetComponent<CanvasGroup>() : null;
            CanvasGroup toGroup = to != null ? to.GetComponent<CanvasGroup>() : null;

            if (duration <= 0f)
            {
                if (toGroup != null) toGroup.alpha = 1f;
                if (from != null) Destroy(from.gameObject);
                crossfadeRoutine = null;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (fromGroup != null) fromGroup.alpha = 1f - k;
                if (toGroup != null) toGroup.alpha = k;
                yield return null;
            }

            if (toGroup != null) toGroup.alpha = 1f;
            if (from != null) Destroy(from.gameObject);
            crossfadeRoutine = null;
        }
    }
}
