using UnityEngine;
using UnityEngine.Events;

public class GeneratorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string generatorId = "GeneradorPrincipal";
    [SerializeField] private bool oneTimeActivation = true;
    [SerializeField] private string interactPrompt = "[E] Activar generador";
    [SerializeField] private string activatedPrompt = "Generador activado";
    [SerializeField] private UnityEvent onActivated;

    private bool activated;

    public void Interact(PlayerInteraction interactor)
    {
        if (activated && oneTimeActivation)
        {
            if (interactor != null)
            {
                interactor.ShowTemporaryMessage("El generador ya esta activo", 1.2f);
            }
            return;
        }

        activated = true;
        onActivated?.Invoke();

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RegisterGeneratorActivated(generatorId);
        }

        if (interactor != null)
        {
            interactor.ShowTemporaryMessage("Electricidad restaurada", 1.5f);
        }
    }

    public string GetInteractionPrompt(PlayerInteraction interactor)
    {
        if (activated && oneTimeActivation)
        {
            return activatedPrompt;
        }

        return interactPrompt;
    }
}
