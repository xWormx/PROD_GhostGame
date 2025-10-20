using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CombatPhase
{
    EnemyTurn,
    PlayerCue,
    PlayerTurn,
    Evaluate
}

public class NewCombatManager : MonoBehaviour
{
    public static NewCombatManager Instance;

    private void Awake()
    {
        if (NewCombatManager.Instance != null && NewCombatManager.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            NewCombatManager.Instance = this;
        }
    }

    public CombatPhase CurrentPhase;

    public UnityEvent OnEnemyTurnStart = new();
    public UnityEvent OnPlayerTurnStart = new();
    public UnityEvent OnEvaluateStart = new();

    public bool bInCombat { get; private set; } = false;

    [SerializeField] private AudioClip winSound, loseSound;
    private AudioSource audioSource;

    private float winBPM = 200.0f;
    private float loseBPM = 60.0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //RunCombat(0);
    }

    public void RunCombat(int level, float startingBPM = 100.0f, float winBPM = 200.0f, float loseBPM = 60.0f)
    {
        BeatMachine.Instance.SetBPM(startingBPM);

        this.winBPM = winBPM;
        this.loseBPM = loseBPM;

        Enemy.Instance.StartCombat(level);
        BeatMachine.Instance.Run(true);
        bInCombat = true;

        StartCoroutine(Combat());
    }

    private IEnumerator Combat()
    {
        while (bInCombat)
        {
            Enemy.Instance.bRiffFinished = false;
            // Enemy's turn
            //Debug.Log("Enemy's Turn.");
            CurrentPhase = CombatPhase.EnemyTurn;
            CombatInputHandler.Instance.ClearCombatInputs();
            Enemy.Instance.ClearCombatInputs();
            OnEnemyTurnStart?.Invoke();

            while (!EnemySequenceFinished())
                yield return null;

            // Player's turn
            //Debug.Log("Player's Turn.");
            List<CombatInput> expectedResponses = Enemy.Instance.GetExpectedResponses();
            CurrentPhase = CombatPhase.PlayerCue;
            OnPlayerTurnStart?.Invoke();
            CombatInputHandler.Instance.ClearCombatInputs();

            double turnStartTime = AudioSettings.dspTime;

            /*
            while (CombatInputHandler.Instance.GetCombatInputs().Count < expectedResponses.Count && 
                AudioSettings.dspTime < expectedResponses[expectedResponses.Count - 1].DSPTime + 0.5)
            {
                yield return null;
            }
            */

            while (AudioSettings.dspTime < expectedResponses[expectedResponses.Count - 1].DSPTime + 0.5)
            {
                yield return null;
            }

            /*
            if (CombatInputHandler.Instance.GetCombatInputs().Count < expectedResponses.Count)
            {
                Debug.Log("Player timed out!");
                audioSource.PlayOneShot(loseSound);
            }
            */

            for (int i = CombatInputHandler.Instance.GetCombatInputs().Count; i < expectedResponses.Count; i++)
            {
                CombatInputHandler.Instance.AddNullInput();
            }

            // Evaluation
            //Debug.Log("Evaluating...");
            CurrentPhase = CombatPhase.Evaluate;
            OnEvaluateStart?.Invoke();

            List<CombatInput> expected = Enemy.Instance.GetExpectedResponses();
            List<CombatInput> player = CombatInputHandler.Instance.GetCombatInputs();

            CombatEvaluator.Instance.Evaluate(expected, player);

            // End of one round
            if (BeatMachine.Instance != null)
            {
                var bpmField = typeof(BeatMachine)
                    .GetField("bpm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                float currentBpm = (float)bpmField.GetValue(BeatMachine.Instance);

                //Debug.Log($"Current BPM: {currentBpm}");

                if (currentBpm >= winBPM)
                {
                    GameLevelHandler.Instance.SetLevelState(LevelState.Navigation);
                    //Debug.Log("Max BPM reached — PLAYER WIN!");
                    bInCombat = false;
                    BeatMachine.Instance.Run(false);
                    audioSource.PlayOneShot(winSound);
                    yield break;
                }

                if (currentBpm < loseBPM)
                {
                    GameLevelHandler.Instance.SetLevelState(LevelState.Navigation);
                    //Debug.Log("Min BPM reached — PLAYER LOSE!");
                    bInCombat = false;
                    BeatMachine.Instance.Run(false);
                    audioSource.PlayOneShot(loseSound);
                    yield break;
                }
            }

            int ticksPerBeat = 8;
            int currentTick = BeatMachine.Instance.GetTick();

            int ticksToNextBeat = (ticksPerBeat - (currentTick % ticksPerBeat)) % ticksPerBeat;

            int extraBeats = 2;
            int waitTicks = ticksToNextBeat + ticksPerBeat * extraBeats;

            Enemy.Instance.noteIndex = 0;
            Enemy.Instance.waitTicks = waitTicks;

            //Debug.Log($"Scheduling next enemy round: currentTick={currentTick}, ticksToNextBeat={ticksToNextBeat}, waitTicks={waitTicks}");

            yield return null; // yield one frame

        }
    }

    private bool EnemySequenceFinished()
    {
        return Enemy.Instance.bRiffFinished && Enemy.Instance.waitTicks == 0;
    }
}
