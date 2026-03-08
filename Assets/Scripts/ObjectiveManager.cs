using UnityEngine;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour
{
    public enum ObjectiveStep
    {
        EnterAsylum = 0,
        FindPowerRoomKey = 1,
        OpenPowerRoomDoor = 2,
        RestoreElectricity = 3,
        Completed = 4
    }

    public static ObjectiveManager Instance { get; private set; }

    [Header("Objective IDs")]
    [SerializeField] private string powerRoomKeyId = "LlaveCuartoElectrico";
    [SerializeField] private string powerRoomDoorId = "PuertaCuartoElectrico";
    [SerializeField] private string generatorId = "GeneradorPrincipal";

    [Header("UI (simple)")]
    [SerializeField] private bool showObjectiveOnScreen = true;
    [SerializeField] private Vector2 uiPosition = new Vector2(20f, 20f);
    [SerializeField] private int fontSize = 24;

    [Header("Events")]
    [SerializeField] private UnityEvent onElectricityRestored;

    [Header("Debug")]
    [SerializeField] private bool logChanges = true;
    [SerializeField] private bool allowOutOfOrderProgression = true;

    [SerializeField] private ObjectiveStep currentStep = ObjectiveStep.EnterAsylum;
    [SerializeField] private bool electricityRestored;
    private bool powerEventFired;

    private GUIStyle objectiveStyle;

    public ObjectiveStep CurrentStep => currentStep;
    public bool ElectricityRestored => electricityRestored;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public string GetCurrentObjectiveText()
    {
        switch (currentStep)
        {
            case ObjectiveStep.EnterAsylum:
                return "Objetivo: Entrar al asilo";
            case ObjectiveStep.FindPowerRoomKey:
                return "Objetivo: Encontrar la llave del cuarto electrico";
            case ObjectiveStep.OpenPowerRoomDoor:
                return "Objetivo: Abrir la puerta del cuarto electrico";
            case ObjectiveStep.RestoreElectricity:
                return "Objetivo: Restaurar la electricidad";
            case ObjectiveStep.Completed:
                return "Objetivo completado: Electricidad restaurada";
            default:
                return "";
        }
    }

    public void RegisterEnteredAsylum()
    {
        if (!allowOutOfOrderProgression && currentStep != ObjectiveStep.EnterAsylum)
        {
            return;
        }

        AdvanceToAtLeast(ObjectiveStep.FindPowerRoomKey);
    }

    public void RegisterKeyCollected(string keyId)
    {
        if (!string.Equals(keyId, powerRoomKeyId, System.StringComparison.Ordinal))
        {
            return;
        }

        if (!allowOutOfOrderProgression && currentStep != ObjectiveStep.FindPowerRoomKey)
        {
            return;
        }

        AdvanceToAtLeast(ObjectiveStep.OpenPowerRoomDoor);
    }

    public void RegisterDoorOpened(string doorId)
    {
        if (!string.Equals(doorId, powerRoomDoorId, System.StringComparison.Ordinal))
        {
            return;
        }

        if (!allowOutOfOrderProgression && currentStep != ObjectiveStep.OpenPowerRoomDoor)
        {
            return;
        }

        AdvanceToAtLeast(ObjectiveStep.RestoreElectricity);
    }

    public void RegisterGeneratorActivated(string sourceGeneratorId)
    {
        if (!string.Equals(sourceGeneratorId, generatorId, System.StringComparison.Ordinal))
        {
            return;
        }

        if (!allowOutOfOrderProgression && currentStep != ObjectiveStep.RestoreElectricity)
        {
            return;
        }

        electricityRestored = true;
        AdvanceToAtLeast(ObjectiveStep.Completed);

        if (!powerEventFired)
        {
            powerEventFired = true;
            onElectricityRestored?.Invoke();
        }
    }

    public void ForceStep(ObjectiveStep step)
    {
        currentStep = step;
        electricityRestored = currentStep >= ObjectiveStep.Completed;
    }

    private void AdvanceToAtLeast(ObjectiveStep next)
    {
        if (next <= currentStep)
        {
            return;
        }

        currentStep = next;
        if (logChanges)
        {
            Debug.Log("Objective -> " + currentStep);
        }
    }

    private void OnGUI()
    {
        if (!showObjectiveOnScreen)
        {
            return;
        }

        string text = GetCurrentObjectiveText();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (objectiveStyle == null)
        {
            objectiveStyle = new GUIStyle(GUI.skin.label);
            objectiveStyle.fontSize = fontSize;
            objectiveStyle.normal.textColor = Color.white;
        }

        Rect rect = new Rect(uiPosition.x, uiPosition.y, Screen.width - 40f, 40f);
        GUI.Label(rect, text, objectiveStyle);
    }
}
