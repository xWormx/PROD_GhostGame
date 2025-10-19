using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CombatPhase
{
    EnemyTurn,
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

    public CombatPhase CurrentPhase { get; private set; }

    public UnityEvent OnEnemyTurnStart = new();
    public UnityEvent OnPlayerTurnStart = new();
    public UnityEvent OnEvaluateStart = new();

    public bool bInCombat { get; private set; } = false;

    [SerializeField] private AudioClip audioClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        RunCombat();
    }

    public void RunCombat()
    {
        BeatMachine.Instance.Run(true);
        bInCombat = true;
        StartCoroutine(Combat());
    }

    private IEnumerator Combat()
    {
        while (bInCombat)
        {
            Debug.Log("Enemy's Turn.");
            CurrentPhase = CombatPhase.EnemyTurn;
            CombatInputHandler.Instance.ClearCombatInputs();
            Enemy.Instance.ClearCombatInputs();
            OnEnemyTurnStart?.Invoke();

            while (!EnemySequenceFinished())
                yield return null;

            Debug.Log("Player's Turn.");
            List<CombatInput> expectedResponses = Enemy.Instance.GetExpectedResponses();
            CurrentPhase = CombatPhase.PlayerTurn;
            CombatInputHandler.Instance.ClearCombatInputs();
            OnPlayerTurnStart?.Invoke();

            while (CombatInputHandler.Instance.GetCombatInputs().Count < expectedResponses.Count)
                yield return null;

            CurrentPhase = CombatPhase.Evaluate;
            OnEvaluateStart?.Invoke();

            List<CombatInput> expected = Enemy.Instance.GetExpectedResponses();
            List<CombatInput> player = CombatInputHandler.Instance.GetCombatInputs();

            CombatEvaluator.Evaluate(expected, player);
        }
    }

    private bool EnemySequenceFinished()
    {
        return Enemy.Instance.noteIndex >= Enemy.Instance.GetCurrentSongNotesCount() && Enemy.Instance.waitTicks == 0;
    }
}
