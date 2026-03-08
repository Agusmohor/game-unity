using UnityEngine;
using UnityEngine.Events;

public class NoteInteractable : MonoBehaviour, IInteractable
{
    [Header("Note")]
    [SerializeField] private string noteId = "";
    [SerializeField] private string noteTitle = "Nota";
    [SerializeField] [TextArea(6, 20)] private string noteContent = "Contenido de la nota...";
    [SerializeField] private string interactPrompt = "[E] Leer nota";

    [Header("Optional Objective")]
    [SerializeField] private bool advanceObjectiveOnFirstRead = false;
    [SerializeField] private ObjectiveManager.ObjectiveStep objectiveStepToAdvance = ObjectiveManager.ObjectiveStep.FindPowerRoomKey;

    [Header("Optional Event")]
    [SerializeField] private bool oneTimeReadEvent = true;
    [SerializeField] private UnityEvent onRead;

    private bool wasRead;

    public void Interact(PlayerInteraction interactor)
    {
        if (NoteUIManager.Instance == null)
        {
            Debug.LogWarning("NoteInteractable: falta NoteUIManager en la escena.");
            return;
        }

        NoteData runtimeData = new NoteData(noteId, noteTitle, noteContent);
        if (string.IsNullOrWhiteSpace(noteContent) || noteContent == "Contenido de la nota...")
        {
            Debug.LogWarning("NoteInteractable: la nota '" + gameObject.name + "' tiene contenido default o vacio.");
        }

        NoteUIManager.Instance.OpenNote(runtimeData, interactor);

        bool shouldFireReadEffects = !wasRead || !oneTimeReadEvent;
        if (!shouldFireReadEffects)
        {
            return;
        }

        if (!wasRead && advanceObjectiveOnFirstRead && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.AdvanceToStep(objectiveStepToAdvance);
        }

        onRead?.Invoke();
        wasRead = true;
    }

    public string GetInteractionPrompt(PlayerInteraction interactor)
    {
        if (NoteUIManager.Instance != null && NoteUIManager.Instance.IsOpen)
        {
            return "";
        }

        return interactPrompt;
    }
}
