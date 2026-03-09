using System.Collections;
using UnityEngine;

public class ElevatorTelescopicDoor : MonoBehaviour, IInteractable
{
    [Header("Panels (Left = abre primero)")]
    [SerializeField] private Transform firstPanel;
    [SerializeField] private Transform secondPanel;

    [Header("Optional exact targets (recommended)")]
    [SerializeField] private bool useExactTargets = false;
    [SerializeField] private Transform firstClosedTarget;
    [SerializeField] private Transform secondClosedTarget;
    [SerializeField] private Transform firstOpenTarget;
    [SerializeField] private Transform secondOpenTarget;

    [Header("Motion")]
    [SerializeField] private Vector3 slideDirectionLocal = Vector3.right;
    [SerializeField] private float firstPanelDistance = 0.9f;
    [SerializeField] private float secondPanelDistance = 0.9f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float panelSequenceDelay = 0.05f;

    [Header("Interaction")]
    [SerializeField] private bool startOpen = false;
    [SerializeField] private bool ignoreInteractWhileMoving = true;
    [SerializeField] private string openPrompt = "[E] Abrir ascensor";
    [SerializeField] private string closePrompt = "[E] Cerrar ascensor";
    [SerializeField] private string movingPrompt = "Ascensor en movimiento...";

    private Vector3 firstClosedLocal;
    private Vector3 secondClosedLocal;
    private Vector3 firstOpenLocal;
    private Vector3 secondOpenLocal;

    private bool isOpen;
    private bool isMoving;
    private Coroutine moveRoutine;

    private void Awake()
    {
        if (firstPanel == null || secondPanel == null)
        {
            Debug.LogWarning("ElevatorTelescopicDoor: falta asignar firstPanel o secondPanel.");
            enabled = false;
            return;
        }

        Vector3 localDir = slideDirectionLocal.sqrMagnitude < 0.0001f
            ? Vector3.right
            : slideDirectionLocal.normalized;

        if (useExactTargets)
        {
            firstClosedLocal = firstClosedTarget != null ? firstClosedTarget.localPosition : firstPanel.localPosition;
            secondClosedLocal = secondClosedTarget != null ? secondClosedTarget.localPosition : secondPanel.localPosition;
            firstOpenLocal = firstOpenTarget != null ? firstOpenTarget.localPosition : firstClosedLocal + localDir * firstPanelDistance;
            secondOpenLocal = secondOpenTarget != null ? secondOpenTarget.localPosition : secondClosedLocal + localDir * secondPanelDistance;
        }
        else
        {
            firstClosedLocal = firstPanel.localPosition;
            secondClosedLocal = secondPanel.localPosition;
            firstOpenLocal = firstClosedLocal + localDir * firstPanelDistance;
            secondOpenLocal = secondClosedLocal + localDir * secondPanelDistance;
        }

        isOpen = startOpen;
        ApplyInstantPose(isOpen);
    }

    public void Interact(PlayerInteraction interactor)
    {
        if (isMoving && ignoreInteractWhileMoving)
        {
            return;
        }

        bool targetOpen = !isOpen;

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(AnimateDoor(targetOpen));
    }

    public string GetInteractionPrompt(PlayerInteraction interactor)
    {
        if (isMoving)
        {
            return movingPrompt;
        }

        return isOpen ? closePrompt : openPrompt;
    }

    private IEnumerator AnimateDoor(bool opening)
    {
        isMoving = true;

        if (opening)
        {
            yield return MovePanelTo(firstPanel, firstOpenLocal);
            if (panelSequenceDelay > 0f)
            {
                yield return new WaitForSeconds(panelSequenceDelay);
            }
            yield return MovePanelTo(secondPanel, secondOpenLocal);
            isOpen = true;
        }
        else
        {
            yield return MovePanelTo(secondPanel, secondClosedLocal);
            if (panelSequenceDelay > 0f)
            {
                yield return new WaitForSeconds(panelSequenceDelay);
            }
            yield return MovePanelTo(firstPanel, firstClosedLocal);
            isOpen = false;
        }

        isMoving = false;
        moveRoutine = null;
    }

    private IEnumerator MovePanelTo(Transform panel, Vector3 targetLocalPos)
    {
        float step;
        while ((panel.localPosition - targetLocalPos).sqrMagnitude > 0.00001f)
        {
            step = moveSpeed * Time.deltaTime;
            panel.localPosition = Vector3.MoveTowards(panel.localPosition, targetLocalPos, step);
            yield return null;
        }

        panel.localPosition = targetLocalPos;
    }

    private void ApplyInstantPose(bool opened)
    {
        firstPanel.localPosition = opened ? firstOpenLocal : firstClosedLocal;
        secondPanel.localPosition = opened ? secondOpenLocal : secondClosedLocal;
    }
}
