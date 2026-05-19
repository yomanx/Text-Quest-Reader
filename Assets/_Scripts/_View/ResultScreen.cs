using System;
using System.Collections;
using System.Collections.Generic;
using TextQuestReader.Cinematic;
using TextQuestReader.Cinematic.Procedural;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.View
{
    /// <summary>
    /// Cinematic end-of-quest overlay. Spawns from code (no prefab needed),
    /// displays VICTORY or DEFEAT, optional final stats, "Return to menu" button.
    /// Wires itself to GamePanel.QuestEnded automatically when bootstrapped.
    /// </summary>
    public class ResultScreen : MonoBehaviour
    {
        private static ResultScreen instance;

        public static void Bootstrap(GamePanel gamePanel, Canvas hostCanvas)
        {
            if (instance != null) return;
            if (gamePanel == null || hostCanvas == null) return;

            GameObject go = new GameObject("ResultScreenBridge");
            ResultScreen bridge = go.AddComponent<ResultScreen>();
            bridge.gamePanel = gamePanel;
            bridge.hostCanvas = hostCanvas;
            gamePanel.QuestEnded += bridge.OnQuestEnded;
            instance = bridge;
        }

        private GamePanel gamePanel;
        private Canvas hostCanvas;
        private GameObject currentOverlay;

        private void OnDestroy()
        {
            if (gamePanel != null)
                gamePanel.QuestEnded -= OnQuestEnded;
            if (instance == this) instance = null;
        }

        private void OnQuestEnded(bool victory)
        {
            StartCoroutine(ShowDelayed(victory));
        }

        private IEnumerator ShowDelayed(bool victory)
        {
            yield return new WaitForSeconds(1.5f);

            if (gamePanel == null || gamePanel.Player == null) yield break;

            Show(victory, gamePanel.Player.quest);
        }

        public void Show(bool victory, Quest quest)
        {
            if (currentOverlay != null) Destroy(currentOverlay);
            if (hostCanvas == null) return;

            currentOverlay = BuildOverlay(victory, quest);
            StartCoroutine(AnimateIn(currentOverlay));

            if (CinematicEffectsService.Instance != null)
            {
                CinematicTagInfo flash = new CinematicTagInfo
                {
                    Kind = CinematicEffectKind.Flash,
                    Args = new List<string>(new[] { "flash", victory ? "FFEEAA" : "FF3333", "0.6", "0.55" })
                };
                CinematicEffectsService.Instance.PlayTag(flash);
            }
        }

        private GameObject BuildOverlay(bool victory, Quest quest)
        {
            GameObject root = new GameObject("ResultOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(hostCanvas.transform, false);
            RectTransform rrt = (RectTransform)root.transform;
            rrt.anchorMin = Vector2.zero;
            rrt.anchorMax = Vector2.one;
            rrt.offsetMin = Vector2.zero;
            rrt.offsetMax = Vector2.zero;
            Image bg = root.GetComponent<Image>();
            bg.color = victory ? new Color(0.02f, 0.03f, 0.07f, 0.92f) : new Color(0.07f, 0.01f, 0.02f, 0.94f);

            ProceduralSceneRenderer backdrop = ProceduralSceneRenderer.Attach(rrt, siblingIndex: 0);
            backdrop.ShowPreset(victory ? "victory_scene" : "failure_scene", instant: true);

            GameObject titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI title = titleGo.AddComponent<TextMeshProUGUI>();
            RectTransform trt = title.rectTransform;
            trt.anchorMin = new Vector2(0f, 0.55f);
            trt.anchorMax = new Vector2(1f, 0.78f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            title.text = victory ? "VICTORY" : "FAILURE";
            title.fontSize = 96f;
            title.alignment = TextAlignmentOptions.Center;
            title.color = victory ? new Color(0.95f, 0.85f, 0.40f, 1f) : new Color(0.95f, 0.30f, 0.30f, 1f);
            title.fontStyle = FontStyles.Bold;

            GameObject subGo = new GameObject("Subtitle", typeof(RectTransform));
            subGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI sub = subGo.AddComponent<TextMeshProUGUI>();
            RectTransform srt = sub.rectTransform;
            srt.anchorMin = new Vector2(0.1f, 0.40f);
            srt.anchorMax = new Vector2(0.9f, 0.55f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            sub.text = victory
                ? "The path is yours.\nYou have written this story to its end."
                : "The story does not end here for everyone.\nReturn, choose differently, survive.";
            sub.fontSize = 22f;
            sub.alignment = TextAlignmentOptions.Center;
            sub.color = new Color(0.85f, 0.85f, 0.90f, 0.95f);
#pragma warning disable CS0618
            sub.enableWordWrapping = true;
#pragma warning restore CS0618

            GameObject statsGo = new GameObject("Stats", typeof(RectTransform));
            statsGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI stats = statsGo.AddComponent<TextMeshProUGUI>();
            RectTransform strt = stats.rectTransform;
            strt.anchorMin = new Vector2(0.15f, 0.22f);
            strt.anchorMax = new Vector2(0.85f, 0.38f);
            strt.offsetMin = Vector2.zero;
            strt.offsetMax = Vector2.zero;
            stats.text = BuildStatsLine(quest);
            stats.fontSize = 16f;
            stats.alignment = TextAlignmentOptions.Center;
            stats.color = new Color(0.7f, 0.85f, 1f, 0.85f);
#pragma warning disable CS0618
            stats.enableWordWrapping = true;
#pragma warning restore CS0618

            GameObject btnGo = new GameObject("ContinueBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(root.transform, false);
            RectTransform brt = (RectTransform)btnGo.transform;
            brt.anchorMin = new Vector2(0.5f, 0.10f);
            brt.anchorMax = new Vector2(0.5f, 0.10f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(240f, 56f);
            Image btnImage = btnGo.GetComponent<Image>();
            btnImage.color = victory ? new Color(0.20f, 0.55f, 0.30f, 1f) : new Color(0.55f, 0.20f, 0.20f, 1f);
            Button btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImage;
            btn.onClick.AddListener(OnContinueClicked);

            GameObject btnLabelGo = new GameObject("Label", typeof(RectTransform));
            btnLabelGo.transform.SetParent(btnGo.transform, false);
            TextMeshProUGUI btnLabel = btnLabelGo.AddComponent<TextMeshProUGUI>();
            RectTransform lrt = btnLabel.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            btnLabel.text = "Return to library";
            btnLabel.alignment = TextAlignmentOptions.Center;
            btnLabel.fontSize = 18f;
            btnLabel.color = Color.white;
            btnLabel.fontStyle = FontStyles.Bold;

            return root;
        }

        private string BuildStatsLine(Quest quest)
        {
            if (quest == null || quest.parameters == null) return string.Empty;

            List<string> parts = new List<string>();
            foreach (Parameter p in quest.parameters)
            {
                if (!p.isActive) continue;
                if (p.isHidden) continue;
                parts.Add($"{p.workingName}: {p.value}");
            }

            return string.Join("    ·    ", parts);
        }

        private IEnumerator AnimateIn(GameObject overlay)
        {
            if (overlay == null) yield break;
            CanvasGroup group = overlay.GetComponent<CanvasGroup>();
            if (group == null) group = overlay.AddComponent<CanvasGroup>();

            group.alpha = 0f;
            float t = 0f;
            while (t < 0.7f)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Clamp01(t / 0.7f);
                yield return null;
            }
            group.alpha = 1f;
        }

        private void OnContinueClicked()
        {
            if (currentOverlay != null) Destroy(currentOverlay);

            if (gamePanel != null)
                gamePanel.AbandonQuest();
        }
    }
}
