using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string keyId = "LlaveAsilo1";
    [SerializeField] private bool destroyOnPickup = true;

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
