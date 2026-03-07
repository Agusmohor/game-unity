using UnityEngine;

public class Player : MonoBehaviour
{
    Camera_movement cam;
    public float speed = 5f;

 
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponentInChildren<Camera_movement>();
    }

    // Update is called once per frames
    void Update()
    {
        playerMovement();
        Debug.Log(cam.getYaw());
    }

    private void playerMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(x,0,z);
        transform.Translate(move*speed*Time.deltaTime);
        float yaw = cam.getYaw();
        transform.rotation = Quaternion.Euler(0,yaw,0);
    }


}
