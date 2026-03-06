using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float speed = 5f;
    public float m_sens = 100;
    public float cam_rot = 0;

    float pitch;
    float yaw;
 
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(x,0,z);
        transform.Translate(move*speed*Time.deltaTime);


        //Camera
        camMovement();
      
    }

    private void camMovement()
    {
        float rot_x = Input.GetAxis("Mouse X") * m_sens;
        float rot_y = Input.GetAxis("Mouse Y") * m_sens;

        yaw += rot_x;
        pitch -= rot_y;
        pitch = Mathf.Clamp(pitch, -90, 90);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0);   
        Debug.Log(rot_x);
        Debug.Log(rot_y);
    }

}
