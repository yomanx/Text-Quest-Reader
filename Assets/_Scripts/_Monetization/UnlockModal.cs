using System;
using TextQuestReader.Cinematic.Procedural;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Monetization
{
    /// <summary>
    /// Lightweight code-built modal that asks the user to confirm a mock purchase.
    /// Spawned via UnlockModal.Show(); destroys itself when dismissed.
    /// </summary>
    public class UnlockModal : MonoBehaviour
    {
        private Action<bool> resultCallback;

        public static UnlockModal Show(Canvas parentCanvas, ProductDefinition product, Action<bool> resultCallback)
        {
            if (parentCanvas == null || product == null) return null;

            GameObject root = new GameObject("UnlockModal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(parentCanvas.transform, false);
            RectTransform rrt = (RectTransform)root.transform;
            rrt.anchorMin = Vector2.zero;
            rrt.anchorMax = Vector2.one;
            rrt.offsetMin = Vector2.zero;
            rrt.offsetMax = Vector2.zero;
            Image dim = root.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.65f);
            dim.raycastTarget = true;

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            RectTransform prt = (RectTransform)panel.transform;
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(560f, 360f);
            Image bg = panel.GetComponent<Image>();
            bg.sprite = ProceduralTextureFactory.CreateRoundedRectSprite(new Color(0.06f, 0.08f, 0.14f, 1f), 18, 96);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(1f, 1f, 1f, 0.98f);

            ProceduralSceneRenderer backdrop = ProceduralSceneRenderer.Attach(prt, siblingIndex: 0);
            backdrop.ShowPreset("hidden_lab", instant: true);

            GameObject frame = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frame.transform.SetParent(panel.transform, false);
            RectTransform frt = (RectTransform)frame.transform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = new Vector2(-4f, -4f);
            frt.offsetMax = new Vector2(4f, 4f);
            Image frameImg = frame.GetComponent<Image>();
            frameImg.sprite = ProceduralTextureFactory.CreateRoundedRectBorderSprite(Color.white, 2, 22, 96);
            frameImg.type = Image.Type.Sliced;
            frameImg.color = new Color(0.5f, 0.85f, 1f, 0.85f);
            frameImg.raycastTarget = false;

            CreateLabel(panel.transform, product.displayName ?? product.id, 24, FontStyles.Bold, new Vector2(0f, 0.78f), new Vector2(1f, 0.95f), TextAlignmentOptions.Center);
            CreateLabel(panel.transform, product.description ?? "Unlock premium quest content.", 16, FontStyles.Normal, new Vector2(0.05f, 0.34f), new Vector2(0.95f, 0.75f), TextAlignmentOptions.TopLeft);
            CreateLabel(panel.transform, product.price ?? "$0.99", 22, FontStyles.Bold, new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.32f), TextAlignmentOptions.Center);

            GameObject confirmBtn = CreateButton(panel.transform, "Mock Purchase", new Vector2(0.55f, 0.05f), new Vector2(0.95f, 0.16f), new Color(0.95f, 0.55f, 0.20f, 1f));
            GameObject cancelBtn = CreateButton(panel.transform, "Cancel", new Vector2(0.05f, 0.05f), new Vector2(0.45f, 0.16f), new Color(0.25f, 0.25f, 0.30f, 1f));

            UnlockModal modal = root.AddComponent<UnlockModal>();
            modal.resultCallback = resultCallback;

            confirmBtn.GetComponent<Button>().onClick.AddListener(() => modal.Close(true));
            cancelBtn.GetComponent<Button>().onClick.AddListener(() => modal.Close(false));

            return modal;
        }

        private void Close(bool confirmed)
        {
            resultCallback?.Invoke(confirmed);
            Destroy(gameObject);
        }

        private static GameObject CreateButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new GameObject(label + "Btn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image bg = go.GetComponent<Image>();
            bg.color = color;
            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = bg;

            GameObject labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            RectTransform lrt = tmp.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 18f;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            return go;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, FontStyles style, Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions align)
        {
            GameObject labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            RectTransform lrt = tmp.rectTransform;
            lrt.anchorMin = anchorMin;
            lrt.anchorMax = anchorMax;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = align;
            tmp.fontStyle = style;
#pragma warning disable CS0618
            tmp.enableWordWrapping = true;
#pragma warning restore CS0618
            return tmp;
        }
    }
}
