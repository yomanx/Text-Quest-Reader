using TextQuestReader.Cinematic.Procedural;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Fullscreen procedural background — sits behind every shell surface.
    /// Wraps a ProceduralSceneRenderer that crossfades between presets.
    /// </summary>
    public class CinematicBackgroundLayer : MonoBehaviour
    {
        private ProceduralSceneRenderer renderer_;
        private Image darkVeil;
        private RectTransform rect;

        public static CinematicBackgroundLayer Create(RectTransform shellRoot)
        {
            GameObject go = new GameObject("CinematicBackground", typeof(RectTransform));
            go.transform.SetParent(shellRoot, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.SetSiblingIndex(0);

            CinematicBackgroundLayer layer = go.AddComponent<CinematicBackgroundLayer>();
            layer.rect = rt;
            layer.Build();
            return layer;
        }

        private void Build()
        {
            renderer_ = ProceduralSceneRenderer.Attach(rect, siblingIndex: 0);
            renderer_.ShowPreset("deep_space", instant: true);

            GameObject veil = new GameObject("DarkVeil", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            veil.transform.SetParent(rect, false);
            RectTransform vrt = (RectTransform)veil.transform;
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero;
            vrt.offsetMax = Vector2.zero;
            darkVeil = veil.GetComponent<Image>();
            darkVeil.raycastTarget = false;
            darkVeil.color = new Color(0f, 0f, 0f, 0.25f);
            vrt.SetAsLastSibling();
        }

        public void ShowPreset(string id)
        {
            if (renderer_ != null) renderer_.ShowPreset(id);
        }

        public void SetVeilStrength(float alpha01)
        {
            if (darkVeil != null) darkVeil.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha01));
        }
    }
}
