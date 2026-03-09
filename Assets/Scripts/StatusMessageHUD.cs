using System.Collections;
using UnityEngine;
using TMPro;

public class StatusMessageHUD : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.12f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float defaultDuration = 1.8f;

    private Coroutine messageRoutine;

    private void Awake()
    {
        EnsureCanvasGroup();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowMessage(string message, float duration = -1f)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }

        float finalDuration = duration > 0f ? duration : defaultDuration;
        messageRoutine = StartCoroutine(ShowRoutine(message.Trim(), finalDuration));
    }

    private IEnumerator ShowRoutine(string message, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        yield return FadeTo(1f, fadeInDuration);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return FadeTo(0f, fadeOutDuration);
        messageRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        if (duration <= 0.0001f)
        {
            canvasGroup.alpha = targetAlpha;
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
}
