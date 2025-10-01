using System.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    // Singleton pattern
    public static CombatManager Instance;
    private void Awake()
    {
        if (CombatManager.Instance != null && CombatManager.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            CombatManager.Instance = this;
        }
    }
    // End of Singleton pattern

    [SerializeField] private Song[] songs;
    private int currentSongIndex = 0;
    private Song currentSong;
    private int currentRiffIndex = 0;
    private Riff currentRiff;

    private void Start()
    {
        if (!songs.Any())
        {
            Debug.LogError("CombatManager.songs is empty!");
        }
    }

    private void Update()
    {
        if (!songs.Any()) return;

        if (BeatManager.Instance.beat % 8  == 0)
        {
            // Spela nästa riff
        }
    }

    private void PlayRiff(Riff riff)
    {

    }
}
