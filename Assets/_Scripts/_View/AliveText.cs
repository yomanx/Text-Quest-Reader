using System;
using System.Collections;
using TextQuestReader.Settings;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class AliveText : MonoBehaviour
{
    private TMP_Text text;
    public TMP_Text Text => text;

    private string startText;

    private const float minCharsPerSecond = 80f;
    private const float maxCharsPerSecond = 700f;
    private const int shortTextLength = 40;
    private const int longTextLength = 500;

    [SerializeField] private bool emitTypewriterSfx = false;
    [SerializeField] private AudioClip typewriterClip;
    [SerializeField, Range(0f, 1f)] private float typewriterClipVolume = 0.15f;
    [SerializeField] private int sfxEveryNChars = 2;
    [SerializeField] private bool showSkipAffordance = true;

    private bool isTyping;
    private bool finishRequested;
    private string currentValue = string.Empty;

    public event Action TypingStarted;
    public event Action TypingFinished;

    public bool IsTyping => isTyping;
    public string CurrentValue => currentValue;

    // Runtime "tap / space / click to skip" hint. Lazily built as a child of
    // this component's GameObject; pinned to the bottom-right corner with
    // LayoutElement.ignoreLayout so parent layout groups don't account for it.
    private TextMeshProUGUI skipAffordanceLabel;
    private Coroutine affordanceRoutine;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        startText = text.text;
    }

    private void OnEnable()
    {
        text.text = startText;
    }

    public void SetText(string value)
    {
        currentValue = value ?? string.Empty;
        finishRequested = false;
        StopAllCoroutines();
        StartCoroutine(ShowText(currentValue));
    }

    public void FinishImmediately()
    {
        if (!isTyping)
            return;

        finishRequested = true;
    }

    public void ClearImmediate()
    {
        StopAllCoroutines();
        isTyping = false;
        finishRequested = false;
        currentValue = string.Empty;
        text.text = string.Empty;
        text.maxVisibleCharacters = 0;
        HideAffordance();
    }

    private IEnumerator ShowText(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            text.text = string.Empty;
            text.maxVisibleCharacters = 0;
            HideAffordance();
            yield break;
        }

        isTyping = true;
        TypingStarted?.Invoke();

        text.text = value;
        text.maxVisibleCharacters = 0;

        float speed = Mathf.Clamp(GameSettings.TextSpeed, GameSettings.MinTextSpeed, GameSettings.MaxTextSpeed);
        float charsPerSecond = GetCharsPerSecond(value.Length) * speed;

        // Punctuation-aware pauses, scaled inversely to the user's text speed:
        // a fast reader gets micro-pauses, a slow reader gets real beats.
        float pauseAfterSentence = 0.30f / Mathf.Max(speed, 0.25f);
        float pauseAfterClause   = 0.12f / Mathf.Max(speed, 0.25f);

        ShowAffordance();

        float timer = 0f;
        float pendingPause = 0f;
        int charIndex = 0;
        int lastSfxIndex = 0;
        int prevCharIndex = 0;

        while (charIndex < value.Length)
        {
            if (finishRequested)
            {
                charIndex = value.Length;
                break;
            }

            // Honour any punctuation-driven pause without stalling Update or
            // adding a non-cancellable WaitForSeconds.
            if (pendingPause > 0f)
            {
                pendingPause -= Time.deltaTime;
                yield return null;
                continue;
            }

            timer += Time.deltaTime;
            int charsToShow = Mathf.FloorToInt(timer * charsPerSecond);

            if (charsToShow > 0)
            {
                timer -= charsToShow / charsPerSecond;
                prevCharIndex = charIndex;
                charIndex = Mathf.Min(value.Length, charIndex + charsToShow);

                text.maxVisibleCharacters = charIndex;

                // Look at the characters we just revealed (prevCharIndex..charIndex).
                // The strongest punctuation in that span wins.
                float pauseSet = 0f;
                for (int i = prevCharIndex; i < charIndex; i++)
                {
                    char c = value[i];
                    if (c == '.' || c == '!' || c == '?')
                    {
                        pauseSet = Mathf.Max(pauseSet, pauseAfterSentence);
                    }
                    else if (c == ',' || c == ';' || c == ':' || c == '—' || c == '–')
                    {
                        pauseSet = Mathf.Max(pauseSet, pauseAfterClause);
                    }
                }
                if (pauseSet > 0f && charIndex < value.Length)
                    pendingPause = pauseSet;

                if (emitTypewriterSfx && GameSettings.TypewriterSfxEnabled && typewriterClip != null && AudioManager.Instance != null)
                {
                    if (charIndex - lastSfxIndex >= Mathf.Max(1, sfxEveryNChars) && !char.IsWhiteSpace(value[Mathf.Max(0, charIndex - 1)]))
                    {
                        AudioManager.Instance.PlaySfxClip(typewriterClip, typewriterClipVolume);
                        lastSfxIndex = charIndex;
                    }
                }
            }

            yield return null;
        }

        text.maxVisibleCharacters = value.Length;
        finishRequested = false;
        isTyping = false;
        HideAffordance();
        // TypingFinished fires exactly once per ShowText invocation, both on
        // natural completion and on skip-to-end.
        TypingFinished?.Invoke();
    }

    private float GetCharsPerSecond(int textLength)
    {
        float t = Mathf.InverseLerp(shortTextLength, longTextLength, textLength);
        return Mathf.Lerp(minCharsPerSecond, maxCharsPerSecond, t);
    }

    private void EnsureAffordance()
    {
        if (!showSkipAffordance) return;
        if (skipAffordanceLabel != null) return;

        GameObject go = new GameObject("SkipAffordance", typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(transform, false);

        LayoutElement le = go.GetComponent<LayoutElement>();
        le.ignoreLayout = true;   // parent VerticalLayoutGroup / SizeFitter won't account for it

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-4f, 6f);
        rt.sizeDelta = new Vector2(160f, 18f);

        skipAffordanceLabel = go.AddComponent<TextMeshProUGUI>();
        skipAffordanceLabel.text = "▸  Space / Enter to skip";
        skipAffordanceLabel.fontSize = 11f;
        skipAffordanceLabel.alignment = TextAlignmentOptions.MidlineRight;
        skipAffordanceLabel.color = new Color(0.55f, 0.80f, 0.95f, 0f);
        skipAffordanceLabel.characterSpacing = 3f;
        skipAffordanceLabel.raycastTarget = false;
    }

    private void ShowAffordance()
    {
        EnsureAffordance();
        if (skipAffordanceLabel == null) return;
        if (affordanceRoutine != null) StopCoroutine(affordanceRoutine);
        affordanceRoutine = StartCoroutine(AffordanceBreathe());
    }

    private void HideAffordance()
    {
        if (skipAffordanceLabel == null) return;
        if (affordanceRoutine != null) StopCoroutine(affordanceRoutine);
        affordanceRoutine = StartCoroutine(AffordanceFadeOut());
    }

    private IEnumerator AffordanceBreathe()
    {
        // 0.45s fade-in delay so a tiny one-line text doesn't blip the hint;
        // it only shows up if the typewriter has been running long enough
        // that "you may skip" is actually useful information.
        float t = 0f;
        while (t < 0.45f)
        {
            t += Time.deltaTime;
            if (!isTyping) yield break;
            yield return null;
        }

        Color baseColor = skipAffordanceLabel.color;
        float phase = 0f;
        while (isTyping)
        {
            phase += Time.deltaTime * 2.2f;
            float a = 0.55f + 0.20f * Mathf.Sin(phase);
            skipAffordanceLabel.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }
    }

    private IEnumerator AffordanceFadeOut()
    {
        Color baseColor = skipAffordanceLabel.color;
        float startAlpha = baseColor.a;
        float t = 0f;
        const float dur = 0.25f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            skipAffordanceLabel.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(startAlpha, 0f, k));
            yield return null;
        }
        skipAffordanceLabel.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        affordanceRoutine = null;
    }
}
