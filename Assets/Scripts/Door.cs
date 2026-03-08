using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private bool opensToPositiveY = true;
    [Header("Lock")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string requiredKeyId = "";
    [Header("Objective (optional)")]
    [SerializeField] private bool notifyObjectiveOnFirstOpen = false;
    [SerializeField] private string objectiveDoorId = "";

    private bool isOpen;
    private bool objectiveOpenNotified;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Awake()
    {
        closedRotation = transform.localRotation;
        float signedAngle = opensToPositiveY ? openAngle : -openAngle;
        openRotation = closedRotation * Quaternion.Euler(0f, signedAngle, 0f);
    }

    void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            target,
            rotationSpeed * Time.deltaTime
        );
    }

    public void Interact(PlayerInteraction interactor)
    {
        if (isLocked)
        {
            if (interactor == null)
            {
                Debug.Log("Door: interactor nulo.");
                return;
            }

            if (!interactor.HasKey(requiredKeyId))
            {
                interactor.ShowTemporaryMessage("La puerta esta cerrada con llave");
                Debug.Log("Puerta bloqueada. Falta llave: " + requiredKeyId);
                return;
            }
        }

        isOpen = !isOpen;

        if (isOpen && notifyObjectiveOnFirstOpen && !objectiveOpenNotified && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RegisterDoorOpened(objectiveDoorId);
            objectiveOpenNotified = true;
        }
    }

    public string GetInteractionPrompt(PlayerInteraction interactor)
    {
        if (isLocked)
        {
            bool hasKey = interactor != null && interactor.HasKey(requiredKeyId);
            if (!hasKey)
            {
                return "[E] Abrir puerta (bloqueada)";
            }
        }

        return isOpen ? "[E] Cerrar puerta" : "[E] Abrir puerta";
    }
}
