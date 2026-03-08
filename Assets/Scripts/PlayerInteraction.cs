using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private LayerMask interactionMask = ~0;
    [Header("UI prototipo")]
    [SerializeField] private bool showPromptOnScreen = true;

    private IInteractable currentInteractable;
    private string lookMessage = "";
    private string tempMessage = "";
    private float tempMessageUntil;
    private GUIStyle guiStyle;
    private PlayerInventory inventory;
    private Player player;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = GetComponentInParent<PlayerInventory>();
        }

        player = GetComponent<Player>();
        if (player == null)
        {
            player = GetComponentInParent<Player>();
        }
    }

    void Update()
    {
        UpdateLookTarget();

        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if (player != null && player.IsHidden)
        {
            player.ExitHideSpot();
            ShowTemporaryMessage("Saliste del escondite", 1.2f);
            return;
        }

        if (currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    public bool HasKey(string keyId)
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                inventory = GetComponentInParent<PlayerInventory>();
            }
        }

        return inventory != null && inventory.HasKey(keyId);
    }

    public bool AddKey(string keyId)
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                inventory = GetComponentInParent<PlayerInventory>();
            }
        }

        if (inventory == null)
        {
            Debug.LogWarning("PlayerInteraction: falta PlayerInventory en el jugador.");
            return false;
        }

        return inventory.AddKey(keyId);
    }

    public void ShowTemporaryMessage(string message, float duration = 1.5f)
    {
        tempMessage = message;
        tempMessageUntil = Time.time + duration;
    }

    private void UpdateLookTarget()
    {
        if (player == null)
        {
            player = GetComponent<Player>();
            if (player == null)
            {
                player = GetComponentInParent<Player>();
            }
        }

        if (player != null && player.IsHidden)
        {
            currentInteractable = null;
            lookMessage = "[E] Salir del escondite";
            return;
        }

        if (playerCamera == null)
        {
            currentInteractable = null;
            lookMessage = "";
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionMask, QueryTriggerInteraction.Ignore))
        {
            currentInteractable = null;
            lookMessage = "";
            return;
        }

        currentInteractable = hit.collider.GetComponentInParent<IInteractable>();
        lookMessage = currentInteractable != null ? currentInteractable.GetInteractionPrompt(this) : "";
    }

    private void OnGUI()
    {
        if (!showPromptOnScreen)
        {
            return;
        }

        string message = Time.time < tempMessageUntil ? tempMessage : lookMessage;
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (guiStyle == null)
        {
            guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.alignment = TextAnchor.MiddleCenter;
            guiStyle.fontSize = 20;
            guiStyle.normal.textColor = Color.white;
        }

        Rect rect = new Rect(0f, Screen.height - 90f, Screen.width, 30f);
        GUI.Label(rect, message, guiStyle);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Vector3 origin = playerCamera.transform.position;
        Vector3 end = origin + playerCamera.transform.forward * interactDistance;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(end, 0.04f);
    }
}
