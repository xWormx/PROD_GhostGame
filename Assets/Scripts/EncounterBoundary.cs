using UnityEngine;

public class EncounterBoundary : MonoBehaviour
{
    [SerializeField] private int enemyNumber = 0;
    [SerializeField] private float startingBPM = 100.0f;
    [SerializeField] private float winBPM = 200.0f;
    [SerializeField] private float loseBPM = 60.0f;
    [SerializeField] private GameObject parent;

    private bool bIsActive = true;
    private bool bHasMoved = false;
    private Vector3 startPos;

    private void Start()
    {
        startPos = parent.transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (bIsActive)
        {
            bIsActive = false;
            NewCombatManager.Instance.RunCombat(enemyNumber, startingBPM, winBPM, loseBPM);
            GameLevelHandler.Instance.SetLevelState(LevelState.Combat);
            
            if (bHasMoved)
            {
                parent.transform.position = startPos;
            }
            else
            {
                parent.transform.position = new Vector3(0, 0, 0);
            }

            bIsActive = true;
        }
    }
}
