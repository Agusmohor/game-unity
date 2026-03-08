using MimicSpace;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerNoiseEmitter : MonoBehaviour
{
    [SerializeField] private float stepInterval = 0.45f;
    [SerializeField] private float walkLoudness = 1f;
    [SerializeField] private float minMoveInput = 0.1f;

    private CharacterController controller;
    private float stepTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > minMoveInput
            || Mathf.Abs(Input.GetAxisRaw("Vertical")) > minMoveInput;

        if (!isMoving || !controller.isGrounded)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;
        if (stepTimer >= stepInterval)
        {
            MimicNoiseSystem.EmitNoise(transform.position, walkLoudness, gameObject);
            stepTimer = 0f;
        }
    }
}
