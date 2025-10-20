using UnityEngine;

/*
 * 
 * TODO (Calle): Om vi har en stor cirkel som täcker hela planen, så kan vi sätta panorering i förhållande till mitt punkten
 * Som att det står ett par öron i mitten av huset som lyssnar efter en fotsteg.
 * 
 * om man går åt höger så minskar ljudvolymen med avståndet och ökar när man trycker höger men panoreringen växlar inte förens man 
 * kommit förbi mittpunkten.
 * 
 */

[System.Serializable]
public class FootstepAudioPlayer
{
    [SerializeField] private AudioSource audioSourceFootStep;
    [SerializeField] AudioClip[] audioClipFootsteps;

    [SerializeField] private double playTimeIntervallFootsteps;

    private double playTimer;
    
    public void Init()
    {
        playTimer = playTimeIntervallFootsteps;
    }

    public void SetPan(float pan)
    {
            audioSourceFootStep.panStereo = pan;  
    }

    public float GetPan() { return audioSourceFootStep.panStereo; }

    public void Stop()
    {
        // Så att vi spelare direkt när vi börjar gå igen
        playTimer = -1.0f;
    }

    public void Play()
    {
        playTimer -= Time.deltaTime;
        if(playTimer < 0.0f)
        {
            audioSourceFootStep.PlayOneShot(RandomClip());
            playTimer = playTimeIntervallFootsteps;
        }
        
    }

    AudioClip RandomClip()
    {
        int randomIndex = Random.Range(0, audioClipFootsteps.Length);
        return audioClipFootsteps[randomIndex];
    }
};

public class MovementHandler : MonoBehaviour
{

    [SerializeField] private FootstepAudioPlayer footstepAudioPlayer;
    [SerializeField] private AudioSource audioSourceGrunt;
    [SerializeField] private AudioClip[] audioClipGrunts;

    [SerializeField] private float speed = 1.0f;
    

    private Rigidbody rgbd;

    private Vector3 movementVector = new Vector3(0,0,0);

    private bool isWalking = false;
    private bool inCorner = false;
    private bool perpendicularToWall = false;

    void Start()
    {
        rgbd = GetComponent<Rigidbody>();
        footstepAudioPlayer.Init();
    }

    void FixedUpdate()
    {
        if (GameLevelHandler.Instance.GetLevelState() != LevelState.Navigation) // Joel: Combat och Navigation hanteras i samma scen
        {
            return;
        }

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
                footstepAudioPlayer.SetPan(0.0f);
            else if (movementVector.x > 0.0f)
                footstepAudioPlayer.SetPan(1.0f);
            else
                footstepAudioPlayer.SetPan(-1.0f);
        }
        else
        {
            isWalking = false;
        }
 
        if (isWalking && !inCorner && !perpendicularToWall)
        {
            footstepAudioPlayer.Play();
        }
        else
        {
           footstepAudioPlayer.Stop();
        }

        transform.position += movementVector;

    }

    private void PlayRandomGruntClip()
    {
        int random = Random.Range(0, audioClipGrunts.Length);
        audioSourceGrunt.panStereo = footstepAudioPlayer.GetPan();
        audioSourceGrunt.PlayOneShot(audioClipGrunts[random]);
    }

    private void OnCollisionEnter(Collision other)
    {

        if (other.gameObject.CompareTag("Wall"))
        {
            isWalking = false;

            if(IsCollidingPerpendicularToObject(other))
            {
                PlayRandomGruntClip();
            }
        }
    }

    private void OnCollisionStay(Collision other)
    {
        
        if(other.gameObject.CompareTag("Wall"))
        {
            isWalking = false;



            perpendicularToWall = IsCollidingPerpendicularToObject(other);
            
            if (perpendicularToWall && !audioSourceGrunt.isPlaying)
                PlayRandomGruntClip();

            Debug.Log("Perp to wall: " + perpendicularToWall);
        }
    }

    private bool IsCollidingPerpendicularToObject(Collision collision)
    {
        bool result = false;

        foreach (ContactPoint c in collision.contacts)
        {
            Vector3 movementDir = new Vector3(movementVector.x, 0.0f, movementVector.z).normalized;
            float dot = Vector3.Dot(c.normal, movementDir);

            if (dot < -0.8f)
            {
                result = true;
                break;

            }
        }

        return result;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {

            perpendicularToWall = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CornerTrigger"))
        {
            inCorner = true;
            PlayRandomGruntClip();
        }
        
        /* Joel: Gör detta m.m. i den nya EncounterBoundary-klassen
        if (other.gameObject.CompareTag("EncounterBoundry"))
        {
            GameLevelHandler.Instance.SetLevelState(LevelState.Combat);
        }
        */
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("CornerTrigger"))
        {
            inCorner = false;
        }
    }
}
