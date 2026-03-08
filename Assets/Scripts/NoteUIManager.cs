using UnityEngine;
using TMPro;
using System.Collections;

public class NoteUIManager : MonoBehaviour
{
    public static NoteUIManager Instance { get; private set; }

    [Header("UI Refs")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private CanvasGroup noteCanvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private TMP_Text closeHintText;
    [SerializeField] private string closeHint = "E / Esc para cerrar";

    [Header("Input")]
    [SerializeField] private KeyCode closeKey = KeyCode.E;
    [SerializeField] private KeyCode alternateCloseKey = KeyCode.Escape;

    [Header("Fade")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool logOpenClose = false;

    private bool isOpen;
    private bool isTransitioning;
    private Player lockedPlayer;
    private Camera_movement lockedCamera;
    private Coroutine transitionRoutine;

    public bool IsOpen => isOpen || isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (notePanel != null && noteCanvasGroup == null)
        {
            noteCanvasGroup = notePanel.GetComponent<CanvasGroup>();
            if (noteCanvasGroup == null)
            {
                noteCanvasGroup = notePanel.AddComponent<CanvasGroup>();
            }
        }

        if (closeHintText != null)
        {
            closeHintText.text = closeHint;
        }

        if (notePanel != null)
        {
            if (noteCanvasGroup != null)
            {
                noteCanvasGroup.alpha = 0f;
                noteCanvasGroup.interactable = false;
                noteCanvasGroup.blocksRaycasts = false;
            }
            notePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        if (Input.GetKeyDown(closeKey) || Input.GetKeyDown(alternateCloseKey))
        {
            CloseNote();
        }
    }

    public void OpenNote(NoteData noteData, PlayerInteraction interactor)
    {
        if (noteData == null)
        {
            return;
        }

        CachePlayerRefs(interactor);

        isOpen = true;
        SetControlLock(true); // bloqueo movimiento/look mientras se lee

        if (notePanel != null)
        {
            notePanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = noteData.Title;
        }

        if (contentText != null)
        {
            contentText.text = noteData.Content;
        }

        if (closeHintText != null)
        {
            closeHintText.text = closeHint;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        if (noteCanvasGroup == null || !useFade || fadeDuration <= 0f)
        {
            isTransitioning = false;
            if (noteCanvasGroup != null)
            {
                noteCanvasGroup.alpha = 1f;
                noteCanvasGroup.interactable = true;
                noteCanvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            transitionRoutine = StartCoroutine(FadeCanvas(1f, true));
        }

        if (logOpenClose)
        {
            Debug.Log("NoteUIManager: note abierta.");
        }
    }

    public void CloseNote()
    {
        if (!isOpen)
        {
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        if (noteCanvasGroup == null || !useFade || fadeDuration <= 0f)
        {
            isOpen = false;
            isTransitioning = false;

            if (noteCanvasGroup != null)
            {
                noteCanvasGroup.alpha = 0f;
                noteCanvasGroup.interactable = false;
                noteCanvasGroup.blocksRaycasts = false;
            }

            if (notePanel != null)
            {
                notePanel.SetActive(false);
            }

            SetControlLock(false);
        }
        else
        {
            transitionRoutine = StartCoroutine(FadeCanvas(0f, false));
        }

        if (logOpenClose)
        {
            Debug.Log("NoteUIManager: note cerrada.");
        }
    }

    private void CachePlayerRefs(PlayerInteraction interactor)
    {
        if (interactor == null)
        {
            return;
        }

        lockedPlayer = interactor.GetComponent<Player>();
        if (lockedPlayer == null)
        {
            lockedPlayer = interactor.GetComponentInParent<Player>();
        }

        if (lockedPlayer != null)
        {
            lockedCamera = lockedPlayer.GetComponentInChildren<Camera_movement>();
        }
    }

    private void SetControlLock(bool locked)
    {
        if (lockedPlayer != null)
        {
            lockedPlayer.SetMovementLocked(locked);
        }

        if (lockedCamera != null)
        {
            lockedCamera.SetLookInputEnabled(!locked);
        }
    }

    private IEnumerator FadeCanvas(float targetAlpha, bool opening)
    {
        isTransitioning = true;

        if (noteCanvasGroup == null)
        {
            isTransitioning = false;
            yield break;
        }

        noteCanvasGroup.interactable = opening;
        noteCanvasGroup.blocksRaycasts = opening;

        float startAlpha = noteCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            noteCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        noteCanvasGroup.alpha = targetAlpha;
        isTransitioning = false;
        transitionRoutine = null;

        if (!opening)
        {
            isOpen = false;
            noteCanvasGroup.interactable = false;
            noteCanvasGroup.blocksRaycasts = false;
            if (notePanel != null)
            {
                notePanel.SetActive(false);
            }
            SetControlLock(false);
        }
    }
}
