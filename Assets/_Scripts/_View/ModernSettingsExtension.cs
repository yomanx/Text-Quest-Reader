using TextQuestReader.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.View
{
    /// <summary>
    /// Code-built section attached at runtime to the SettingsPanel — adds master/
    /// music/sfx sliders, text-speed slider, and toggles for screen effects and
    /// screen shake. Persisted via GameSettings (PlayerPrefs).
    /// </summary>
    [RequireComponent(typeof(SettingsPanel))]
    public class ModernSettingsExtension : MonoBehaviour
    {
        private RectTransform sectionRoot;

        private Slider masterSlider;
        private Slider musicSlider;
        private Slider sfxSlider;
        private Slider textSpeedSlider;
        private Toggle effectsToggle;
        private Toggle shakeToggle;
        private Toggle fadeToggle;
        private Toggle typewriterSfxToggle;

        // Live read-out next to the text-speed slider so the user can see
        // whether they're at 0.5x / 1x / 2x without guessing from the knob.
        private TextMeshProUGUI textSpeedValueLabel;

        private void Start()
        {
            BuildIfNeeded();
            SyncFromSettings();
        }

        private void BuildIfNeeded()
        {
            if (sectionRoot != null) return;

            RectTransform host = (RectTransform)transform;

            GameObject sectionGo = new GameObject("ModernSettings", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sectionGo.transform.SetParent(host, false);
            sectionRoot = (RectTransform)sectionGo.transform;
            sectionRoot.anchorMin = new Vector2(0f, 0f);
            sectionRoot.anchorMax = new Vector2(1f, 0.45f);
            sectionRoot.offsetMin = new Vector2(20f, 20f);
            sectionRoot.offsetMax = new Vector2(-20f, -10f);
            Image bg = sectionGo.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.30f);

            CreateLabel("Master Volume", new Vector2(0.02f, 0.85f), new Vector2(0.30f, 1f));
            masterSlider = CreateSlider(new Vector2(0.32f, 0.86f), new Vector2(0.98f, 0.96f), GameSettings.MasterVolume, v => GameSettings.MasterVolume = v);

            CreateLabel("Music Volume", new Vector2(0.02f, 0.70f), new Vector2(0.30f, 0.85f));
            musicSlider = CreateSlider(new Vector2(0.32f, 0.71f), new Vector2(0.98f, 0.81f), GameSettings.MusicVolume, v => GameSettings.MusicVolume = v);

            CreateLabel("Sound Effects", new Vector2(0.02f, 0.55f), new Vector2(0.30f, 0.70f));
            sfxSlider = CreateSlider(new Vector2(0.32f, 0.56f), new Vector2(0.98f, 0.66f), GameSettings.SfxVolume, v => GameSettings.SfxVolume = v);

            CreateLabel("Text Speed", new Vector2(0.02f, 0.40f), new Vector2(0.30f, 0.55f));
            textSpeedSlider = CreateSlider(new Vector2(0.32f, 0.41f), new Vector2(0.88f, 0.51f), Mathf.InverseLerp(GameSettings.MinTextSpeed, GameSettings.MaxTextSpeed, GameSettings.TextSpeed),
                v =>
                {
                    GameSettings.TextSpeed = Mathf.Lerp(GameSettings.MinTextSpeed, GameSettings.MaxTextSpeed, v);
                    UpdateTextSpeedValueLabel();
                });

            // Live "1.0x" / "2.5x" read-out — slider was at 0.32..0.98 width
            // before; trimmed to 0.32..0.88 so the value label fits to the
            // right at 0.89..0.98 without overlapping the knob.
            textSpeedValueLabel = CreateLabel("1.0x", new Vector2(0.89f, 0.40f), new Vector2(0.98f, 0.55f));
            textSpeedValueLabel.alignment = TextAlignmentOptions.Right;
            textSpeedValueLabel.fontStyle = FontStyles.Bold;
            UpdateTextSpeedValueLabel();

            CreateLabel("Screen Effects", new Vector2(0.02f, 0.25f), new Vector2(0.30f, 0.40f));
            effectsToggle = CreateToggle(new Vector2(0.32f, 0.26f), new Vector2(0.55f, 0.36f), GameSettings.ScreenEffectsEnabled, v => GameSettings.ScreenEffectsEnabled = v);

            CreateLabel("Screen Shake", new Vector2(0.55f, 0.25f), new Vector2(0.78f, 0.40f));
            shakeToggle = CreateToggle(new Vector2(0.80f, 0.26f), new Vector2(0.98f, 0.36f), GameSettings.ScreenShakeEnabled, v => GameSettings.ScreenShakeEnabled = v);

            CreateLabel("Scene Fade", new Vector2(0.02f, 0.10f), new Vector2(0.30f, 0.25f));
            fadeToggle = CreateToggle(new Vector2(0.32f, 0.11f), new Vector2(0.55f, 0.21f), GameSettings.SceneFadeEnabled, v => GameSettings.SceneFadeEnabled = v);

            CreateLabel("Typewriter SFX", new Vector2(0.55f, 0.10f), new Vector2(0.78f, 0.25f));
            typewriterSfxToggle = CreateToggle(new Vector2(0.80f, 0.11f), new Vector2(0.98f, 0.21f), GameSettings.TypewriterSfxEnabled, v => GameSettings.TypewriterSfxEnabled = v);
        }

        private void SyncFromSettings()
        {
            if (masterSlider != null) masterSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
            if (textSpeedSlider != null) textSpeedSlider.SetValueWithoutNotify(Mathf.InverseLerp(GameSettings.MinTextSpeed, GameSettings.MaxTextSpeed, GameSettings.TextSpeed));
            if (effectsToggle != null) effectsToggle.SetIsOnWithoutNotify(GameSettings.ScreenEffectsEnabled);
            if (shakeToggle != null) shakeToggle.SetIsOnWithoutNotify(GameSettings.ScreenShakeEnabled);
            if (fadeToggle != null) fadeToggle.SetIsOnWithoutNotify(GameSettings.SceneFadeEnabled);
            if (typewriterSfxToggle != null) typewriterSfxToggle.SetIsOnWithoutNotify(GameSettings.TypewriterSfxEnabled);
            UpdateTextSpeedValueLabel();
        }

        private void UpdateTextSpeedValueLabel()
        {
            if (textSpeedValueLabel == null) return;
            float v = GameSettings.TextSpeed;
            // Format: 0.25x .. 4.0x with one decimal place.
            textSpeedValueLabel.text = v.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "x";
        }

        private TextMeshProUGUI CreateLabel(string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject("Label_" + text, typeof(RectTransform));
            go.transform.SetParent(sectionRoot, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            tmp.text = text;
            tmp.fontSize = 15f;
            tmp.color = new Color(0.9f, 0.9f, 0.92f, 1f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            return tmp;
        }

        private Slider CreateSlider(Vector2 anchorMin, Vector2 anchorMax, float initial, System.Action<float> onChange)
        {
            GameObject go = new GameObject("Slider", typeof(RectTransform));
            go.transform.SetParent(sectionRoot, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Slider slider = go.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            RectTransform bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = new Vector2(0f, 0.4f);
            bgRt.anchorMax = new Vector2(1f, 0.6f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.22f, 1f);

            GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            RectTransform faRt = (RectTransform)fillArea.transform;
            faRt.anchorMin = new Vector2(0f, 0.4f);
            faRt.anchorMax = new Vector2(1f, 0.6f);
            faRt.offsetMin = Vector2.zero;
            faRt.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fRt = (RectTransform)fill.transform;
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.offsetMin = Vector2.zero;
            fRt.offsetMax = Vector2.zero;
            Image fillImg = fill.GetComponent<Image>();
            fillImg.color = new Color(0.95f, 0.65f, 0.30f, 1f);
            slider.fillRect = fRt;

            GameObject handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            RectTransform haRt = (RectTransform)handleArea.transform;
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.offsetMin = Vector2.zero;
            haRt.offsetMax = Vector2.zero;

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform hRt = (RectTransform)handle.transform;
            hRt.sizeDelta = new Vector2(18f, 18f);
            Image handleImg = handle.GetComponent<Image>();
            handleImg.color = Color.white;
            slider.handleRect = hRt;
            slider.targetGraphic = handleImg;

            slider.SetValueWithoutNotify(Mathf.Clamp01(initial));
            slider.onValueChanged.AddListener(v => onChange?.Invoke(v));
            return slider;
        }

        private Toggle CreateToggle(Vector2 anchorMin, Vector2 anchorMax, bool initial, System.Action<bool> onChange)
        {
            GameObject go = new GameObject("Toggle", typeof(RectTransform));
            go.transform.SetParent(sectionRoot, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Toggle toggle = go.AddComponent<Toggle>();

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            RectTransform bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(0f, 0.5f);
            bgRt.pivot = new Vector2(0f, 0.5f);
            bgRt.sizeDelta = new Vector2(22f, 22f);
            bgRt.anchoredPosition = new Vector2(6f, 0f);
            Image bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0.18f, 0.18f, 0.22f, 1f);

            GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            RectTransform cRt = (RectTransform)check.transform;
            cRt.anchorMin = new Vector2(0.15f, 0.15f);
            cRt.anchorMax = new Vector2(0.85f, 0.85f);
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;
            Image cImg = check.GetComponent<Image>();
            cImg.color = new Color(0.95f, 0.85f, 0.45f, 1f);

            toggle.targetGraphic = bgImg;
            toggle.graphic = cImg;
            toggle.SetIsOnWithoutNotify(initial);
            toggle.onValueChanged.AddListener(v => onChange?.Invoke(v));
            return toggle;
        }
    }
}
