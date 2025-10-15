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

[System.Serializable]
public class Melody
{
    public AudioClip[] notes;
};

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

    [SerializeField] private Melody[] melodies;
    [SerializeField] private int currentMelody;
    [SerializeField] private int currentNote;

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

        //this.bShouldPlay = Input.anyKeyDown;
        this.bShouldPlay = false;
        
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
            int randomIndex = Random.Range(0, melodies.Length);
            switch (audioState.GetInputState())
            {
                case InputState.InputState_None:
                    {

                    }
                    break;
                case InputState.InputState_Left:
                    {
                        audioSrc.volume = 1f;
                        audioSrc.panStereo = -1.0f;
                        audioSrc.PlayOneShot(melodies[currentMelody].notes[currentNote], 1.0f);
                    }
                    break;

                case InputState.InputState_Right:
                    {
                        audioSrc.volume = 1f;
                        audioSrc.panStereo = 1.0f;
                        audioSrc.PlayOneShot(melodies[currentMelody].notes[currentNote], 1.0f);
                    }
                    break;
                case InputState.InputState_Up:
                    {
                        audioSrc.volume = 0.5f;
                        audioSrc.panStereo = 0.0f;
                        audioSrc.PlayOneShot(melodies[currentMelody].notes[currentNote], 0.8f);
                    }
                    break;
                case InputState.InputState_Down: // Only for testing sounds behind.
                    {
                        audioSrc.volume = 0.5f;
                        audioSrc.panStereo = 0.0f;
                        audioSrc.PlayOneShot(melodies[currentMelody].notes[currentNote], 0.5f);
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

    public void SetAudioState(InputState direction)
    {
        switch(direction)
        {
            case InputState.InputState_Left:
                {
                    audioSrc.volume = 1f;
                    audioSrc.panStereo = -1.0f;
                    break;
                }
            case InputState.InputState_Right:
                {
                    audioSrc.volume = 1f;
                    audioSrc.panStereo = 1.0f;
                    break;
                }
            case InputState.InputState_Up:
                {
                    audioSrc.volume = 0.5f;
                    audioSrc.panStereo = 0f;
                    break;
                }
        }
    }

    public void SetActiveRandomMelody()
    {

        // TODO (Calle): det ska vara random, men kör bara melodi '0' för test.
        //currentMelody = Random.Range(0, melodies.Length);
        currentMelody = 0;
        currentNote = 0;
    }

    public void PlayeNextNoteInActiveMelody()
    {
        audioSrc.PlayOneShot(melodies[0].notes[currentNote]);
        currentNote++;
        // NOTE (Calle): Detta är en safeguard för nu, ifall antal noter i meldin är mindre
        //               än antalet inputs från combaten.
        if(currentNote >= melodies[0].notes.Length) 
            currentNote = 0;
    }

    //public void PlayRandomAudioClip()
    //{
    //    int randomIndex = Random.Range(0, melodies.Length);
    //    audioSrc.PlayOneShot(melodies[currentMelody].notes[currentNote]);
    //}
}
