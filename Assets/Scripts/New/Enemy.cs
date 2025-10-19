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

    [SerializeField] private List<NoteList> songs;

    private int songIndex = 0;
    private AudioSource audioSource;
    private List<CombatInput> combatInputs = new();

    public int noteIndex { get; private set; } = 0;
    public int waitTicks { get; private set; } = 0;


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

        if (!songs.Any())
        {
            return;
        }

        if (!songs[songIndex].notes.Any())
        {
            return;
        }

        if (waitTicks > 0)
        {
            waitTicks--;
            return;
        }

        if (noteIndex >= songs[songIndex].notes.Count)
        {
            return;
        }

        NewNote n = songs[songIndex].notes[noteIndex];

        if (n == null)
        {
            Debug.Log($"Note {noteIndex} MISSING!");
            return;
        }

        switch (n.inputDir)
        {
            case InputDir.Left:
                {
                    audioSource.panStereo = -1f;
                    break;
                }

            case InputDir.Right:
                {
                    audioSource.panStereo = 1f;
                    break;
                }

            case InputDir.Up:
                {
                    audioSource.panStereo = 0f;
                    break;
                }
        }

        combatInputs.Add(new CombatInput(n.inputDir, AudioSettings.dspTime));
        audioSource.PlayOneShot(n.audioClip);

        waitTicks = Mathf.Max(0, n.eights - 1);
        noteIndex++;
    }

    public void ClearCombatInputs() => combatInputs.Clear();

    public List<CombatInput> GetExpectedResponses(double extraDelay = 0.0)
    {
        if (combatInputs.Count == 0)
            return new List<CombatInput>();

        // Get the DSP time of the last note enemy played
        double lastNoteTime = combatInputs[combatInputs.Count - 1].DSPTime;

        // The player is expected to start after the last note + 1 full beat (or optional extra delay)
        double tickInterval = BeatMachine.Instance.GetTickInterval();
        double playerStartOffset = tickInterval * 8.0 + extraDelay; // 8 ticks = 1 full beat

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
        if (songIndex < songs.Count)
        {
            return songs[songIndex].Count();
        }

        return 0;
    }
}
