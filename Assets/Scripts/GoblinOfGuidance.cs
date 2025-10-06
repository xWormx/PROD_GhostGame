using UnityEngine;

public class GoblinOfGuidance : MonoBehaviour
{
    // Singleton pattern
    public static GoblinOfGuidance Instance;
    private void Awake()
    {
        if (GoblinOfGuidance.Instance != null && GoblinOfGuidance.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            GoblinOfGuidance.Instance = this;
        }
    }
    // End of Singleton pattern

    // Cache
    private AudioSource audioSource;

    // Variables
    private KeyCode[] excludedKeys = { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    [SerializeField] private AudioClip[] audioClips;
    private int index = 0;

    [SerializeField] private AudioClip pressLeft;
    [SerializeField] private AudioClip pressRight;

    private bool hasPlayedFinalClip = false;

    private bool isActive = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isActive)
        {
            return;
        }

        if (Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    if (System.Array.Exists(excludedKeys, k => k == key))
                    {
                        return;
                    }

                    RepeatLastAudioClip();
                    return;
                }
            }
        }
    }

    private void RepeatLastAudioClip()
    {
        int repeatIndex = index;

        if (repeatIndex >= audioClips.Length || hasPlayedFinalClip)
        {
            repeatIndex = audioClips.Length - 1;
        }

        audioSource.Stop();
        audioSource.PlayOneShot(audioClips[repeatIndex]);
    }

    public void PlayNextAudioClip()
    {
        if (index >= audioClips.Length || hasPlayedFinalClip) return;

        audioSource.PlayOneShot(audioClips[index]);
        index++;
        hasPlayedFinalClip = index >= audioClips.Length;

        if (!isActive)
        {
            isActive = true;
        }
    }

    public bool CheckIfPlaying()
    {
        return audioSource.isPlaying;
    }

    public void PlayPressLeft()
    {
        audioSource.PlayOneShot(pressLeft);
    }

    public void PlayPressRight()
    {
        audioSource.PlayOneShot(pressRight);
    }
}
