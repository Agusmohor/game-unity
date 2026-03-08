using UnityEngine;

public class Camera_movement : MonoBehaviour
{
    public float sens = 100f;

    float pitch;
    float yaw;
    bool restrictLook;
    float restrictYawCenter;
    float restrictPitchCenter;
    float restrictYawLimit;
    float restrictPitchLimit;

    void Update()
    {
        camMovement();
    }

    public void camMovement()
    {
        float rot_x = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        float rot_y = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        yaw += rot_x;
        pitch -= rot_y;

        if (restrictLook)
        {
            yaw = Mathf.Clamp(yaw, restrictYawCenter - restrictYawLimit, restrictYawCenter + restrictYawLimit);
            pitch = Mathf.Clamp(pitch, restrictPitchCenter - restrictPitchLimit, restrictPitchCenter + restrictPitchLimit);
        }

        pitch = Mathf.Clamp(pitch, -90f, 90f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public float getYaw()
    {
        return yaw;
    }

    public void SetHiddenLookLimits(float yawLimit, float pitchLimit)
    {
        restrictLook = true;
        restrictYawCenter = yaw;
        restrictPitchCenter = pitch;
        restrictYawLimit = Mathf.Max(0f, yawLimit);
        restrictPitchLimit = Mathf.Max(0f, pitchLimit);
    }

    public void ClearHiddenLookLimits()
    {
        restrictLook = false;
    }

    public void SnapView(float worldYaw, float worldPitch = 0f)
    {
        yaw = NormalizeAngle(worldYaw);
        pitch = Mathf.Clamp(NormalizeAngle(worldPitch), -90f, 90f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }
}
