using UnityEngine;

public class BedHideSpot : HideSpot
{
    private void Reset()
    {
        enterPrompt = "[E] Esconderse debajo de la cama";
        exitPrompt = "[E] Salir de debajo de la cama";

        restrictCameraLook = true;
        hiddenYawLimit = 1.5f;
        hiddenPitchLimit = 1.5f;

        reducesDetection = true;
        detectionMultiplier = 0.2f;
    }
}
