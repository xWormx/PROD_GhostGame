using UnityEngine;
using UnityEngine.SceneManagement;



public enum LevelState
{
    Navigation, 
    Combat, 
    EndGame
};

public class GameLevelHandler : MonoBehaviour
{
    // Singleton
    public static GameLevelHandler Instance {  get; private set; }

    [SerializeField] private int demonsDefeated = 0;
    [SerializeField] private LevelState levelState = LevelState.Navigation;

    private GameObject currentEncounteredDemon;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public LevelState GetLevelState() { return levelState; }

    public void SetLevelState(LevelState state)
    {
        levelState = state;
        Debug.Log($"LevelState = {state}");

        switch(levelState)
        {
            case LevelState.Navigation:
                NavigationAudioHandler.Instance.SnapshotToNavigation();
                //SceneManager.LoadScene(1);
                break;
                
            case LevelState.Combat:
                NavigationAudioHandler.Instance.SnapshotToCombat();
                //SceneManager.LoadScene(2);
                break;

            case LevelState.EndGame:
                break;
        }
    }

    public void SetCurrentEncounteredDemon(GameObject encounteredDemon)
    {
        currentEncounteredDemon = encounteredDemon;
    }

    public void DemonDefeated()
    {
        demonsDefeated++;
        RemoveDemon();
    }

    public int GetDemonsDefeated() { return demonsDefeated; }

    private void RemoveDemon()
    {
        if(currentEncounteredDemon)
            Destroy(currentEncounteredDemon);
    }
}
