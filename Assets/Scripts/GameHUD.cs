using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [Header("Widgets")]
    [SerializeField] private InteractionPromptHUD interactionPrompt;
    [SerializeField] private StatusMessageHUD statusMessage;
    [SerializeField] private FlashlightHUD flashlightHud;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Reset()
    {
        if (interactionPrompt == null)
        {
            interactionPrompt = GetComponentInChildren<InteractionPromptHUD>(true);
        }

        if (statusMessage == null)
        {
            statusMessage = GetComponentInChildren<StatusMessageHUD>(true);
        }

        if (flashlightHud == null)
        {
            flashlightHud = GetComponentInChildren<FlashlightHUD>(true);
        }
    }

    public void SetInteractionPrompt(string prompt)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetPrompt(prompt);
        }
    }

    public void ClearInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.ClearPrompt();
        }
    }

    public void ShowStatusMessage(string message, float duration = 1.8f)
    {
        if (statusMessage != null)
        {
            statusMessage.ShowMessage(message, duration);
        }
    }

    public void BindFlashlight(FlashlightController controller)
    {
        if (flashlightHud != null)
        {
            flashlightHud.SetController(controller);
        }
    }
}
