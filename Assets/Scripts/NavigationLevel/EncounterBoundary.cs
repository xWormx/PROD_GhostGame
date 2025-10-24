using UnityEngine;

public class EncounterBoundary : MonoBehaviour
{
    [SerializeField] private int enemyNumber = 0;
    [SerializeField] private float startingBPM = 100.0f;
    [SerializeField] private float winBPM = 200.0f;
    [SerializeField] private float loseBPM = 60.0f;
    [SerializeField] private float respawnTime = 180.0f;
    [SerializeField] private GameObject parent;

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

            NewCombatManager.Instance.audioSource.Stop();
            NewCombatManager.Instance.RunCombat(enemyNumber, startingBPM, winBPM, loseBPM);
            GameLevelHandler.Instance.SetLevelState(LevelState.Combat);
            parent.SetActive(false);

            Invoke("Respawn", respawnTime);
        }
    }

    private void Respawn()
    {
        parent.SetActive(true);
        bIsActive = true;
    }
}
