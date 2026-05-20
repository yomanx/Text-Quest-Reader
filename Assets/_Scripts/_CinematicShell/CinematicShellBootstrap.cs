using System.Collections;
using TextQuestReader.Cinematic.Procedural;
using UnityEngine;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Auto-attached root of the new cinematic shell. Wakes after scene load,
    /// finds the GamePanel + its canvas, hides the legacy UI, and constructs
    /// a fullscreen sci-fi interface — fullscreen procedural background,
    /// premium catalog, cinematic reader, big HUD, big choice cards, result
    /// screen — entirely from code. No prefab/scene edits required.
    /// </summary>
    public class CinematicShellBootstrap : MonoBehaviour
    {
        private static CinematicShellBootstrap instance;

        // Auto-init disabled by user request — the runtime-built shell was
        // replacing the original UI with bare procedural surfaces, which felt
        // worse than the original prefab UI. The shell code is kept in the
        // project so it can be re-enabled (or evolved) later, but it no
        // longer activates on Play. The original GamePanel UI stays visible.
        // To re-enable, restore the [RuntimeInitializeOnLoadMethod] attribute.
        private static void AutoInit_Disabled()
        {
            if (instance != null) return;
            GameObject host = new GameObject("CinematicShell");
            host.AddComponent<CinematicShellBootstrap>();
            DontDestroyOnLoad(host);
        }

        private GamePanel gamePanel;
        private Canvas hostCanvas;
        private RectTransform hostCanvasRect;
        private RectTransform shellRoot;

        private CinematicBackgroundLayer backgroundLayer;
        private CinematicCatalogScreen catalogScreen;
        private CinematicReaderScreen readerScreen;
        private LegacyUiSuppressor legacySuppressor;

        public RectTransform ShellRoot => shellRoot;
        public Canvas HostCanvas => hostCanvas;
        public CinematicBackgroundLayer Background => backgroundLayer;
        public CinematicCatalogScreen Catalog => catalogScreen;
        public CinematicReaderScreen Reader => readerScreen;

        private void Awake()
        {
            instance = this;
            StartCoroutine(InitializeAfterGamePanel());
        }

        private IEnumerator InitializeAfterGamePanel()
        {
            float deadline = Time.realtimeSinceStartup + 5f;
            while (GamePanel.Instance == null && Time.realtimeSinceStartup < deadline)
                yield return null;

            gamePanel = GamePanel.Instance;
            if (gamePanel == null)
            {
                Debug.LogWarning("[CinematicShell] No GamePanel found within 5s. Aborting.");
                yield break;
            }

            hostCanvas = FindCanvasFor(gamePanel);
            if (hostCanvas == null)
            {
                Debug.LogWarning("[CinematicShell] No host Canvas found. Aborting.");
                yield break;
            }
            hostCanvasRect = hostCanvas.GetComponent<RectTransform>();

            BuildShellRoot();
            BuildLayers();

            catalogScreen.Hide(instant: true);
            readerScreen.Hide(instant: true);

            HookEvents();

            RaiseCinematicOverlaysAboveShell();

            yield return null;

            if (gamePanel.HasActivePlayer && gamePanel.Player != null && gamePanel.Player.quest != null)
            {
                Location current = gamePanel.Player.quest.FindLocationWith(gamePanel.Player.locationID);
                if (current != null) OnLocationShown(current);
            }
            else
            {
                gamePanel.UpdateLocalQuests();
            }
        }

        private void RaiseCinematicOverlaysAboveShell()
        {
            if (hostCanvas == null) return;
            Transform overlays = hostCanvas.transform.Find("CinematicOverlays");
            if (overlays != null) overlays.SetAsLastSibling();
        }

        private Canvas FindCanvasFor(GamePanel panel)
        {
            Canvas canvas = panel.GetComponentInParent<Canvas>();
            if (canvas != null) return canvas;
            return FindAnyObjectByType<Canvas>();
        }

        private void BuildShellRoot()
        {
            GameObject root = new GameObject("CinematicShellRoot", typeof(RectTransform));
            root.transform.SetParent(hostCanvas.transform, false);
            shellRoot = (RectTransform)root.transform;
            shellRoot.anchorMin = Vector2.zero;
            shellRoot.anchorMax = Vector2.one;
            shellRoot.offsetMin = Vector2.zero;
            shellRoot.offsetMax = Vector2.zero;
            shellRoot.SetAsLastSibling();

            legacySuppressor = root.AddComponent<LegacyUiSuppressor>();
            legacySuppressor.Suppress(hostCanvas, gamePanel);
        }

        private void BuildLayers()
        {
            backgroundLayer = CinematicBackgroundLayer.Create(shellRoot);
            catalogScreen = CinematicCatalogScreen.Create(shellRoot, gamePanel, backgroundLayer);
            readerScreen = CinematicReaderScreen.Create(shellRoot, gamePanel, backgroundLayer);
        }

        private void HookEvents()
        {
            gamePanel.CatalogReady += OnCatalogReady;
            gamePanel.QuestPreviewSelected += OnQuestPreviewSelected;
            gamePanel.LocationShown += OnLocationShown;
            gamePanel.MainTextRendered += OnMainTextRendered;
            gamePanel.ChoicesReady += OnChoicesReady;
            gamePanel.SinglePassageReady += OnSinglePassageReady;
            gamePanel.QuestEnded += OnQuestEnded;
            gamePanel.StartQuestEnded += OnStartQuestEnded;
            gamePanel.LocationImageChanged += OnLocationImageChanged;
        }

        private void OnDestroy()
        {
            if (gamePanel != null)
            {
                gamePanel.CatalogReady -= OnCatalogReady;
                gamePanel.QuestPreviewSelected -= OnQuestPreviewSelected;
                gamePanel.LocationShown -= OnLocationShown;
                gamePanel.MainTextRendered -= OnMainTextRendered;
                gamePanel.ChoicesReady -= OnChoicesReady;
                gamePanel.SinglePassageReady -= OnSinglePassageReady;
                gamePanel.QuestEnded -= OnQuestEnded;
                gamePanel.StartQuestEnded -= OnStartQuestEnded;
                gamePanel.LocationImageChanged -= OnLocationImageChanged;
            }
            if (instance == this) instance = null;
        }

        private void OnLocationImageChanged(string imageName)
        {
            if (readerScreen == null) return;
            readerScreen.ApplyRealImage(imageName);
        }

        private void OnCatalogReady(System.Collections.Generic.List<TextQuestReader.Monetization.CatalogEntry> entries, GamePanel.Source source, string preselect)
        {
            if (catalogScreen == null) return;
            if (readerScreen != null) readerScreen.Hide();
            catalogScreen.Show(entries, source, preselect);
            if (backgroundLayer != null) backgroundLayer.ShowPreset("terminal_room");
        }

        private void OnQuestPreviewSelected(QuestShort quest, bool isRemote)
        {
            if (catalogScreen == null || quest == null) return;
            catalogScreen.UpdateSelection(quest);
            string presetKey = ResolvePresetForQuest(quest);
            backgroundLayer.ShowPreset(presetKey);
        }

        private string ResolvePresetForQuest(QuestShort quest)
        {
            if (string.IsNullOrEmpty(quest.QuestName)) return "terminal_room";
            string n = quest.QuestName.ToLowerInvariant();
            if (n.Contains("asteroid") || n.Contains("neon")) return "observation_deck";
            if (n.Contains("space")) return "deep_space";
            return "terminal_room";
        }

        private void OnLocationShown(Location location)
        {
            if (readerScreen == null) return;
            if (catalogScreen != null) catalogScreen.Hide();
            readerScreen.Show();
            readerScreen.ApplyLocation(location);
        }

        private void OnMainTextRendered(string text)
        {
            if (readerScreen == null) return;
            readerScreen.SetMainText(text);
        }

        private void OnChoicesReady(System.Collections.Generic.List<PassageInfo> choices)
        {
            if (readerScreen == null) return;
            readerScreen.SetChoices(choices);
        }

        private void OnSinglePassageReady(Passage next)
        {
            if (readerScreen == null) return;
            readerScreen.SetSingleNext(next);
        }

        private void OnQuestEnded(bool victory)
        {
            if (readerScreen == null) return;
            readerScreen.PrepareForEnding(victory);
        }

        private void OnStartQuestEnded()
        {
        }
    }
}
