using UnityEngine;

public class BeatMachine : MonoBehaviour
{
    public static BeatMachine Instance;
    private void Awake()
    {
        if (BeatMachine.Instance != null && BeatMachine.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            BeatMachine.Instance = this;
        }
    }

    [SerializeField] private float bpm = 100.0f;
    [SerializeField] private int numBeatsPerSegment = 16;
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] private AudioClip kick;
    [SerializeField] private AudioClip snare;
    [SerializeField] private AudioClip hihatc;
    [SerializeField] private AudioClip hihato;

    private double nextEventTime;
    private int flip = 0;
    private bool running = false;

    void Start()
    {
        nextEventTime = AudioSettings.dspTime + 2.0f;
        running = true;
    }

    void Update()
    {
        if (!running)
        {
            return;
        }

        double time = AudioSettings.dspTime;

        if (time + 1.0f > nextEventTime)
        {
            audioSources[flip].clip = kick;
            audioSources[flip].PlayScheduled(nextEventTime);

            //Debug.Log("Scheduled source " + flip + " to start at time " + nextEventTime);

            nextEventTime += 60.0f / bpm * numBeatsPerSegment;

            flip = 1 - flip;
        }
    }
}
