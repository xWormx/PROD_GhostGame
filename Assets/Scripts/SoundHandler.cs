using UnityEngine;

public enum InputState
{
    InputState_None,
    InputState_Left,
    InputState_Right,
    InputState_Up,
    InputState_Down
};
public enum ToneType
{
    ToneType_None,
    ToneType_A
};

public class AudioState
{
    private InputState inputState;
    private ToneType toneType;

    public AudioState()
    {
        inputState = InputState.InputState_None;
        toneType = ToneType.ToneType_None;
    }

    public InputState GetInputState() { return this.inputState; }
    public ToneType GetToneType() { return  this.toneType; }

    public void SetInputState(InputState inputState) { this.inputState = inputState; }
    public void SetToneType(ToneType toneType) {  this.toneType = toneType; }
}

public class SoundHandler : MonoBehaviour
{
    // Singleton pattern
    public static SoundHandler Instance;
    private void Awake()
    {
        if (SoundHandler.Instance != null && SoundHandler.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            SoundHandler.Instance = this;
        }
    } // End of Singleton pattern

    [SerializeField] private AudioClip[] audioClips;
    private bool bShouldPlay = false;
    private AudioState audioState;
    private AudioSource audioSrc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioState = new AudioState();
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
        this.bShouldPlay = Input.anyKeyDown;
        
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            audioState.SetInputState(InputState.InputState_Left);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            audioState.SetInputState(InputState.InputState_Right);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            audioState.SetInputState(InputState.InputState_Up);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            audioState.SetInputState(InputState.InputState_Down);
        }
        else
        {
            audioState.SetInputState(InputState.InputState_None);
        }

        if (bShouldPlay)
        {
            int randomIndex = Random.Range(0, audioClips.Length);
            switch (audioState.GetInputState())
            {
                case InputState.InputState_None:
                    {

                    }
                    break;
                case InputState.InputState_Left:
                    {
                        audioSrc.panStereo = -1.0f;
                        audioSrc.PlayOneShot(audioClips[randomIndex], 1.0f);
                    }
                    break;

                case InputState.InputState_Right:
                    {
                        audioSrc.panStereo = 1.0f;
                        audioSrc.PlayOneShot(audioClips[randomIndex], 1.0f);
                    }
                    break;
                case InputState.InputState_Up:
                    {
                        audioSrc.panStereo = 0.0f;
                        audioSrc.PlayOneShot(audioClips[randomIndex], 0.8f);
                    }
                    break;
                case InputState.InputState_Down: // Only for testing sounds behind.
                    {
                        audioSrc.panStereo = 0.0f;
                        audioSrc.PlayOneShot(audioClips[randomIndex], 0.5f);
                    }
                    break;
            }
            ;

        }
    }

    public InputState GetInputState()
    {
        return audioState.GetInputState();
    }
}
