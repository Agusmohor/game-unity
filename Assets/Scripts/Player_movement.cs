using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    Camera_movement cam;
    CharacterController controller;

    public float speed = 5f;
    public float gravity = -9.81f;

    float verticalVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponentInChildren<Camera_movement>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        RotatePlayerWithCameraYaw();
        MovePlayer();
    }

    void RotatePlayerWithCameraYaw()
    {
        if (cam == null)
        {
            return;
        }

        transform.rotation = Quaternion.Euler(0f, cam.getYaw(), 0f);
    }

    void MovePlayer()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = move * speed + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }
}
