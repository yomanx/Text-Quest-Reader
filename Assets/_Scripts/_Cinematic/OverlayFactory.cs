using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic
{
    /// <summary>
    /// Runtime factory that produces simple atmospheric overlays without relying
    /// on any prefab in the project. Each overlay is a UI GameObject that animates
    /// itself via a dedicated MonoBehaviour. Designed to be safe and lightweight.
    /// </summary>
    public static class OverlayFactory
    {
        public static GameObject CreateOverlay(string overlayName, RectTransform parent, CinematicTagInfo tag)
        {
            if (parent == null || string.IsNullOrEmpty(overlayName))
                return null;

            GameObject go = new GameObject("Overlay_" + overlayName, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            switch (overlayName)
            {
                case "fog":
                    AddFog(go, tag); break;
                case "rain":
                    AddParticleField(go, tag, new Color(0.55f, 0.7f, 0.9f, 0.4f), velocity: new Vector2(-40f, -420f), count: 80, sizeRange: new Vector2(2f, 4f), shape: ParticleField.ShapeKind.LineVertical); break;
                case "snow":
                    AddParticleField(go, tag, new Color(1f, 1f, 1f, 0.85f), velocity: new Vector2(5f, -55f), count: 60, sizeRange: new Vector2(4f, 7f), shape: ParticleField.ShapeKind.Dot); break;
                case "sparks":
                    AddParticleField(go, tag, new Color(1f, 0.7f, 0.2f, 0.95f), velocity: new Vector2(0f, 180f), count: 40, sizeRange: new Vector2(2f, 4f), shape: ParticleField.ShapeKind.Dot); break;
                case "dust":
                    AddParticleField(go, tag, new Color(0.9f, 0.85f, 0.7f, 0.35f), velocity: new Vector2(8f, 4f), count: 50, sizeRange: new Vector2(3f, 6f), shape: ParticleField.ShapeKind.Dot); break;
                case "stars":
                    AddParticleField(go, tag, new Color(0.9f, 0.95f, 1f, 0.85f), velocity: Vector2.zero, count: 80, sizeRange: new Vector2(1.5f, 3f), shape: ParticleField.ShapeKind.Twinkle); break;
                case "shimmer":
                case "magic":
                    AddParticleField(go, tag, new Color(0.7f, 0.5f, 1f, 0.7f), velocity: new Vector2(0f, 30f), count: 50, sizeRange: new Vector2(3f, 6f), shape: ParticleField.ShapeKind.Twinkle); break;
                case "blood":
                case "danger":
                    AddPulsingTint(go, new Color(0.7f, 0.05f, 0.05f, 0.25f), period: 1.4f); break;
                case "scanlines":
                    AddScanlines(go); break;
                default:
                    AddPulsingTint(go, new Color(0.5f, 0.5f, 0.7f, 0.2f), period: 2.2f); break;
            }

            return go;
        }

        private static void AddFog(GameObject host, CinematicTagInfo tag)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject layer = new GameObject("FogLayer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                layer.transform.SetParent(host.transform, false);
                RectTransform rt = (RectTransform)layer.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Image img = layer.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = new Color(0.4f, 0.45f, 0.5f, 0.10f + i * 0.04f);

                FogLayerMover mover = layer.AddComponent<FogLayerMover>();
                mover.Init(speed: 6f + i * 3f);
            }
        }

        private static void AddPulsingTint(GameObject host, Color color, float period)
        {
            GameObject layer = new GameObject("TintLayer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            layer.transform.SetParent(host.transform, false);
            RectTransform rt = (RectTransform)layer.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = layer.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = color;

            PulsingTint pulser = layer.AddComponent<PulsingTint>();
            pulser.Init(color, period);
        }

        private static void AddScanlines(GameObject host)
        {
            for (int i = 0; i < 40; i++)
            {
                GameObject line = new GameObject("ScanLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                line.transform.SetParent(host.transform, false);
                RectTransform rt = (RectTransform)line.transform;
                rt.anchorMin = new Vector2(0f, (float)i / 40f);
                rt.anchorMax = new Vector2(1f, (float)i / 40f);
                rt.sizeDelta = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                Image img = line.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = new Color(0f, 0f, 0f, 0.10f);
            }
        }

        private static void AddParticleField(GameObject host, CinematicTagInfo tag, Color baseColor, Vector2 velocity, int count, Vector2 sizeRange, ParticleField.ShapeKind shape)
        {
            ParticleField field = host.AddComponent<ParticleField>();
            field.Init(baseColor, velocity, count, sizeRange, shape);
        }
    }
}
