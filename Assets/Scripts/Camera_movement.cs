using UnityEngine;

public class Camera_movement : MonoBehaviour
{

    public float sens = 100f;
    public float cam_rot = 0;

    float pitch;
    float yaw;

    void Start()
    {
        
    }

    void Update()
    {
        camMovement();
    }

    public void camMovement()
    {
        float rot_x = Input.GetAxis("Mouse X");
        float rot_y = Input.GetAxis("Mouse Y");

        yaw += rot_x;
        pitch -= rot_y;
        pitch = Mathf.Clamp(pitch, -90, 90);

        transform.localRotation = Quaternion.Euler(pitch,0, 0);   
    }

    public float getYaw()
    {
        return yaw;
    }

}
