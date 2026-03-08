using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    Camera_movement cam;
    CharacterController controller;
    Rigidbody rb;

    public float speed = 5f;
    public float gravity = -9.81f;

    float verticalVelocity;
    bool isHidden;
    HideSpot currentHideSpot;
    Vector3 preHidePosition;
    Quaternion preHideRotation;
    bool rbWasKinematic;

    public bool IsHidden => isHidden;
    public HideSpot CurrentHideSpot => currentHideSpot;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponentInChildren<Camera_movement>();
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        RotatePlayerWithCameraYaw();

        if (isHidden)
        {
            verticalVelocity = 0f;
            return;
        }

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

    public bool EnterHideSpot(HideSpot hideSpot)
    {
        if (hideSpot == null || isHidden)
        {
            return false;
        }

        preHidePosition = transform.position;
        preHideRotation = transform.rotation;

        isHidden = true;
        currentHideSpot = hideSpot;

        controller.enabled = false;

        if (rb != null)
        {
            rbWasKinematic = rb.isKinematic;
            if (!rbWasKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }

        transform.SetPositionAndRotation(hideSpot.GetHidePosition(), hideSpot.GetHideRotation());

        if (cam != null)
        {
            // Force camera/player orientation to the hide anchor facing direction.
            cam.SnapView(hideSpot.GetHideRotation().eulerAngles.y, 0f);

            if (hideSpot.RestrictCameraLook)
            {
                cam.SetHiddenLookLimits(hideSpot.HiddenYawLimit, hideSpot.HiddenPitchLimit);
            }
            else
            {
                cam.ClearHiddenLookLimits();
            }
        }

        return true;
    }

    public void ExitHideSpot()
    {
        if (!isHidden)
        {
            return;
        }

        controller.enabled = false;

        if (currentHideSpot != null && currentHideSpot.HasExitPoint)
        {
            transform.SetPositionAndRotation(currentHideSpot.GetExitPosition(), currentHideSpot.GetExitRotation());
        }
        else
        {
            transform.SetPositionAndRotation(preHidePosition, preHideRotation);
        }

        if (rb != null)
        {
            rb.isKinematic = rbWasKinematic;
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        controller.enabled = true;

        if (cam != null)
        {
            cam.ClearHiddenLookLimits();
        }

        isHidden = false;
        currentHideSpot = null;
        verticalVelocity = 0f;
    }

    public float GetHiddenDetectionMultiplier()
    {
        if (!isHidden || currentHideSpot == null || !currentHideSpot.ReducesDetection)
        {
            return 1f;
        }

        return Mathf.Clamp(currentHideSpot.DetectionMultiplier, 0.05f, 1f);
    }
}
