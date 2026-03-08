using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string keyId = "LlaveAsilo1";
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private bool notifyObjectiveManager = true;

    public void Interact(PlayerInteraction interactor)
    {
        if (interactor == null)
        {
            return;
        }

        bool added = interactor.AddKey(keyId);
        if (!added)
        {
            return;
        }

        if (notifyObjectiveManager && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RegisterKeyCollected(keyId);
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public string GetInteractionPrompt(PlayerInteraction interactor)
    {
        return "[E] Recoger llave";
    }
}
