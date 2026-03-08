public interface IInteractable
{
    void Interact(PlayerInteraction interactor);
    string GetInteractionPrompt(PlayerInteraction interactor);
}
