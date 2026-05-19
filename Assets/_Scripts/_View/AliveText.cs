using System;
using System.Collections;
using TextQuestReader.Settings;
using UnityEngine;
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

    private bool isTyping;
    private bool finishRequested;
    private string currentValue = string.Empty;

    public event Action TypingStarted;
    public event Action TypingFinished;

    public bool IsTyping => isTyping;
    public string CurrentValue => currentValue;

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
    }

    private IEnumerator ShowText(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            text.text = string.Empty;
            text.maxVisibleCharacters = 0;
            yield break;
        }

        isTyping = true;
        TypingStarted?.Invoke();

        text.text = value;
        text.maxVisibleCharacters = 0;

        float charsPerSecond = GetCharsPerSecond(value.Length) * Mathf.Clamp(GameSettings.TextSpeed, GameSettings.MinTextSpeed, GameSettings.MaxTextSpeed);

        float timer = 0f;
        int charIndex = 0;
        int lastSfxIndex = 0;

        while (charIndex < value.Length)
        {
            if (finishRequested)
            {
                charIndex = value.Length;
                break;
            }

            timer += Time.deltaTime;

            int charsToShow = Mathf.FloorToInt(timer * charsPerSecond);

            if (charsToShow > 0)
            {
                timer -= charsToShow / charsPerSecond;

                charIndex += charsToShow;
                charIndex = Mathf.Min(charIndex, value.Length);

                text.maxVisibleCharacters = charIndex;

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
        TypingFinished?.Invoke();
    }

    private float GetCharsPerSecond(int textLength)
    {
        float t = Mathf.InverseLerp(shortTextLength, longTextLength, textLength);
        return Mathf.Lerp(minCharsPerSecond, maxCharsPerSecond, t);
    }
}
