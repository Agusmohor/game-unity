using UnityEngine;

public class Camera_movement : MonoBehaviour
{
    public float sens = 100f;

    float pitch;
    float yaw;

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
        pitch = Mathf.Clamp(pitch, -90, 90);

        transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    public float getYaw()
    {
        return yaw;
    }
}
