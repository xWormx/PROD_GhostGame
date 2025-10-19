using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public static void Evaluate(List<CombatInput> expected, List<CombatInput> player)
    {
        // get tempo-dependent tick duration
        double tickInterval = BeatMachine.Instance.GetTickInterval();

        // define timing windows as fractions of tick length
        double perfectWindow = tickInterval * 0.5;
        double goodWindow = tickInterval * 1.0;

        var unmatchedPlayerInputs = new List<CombatInput>(player);

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
                BeatMachine.Instance.ChangeBPM(4.0f);

            }
            else if (diff <= goodWindow)
            {
                result = $"GOOD ({diff:F3}s)";
                BeatMachine.Instance.ChangeBPM(2.0f);
            }
            else
            {
                result = $"MISS ({diff:F3}s)";
                BeatMachine.Instance.ChangeBPM(-4.0f);
            }

            string timing = bestMatch.DSPTime < e.DSPTime ? "EARLY" : "LATE";
            Debug.Log($"{e.Direction}: {result} {timing}");

            unmatchedPlayerInputs.Remove(bestMatch);
        }

        foreach (var extra in unmatchedPlayerInputs)
        {
            Debug.Log($"Extra input: {extra.Direction} at {extra.DSPTime:F3}s (no matching note)");
        }
    }
}
