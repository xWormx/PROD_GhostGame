using UnityEngine;

public class EncounterBoundary : MonoBehaviour
{
    [SerializeField] private float startingBPM = 100.0f;
    [SerializeField] private float winBPM = 200.0f;
    [SerializeField] private float loseBPM = 60.0f;

    private bool bIsActive = true;

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (bIsActive)
        {
            bIsActive = false;
            NewCombatManager.Instance.RunCombat(0, startingBPM, winBPM, loseBPM);
            GameLevelHandler.Instance.SetLevelState(LevelState.Combat);
        }
    }
}
