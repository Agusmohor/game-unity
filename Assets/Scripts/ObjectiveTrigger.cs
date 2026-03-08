using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        EnterAsylum
    }

    [SerializeField] private TriggerType triggerType = TriggerType.EnterAsylum;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool alreadyTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered && triggerOnlyOnce)
        {
            return;
        }

        Player player = other.GetComponent<Player>();
        if (player == null)
        {
            player = other.GetComponentInParent<Player>();
        }

        if (player == null)
        {
            return;
        }

        if (ObjectiveManager.Instance == null)
        {
            return;
        }

        if (triggerType == TriggerType.EnterAsylum)
        {
            ObjectiveManager.Instance.RegisterEnteredAsylum();
        }

        alreadyTriggered = true;
    }
}
