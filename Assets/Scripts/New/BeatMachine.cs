using UnityEngine;
using UnityEngine.Events;

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

    public UnityEvent tick = new();

    [SerializeField] private float bpm = 100.0f;
    [SerializeField] private int notes = 8;
    [SerializeField] private AudioClip kick, snare, hihat, playersCue;

    private AudioSource audioSource;
    private int tickCounter = 0;
    private int beatCounter = 0;
    private double nextEventTime;
    private bool running = false;
    private int playersTurnCountdown = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        NewCombatManager.Instance.OnPlayerTurnStart.AddListener(PlayerTurnStart);
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
            Tick();
            nextEventTime += 60.0f / bpm / 4f; // Time for eight-notes
        }
    }


    void Tick()
    {
        tick.Invoke();

        tickCounter++;
        //Debug.Log($"Note: {tickCounter} / 8");

        if (tickCounter % 4 == 1 && playersTurnCountdown > 0)
        {
            playersTurnCountdown--;
            audioSource.PlayOneShot(playersCue);
        }

        if (tickCounter % 2 == 1)
        {
            audioSource.PlayOneShot(hihat);
        }

        if (tickCounter == 1)
        {
            audioSource.PlayOneShot(kick);
        }

        if (beatCounter % 2 == 1 && tickCounter == 3)
        {
            audioSource.PlayOneShot(kick);
        }

        if (tickCounter == 5)
        {
            audioSource.PlayOneShot(snare);
        }

        if (tickCounter >= notes)
        {
            tickCounter = 0;
            beatCounter++;
            //Debug.Log($"Beat: {beatCounter}");
        }
    }

    public void ChangeBPM(float f)
    {
        bpm += f;
    }

    public void Run(bool b)
    {
        if (b)
        {
            nextEventTime = AudioSettings.dspTime + 2.0f;
        }

        tickCounter = 0;
        beatCounter = 0;
        running = b;
    }

    public int GetTick()
    {
        return tickCounter;
    }

    public double GetTickInterval()
    {
        return 60.0 / bpm / 4.0;
    }

    private void PlayerTurnStart()
    {
        playersTurnCountdown = 4;
    }
}
