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

    [SerializeField] private List<NoteList> battle0;
    [SerializeField] private List<NoteList> battle1;
    [SerializeField] private List<NoteList> battle2;
    private List<NoteList> currentBattle;

    private int battleIndex = 0;
    private AudioSource audioSource;
    private List<CombatInput> combatInputs = new();

    public int noteIndex = 0;
    public int waitTicks = 0;

    public bool bRiffFinished = false;
    private NoteList lastRiffPlayed;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        BeatMachine.Instance.tick.AddListener(Tick);
    }

    private void Tick()
    {
        if (!NewCombatManager.Instance.bInCombat ||
            NewCombatManager.Instance.CurrentPhase != CombatPhase.EnemyTurn ||
            !battle0.Any() ||
            !battle0[battleIndex].notes.Any())
        {
            return;
        }

        if (waitTicks > 0)
        {
            waitTicks--;
            return;
        }

        NewNote n = battle0[battleIndex].notes[noteIndex];

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

        if (noteIndex >= battle0[battleIndex].notes.Count)
        {
            bRiffFinished = true;
            lastRiffPlayed = battle0[battleIndex];
            battleIndex++;

            if (battleIndex >= battle0.Count())
            {
                battleIndex = 0;
            }

            noteIndex = 0;
        }
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
        if (battleIndex < battle0.Count)
        {
            return battle0[battleIndex].Count();
        }

        return 0;
    }

    public NoteList GetCurrentSongNotes()
    {
        return battle0[battleIndex];
    }

    public NoteList GetLastRiffPlayed()
    {
        return lastRiffPlayed;
    }

    public void StartCombat(int enemyNumber)
    {
        switch (enemyNumber)
        {
            case 0:
                {
                    currentBattle = battle0;
                    break;
                }
            case 1:
                {
                    currentBattle = battle1;
                    break;
                }
            case 2:
                {
                    currentBattle = battle2;
                    break;
                }
            default:
                {
                    currentBattle = battle0;
                    break;
                }
        }

        waitTicks = 16;
        battleIndex = 0;
        noteIndex = 0;
        ClearCombatInputs();
    }
}
