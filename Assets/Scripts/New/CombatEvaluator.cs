using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InputAccuracy
{
    Perfect,
    Good,
    Miss
}

public class CombatEvaluator : MonoBehaviour
{
    public static CombatEvaluator Instance;

    private void Awake()
    {
        if (CombatEvaluator.Instance != null && CombatEvaluator.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            CombatEvaluator.Instance = this;
        }
    }

    [SerializeField] private AudioClip good, bad;
    private AudioSource audioSource;

    private int timesCalled = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Evaluate(List<CombatInput> expected, List<CombatInput> player)
    {
        timesCalled++;
        Debug.Log($"CombatEvaluator.Evaluate() called {timesCalled} times total.");
        float bpmChange = 0f;

        double tickInterval = BeatMachine.Instance.GetTickInterval();
        double perfectWindow = tickInterval * 0.5;
        double goodWindow = tickInterval * 1.0;

        var unmatchedPlayerInputs = new List<CombatInput>(player);
        CombatInputHandler.Instance.ClearCombatInputs();

        foreach (var e in expected)
        {
            var candidates = unmatchedPlayerInputs
                .Where(p => p.Direction == e.Direction)
                .ToList();

            if (candidates.Count == 0)
            {
                Debug.Log($"MISS - No input for {e.Direction}");
                continue;
            }

            var bestMatch = candidates
                .OrderBy(p => Mathf.Abs((float)(p.DSPTime - e.DSPTime)))
                .First();

            double diff = Mathf.Abs((float)(bestMatch.DSPTime - e.DSPTime));

            string result;
            if (diff <= perfectWindow)
            {
                result = $"PERFECT ({diff:F3}s)";
                bpmChange += 4f;
            }
            else if (diff <= goodWindow)
            {
                result = $"GOOD ({diff:F3}s)";
                bpmChange += 2f;
            }
            else
            {
                EventLogger.instance.LogEvent($"Missed during battle {Enemy.Instance.GetCurrentBattleNumber()}");
                result = $"MISS ({diff:F3}s)";
                bpmChange -= 4f;
            }

            string timing = bestMatch.DSPTime < e.DSPTime ? "EARLY" : "LATE";
            Debug.Log($"{e.Direction}: {result} {timing}");

            unmatchedPlayerInputs.Remove(bestMatch);
        }

        foreach (var extra in unmatchedPlayerInputs)
        {
            //Debug.Log($"Extra input: {extra.Direction} at {extra.DSPTime:F3}s (no matching note)");
            bpmChange -= 4f;
            EventLogger.instance.LogEvent($"Non-matching input '{extra.Direction}' during battle {Enemy.Instance.GetCurrentBattleNumber()}");
            Debug.Log($"BPM down. {unmatchedPlayerInputs.Count()} umatched inputs.");
        }

        if (bpmChange > 0)
        {
            // Player wins this turn: POSITIVE SFX
            audioSource.PlayOneShot(good);
            Debug.Log("Riff over: Positive outcome!");
        }
        else
        {
            // Enemy wins this turn: NEGATIVE SFX
            audioSource.PlayOneShot(bad);
            Debug.Log("Riff over: Negative outcome.");
        }

        Debug.Log($"BPM change: {bpmChange}");
        BeatMachine.Instance.ChangeBPM(bpmChange);
    }

    public InputAccuracy CompareInput(CombatInput expected, CombatInput player, bool playSFX = true)
    {
        if (expected.Direction != player.Direction)
        {
            if (playSFX) audioSource.PlayOneShot(bad);
            return InputAccuracy.Miss;
        }

        double tickInterval = BeatMachine.Instance.GetTickInterval();
        double perfectWindow = tickInterval * 0.5;
        double goodWindow = tickInterval * 1.0;

        double diff = Mathf.Abs((float)(player.DSPTime - expected.DSPTime));

        InputAccuracy result;
        if (diff <= perfectWindow)
        {
            result = InputAccuracy.Perfect;
            if (playSFX) audioSource.PlayOneShot(good);
        }
        else if (diff <= goodWindow)
        {
            result = InputAccuracy.Good;
            if (playSFX) audioSource.PlayOneShot(good);
        }
        else
        {
            result = InputAccuracy.Miss;
            if (playSFX) audioSource.PlayOneShot(bad);
        }

        return result;
    }
}
