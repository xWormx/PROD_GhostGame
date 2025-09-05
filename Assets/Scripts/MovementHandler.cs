using UnityEngine;

public class MovementHandler : MonoBehaviour
{
    private Rigidbody rgbd;
    [SerializeField]
    private float speed = 1.0f;

    Vector3 movementVector = new Vector3(0,0,0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rgbd = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        movementVector = new Vector3(0,0,0);

        if (Input.GetKey("p"))
        {
            transform.position = new Vector3(0, 1, 0);
        }
        if (Input.GetKey("w"))
        {
            Debug.Log("HEJ");
            movementVector.z = speed;
        }
        if (Input.GetKey("s"))
        {
            movementVector.z = -speed;
        }
        if (Input.GetKey("d"))
        {
            movementVector.x = speed;
        }
        if (Input.GetKey("a"))
        {
            movementVector.x = -speed;
        }
        transform.position += movementVector;
        //rgbd.AddForce(movementVector * Time.deltaTime, ForceMode.VelocityChange);
    }
}
