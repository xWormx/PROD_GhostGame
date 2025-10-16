using UnityEngine;

public class MovementHandler : MonoBehaviour
{
    /*
     * TODO (Calle): En Audiosource 
     * 
     * 
     * 
     */

    [SerializeField] private AudioSource audioSrcFootsteps;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] AudioClip[] audioClipFootsteps;

    private Rigidbody rgbd;

    private Vector3 movementVector = new Vector3(0,0,0);

    private bool isWalking = false;
    
    void Start()
    {
        rgbd = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        movementVector = new Vector3(0,0,0);

        if (Input.GetKey("p"))
        {
            transform.position = new Vector3(0, 1, 0);
        }
        if (Input.GetKey("w"))
        {
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

        if ((movementVector.x != 0.0f) ||
           (movementVector.z != 0.0f))
        {
            isWalking = true;
            if (movementVector.z > 0.0f || movementVector.z < 0.0f) 
                audioSrcFootsteps.panStereo = 0.0f;
            else if (movementVector.x > 0.0f)
                audioSrcFootsteps.panStereo = 1.0f;
            else
                audioSrcFootsteps.panStereo = -1.0f;
        }
        else
        {
            isWalking = false;
        }


        transform.position += movementVector;

        if (isWalking)
        {
            if (!audioSrcFootsteps.isPlaying)
            {
                int randomFootstepClip = Random.Range(0, audioClipFootsteps.Length);
                audioSrcFootsteps.clip = audioClipFootsteps[randomFootstepClip];
                audioSrcFootsteps.Play();
            }
        }
        else
        {
            audioSrcFootsteps.Stop();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall");
            isWalking = false;
        }
    }
}
