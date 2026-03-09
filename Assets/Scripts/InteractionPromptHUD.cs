using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptHUD : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private Image iconImage;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.12f;
    [SerializeField] private float fadeOutDuration = 0.16f;

    private string currentPrompt = "";
    private Coroutine fadeRoutine;

    private void Awake()
    {
        EnsureCanvasGroup();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        SetIconVisible(iconImage != null && iconImage.sprite != null);
    }

    public void SetPrompt(string prompt, Sprite iconOverride = null)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            ClearPrompt();
            return;
        }

        string normalized = prompt.Trim();
        if (normalized == currentPrompt)
        {
            return;
        }

        currentPrompt = normalized;
        ApplyPrompt(currentPrompt);

        if (iconImage != null)
        {
            if (iconOverride != null)
            {
                iconImage.sprite = iconOverride;
                SetIconVisible(true);
            }
            else
            {
                SetIconVisible(iconImage.sprite != null);
            }
        }

        StartFade(1f, fadeInDuration);
    }

    public void ClearPrompt()
    {
        if (string.IsNullOrEmpty(currentPrompt) && canvasGroup.alpha <= 0.001f)
        {
            return;
        }

        currentPrompt = "";

        if (keyText != null)
        {
            keyText.text = "";
        }

        if (actionText != null)
        {
            actionText.text = "";
        }

        StartFade(0f, fadeOutDuration);
    }

    private void ApplyPrompt(string prompt)
    {
        if (keyText == null && actionText == null)
        {
            return;
        }

        string parsedKey = "";
        string parsedAction = prompt;

        if (prompt.StartsWith("["))
        {
            int closeBracket = prompt.IndexOf(']');
            if (closeBracket > 1)
            {
                parsedKey = prompt.Substring(0, closeBracket + 1);
                parsedAction = prompt.Substring(closeBracket + 1).TrimStart();
            }
        }

        if (keyText != null)
        {
            keyText.text = parsedKey;
        }

        if (actionText != null)
        {
            actionText.text = string.IsNullOrEmpty(parsedAction) ? prompt : parsedAction;
        }
    }

    private void StartFade(float targetAlpha, float duration)
    {
        EnsureCanvasGroup();

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        if (duration <= 0.0001f)
        {
            canvasGroup.alpha = targetAlpha;
            fadeRoutine = null;
            yield break;
        }

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        fadeRoutine = null;
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null)
        {
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void SetIconVisible(bool visible)
    {
        if (iconImage == null)
        {
            return;
        }

        iconImage.enabled = visible;
    }
}
