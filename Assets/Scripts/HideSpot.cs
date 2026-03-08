using UnityEngine;

public class HideSpot : MonoBehaviour, IInteractable
{
    [Header("Anchors")]
    [SerializeField] private Transform hideAnchor;
    [SerializeField] private Transform exitAnchor;

    [Header("Prompt")]
    [SerializeField] private string enterPrompt = "[E] Esconderse";
    [SerializeField] private string exitPrompt = "[E] Salir";

    [Header("Detection (Opcional)")]
    [SerializeField] private bool reducesDetection = true;
    [SerializeField] [Range(0.05f, 1f)] private float detectionMultiplier = 0.35f;
    [Header("Camera While Hidden")]
    [SerializeField] private bool restrictCameraLook = true;
    [SerializeField] [Range(0f, 45f)] private float hiddenYawLimit = 4f;
    [SerializeField] [Range(0f, 45f)] private float hiddenPitchLimit = 6f;

    public bool ReducesDetection => reducesDetection;
    public float DetectionMultiplier => detectionMultiplier;
    public bool HasExitPoint => exitAnchor != null;
    public bool RestrictCameraLook => restrictCameraLook;
    public float HiddenYawLimit => hiddenYawLimit;
    public float HiddenPitchLimit => hiddenPitchLimit;

    public void Interact(PlayerInteraction interactor)
    {
        if (interactor == null)
        {
            return;
        }

        Player player = interactor.GetComponent<Player>();
        if (player == null)
        {
            player = interactor.GetComponentInParent<Player>();
        }

        if (player == null)
        {
            Debug.LogWarning("HideSpot: no se encontro Player en el interactor o sus padres.");
            interactor.ShowTemporaryMessage("No se encontro el Player para esconderse", 1.5f);
            return;
        }

        if (player.IsHidden)
        {
            if (player.CurrentHideSpot == this)
            {
                player.ExitHideSpot();
                interactor.ShowTemporaryMessage("Saliste del escondite", 1.2f);
            }
            return;
        }

        bool entered = player.EnterHideSpot(this);
        if (entered)
        {
            interactor.ShowTemporaryMessage("Te escondiste", 1.2f);
        }
        else
        {
            interactor.ShowTemporaryMessage("No se pudo entrar al escondite", 1.2f);
        }
    }

    public string GetInteractionPrompt(PlayerInteraction interactor)
    {
        if (interactor == null)
        {
            return enterPrompt;
        }

        Player player = interactor.GetComponent<Player>();
        if (player == null)
        {
            player = interactor.GetComponentInParent<Player>();
        }

        if (player != null && player.IsHidden && player.CurrentHideSpot == this)
        {
            return exitPrompt;
        }

        if (player != null && player.IsHidden)
        {
            return "";
        }

        return enterPrompt;
    }

    public Vector3 GetHidePosition()
    {
        return hideAnchor != null ? hideAnchor.position : transform.position;
    }

    public Quaternion GetHideRotation()
    {
        return hideAnchor != null ? hideAnchor.rotation : transform.rotation;
    }

    public Vector3 GetExitPosition()
    {
        return exitAnchor != null ? exitAnchor.position : transform.position;
    }

    public Quaternion GetExitRotation()
    {
        return exitAnchor != null ? exitAnchor.rotation : transform.rotation;
    }
}
