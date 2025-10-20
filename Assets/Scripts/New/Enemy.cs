using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class NewNote
{
    public AudioClip audioClip;
    public InputDir inputDir;
    public int eights;
}

[System.Serializable]
public class NoteList
{
    public List<NewNote> notes;

    public int Count()
    {
        return notes.Count();
    }
}

public class Enemy : MonoBehaviour
{
    public static Enemy Instance;

    private void Awake()
    {
        if (Enemy.Instance != null && Enemy.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Enemy.Instance = this;
        }
    }

    [SerializeField] private List<NoteList> levels;

    private int levelIndex = 0;
    private AudioSource audioSource;
    private List<CombatInput> combatInputs = new();

    public int noteIndex = 0;
    public int waitTicks = 0;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        BeatMachine.Instance.tick.AddListener(Tick);
    }

    private void Tick()
    {
        if (!NewCombatManager.Instance.bInCombat)
        {
            return;
        }

        if (NewCombatManager.Instance.CurrentPhase == CombatPhase.PlayerTurn)
        {
            return;
        }

        if (!levels.Any())
        {
            return;
        }

        if (levelIndex >= levels.Count())
        {
            levelIndex = 0; // Default to the first level
        }

        if (!levels[levelIndex].notes.Any())
        {
            return;
        }

        if (waitTicks > 0)
        {
            waitTicks--;
            return;
        }

        if (noteIndex >= levels[levelIndex].notes.Count)
        {
            return;
        }

        NewNote n = levels[levelIndex].notes[noteIndex];

        if (n == null)
        {
            //Debug.Log($"Note {noteIndex} MISSING!");
            return;
        }

        switch (n.inputDir)
        {
            case InputDir.Left:
                {
                    audioSource.volume = 1f;
                    audioSource.panStereo = -1f;
                    break;
                }

            case InputDir.Right:
                {
                    audioSource.volume = 1f;
                    audioSource.panStereo = 1f;
                    break;
                }

            case InputDir.Up:
                {
                    audioSource.volume = 0.5f;
                    audioSource.panStereo = 0f;
                    break;
                }
        }

        combatInputs.Add(new CombatInput(n.inputDir, AudioSettings.dspTime + 0.10));
        audioSource.PlayOneShot(n.audioClip);

        waitTicks = Mathf.Max(0, n.eights - 1);
        noteIndex++;
    }

    public void ClearCombatInputs() => combatInputs.Clear();

    public List<CombatInput> GetExpectedResponses()
    {
        if (combatInputs.Count == 0)
            return new List<CombatInput>();

        // Get the DSP time of the last note enemy played
        double lastNoteTime = combatInputs[combatInputs.Count - 1].DSPTime;

        // The player is expected to start after the last note + 2 full beats (or optional extra delay)
        double tickInterval = BeatMachine.Instance.GetTickInterval();
        double playerStartOffset = tickInterval * 24.0; // 8 ticks = 1 full beat

        List<CombatInput> expectedResponses = new();

        foreach (var note in combatInputs)
        {
            // Keep relative timing between enemy notes
            double relativeTime = note.DSPTime - combatInputs[0].DSPTime;

            expectedResponses.Add(
                new CombatInput(note.Direction, lastNoteTime + playerStartOffset + relativeTime)
            );
        }

        return expectedResponses;
    }


    public int GetCurrentSongNotesCount()
    {
        if (levelIndex < levels.Count)
        {
            return levels[levelIndex].Count();
        }

        return 0;
    }

    public void StartCombat(int level)
    {
        levelIndex = level;
        waitTicks = 16;
        noteIndex = 0;
        ClearCombatInputs();
    }
}
