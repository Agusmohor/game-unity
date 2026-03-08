using UnityEngine;

public class WorldState : MonoBehaviour
{
    [Header("Power")]
    [SerializeField] private GameObject lightsGroup;
    [SerializeField] private bool logWhenPowerRestored = true;

    private bool powerRestored;

    public bool PowerRestored => powerRestored;

    public void OnPowerRestored()
    {
        if (powerRestored)
        {
            return;
        }

        powerRestored = true;

        if (lightsGroup != null)
        {
            lightsGroup.SetActive(true);
        }

        if (logWhenPowerRestored)
        {
            Debug.Log("WorldState: electricidad restaurada.");
        }
    }
}
