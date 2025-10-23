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

    [SerializeField] private AudioClip A, As, B, E, F, Fs, G, Gs;
    private List<NoteList> battle0;
    private List<NoteList> battle1;
    private List<NoteList> battle2;
    private List<NoteList> currentBattle;
    private int currentBattleNumber = 0;

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
        InitBattles();
        BeatMachine.Instance.tick.AddListener(Tick);
    }

    private void Tick()
    {
        if (!NewCombatManager.Instance.bInCombat ||
            NewCombatManager.Instance.CurrentPhase != CombatPhase.EnemyTurn)
        {
            return;
        }

        if (waitTicks > 0)
        {
            waitTicks--;
            return;
        }

        if (noteIndex == 0 && waitTicks == 0)
        {
            bRiffFinished = false;
            ClearCombatInputs();
        }

        NewNote n = currentBattle[battleIndex].notes[noteIndex];

        if (n == null)
        {
            //Debug.Log($"Note {noteIndex} MISSING!");
            return;
        }

        switch (n.inputDir)
        {
            case InputDir.Left:
                {
                    audioSource.volume = 0.5f;
                    audioSource.panStereo = -1f;
                    break;
                }

            case InputDir.Right:
                {
                    audioSource.volume = 0.5f;
                    audioSource.panStereo = 1f;
                    break;
                }

            case InputDir.Up:
                {
                    audioSource.volume = 0.3f;
                    audioSource.panStereo = 0f;
                    break;
                }
        }

        combatInputs.Add(new CombatInput(n.inputDir, AudioSettings.dspTime + 0.10));
        audioSource.PlayOneShot(n.audioClip);

        waitTicks = Mathf.Max(0, n.eights - 1);
        noteIndex++;

        if (noteIndex >= currentBattle[battleIndex].notes.Count)
        {
            bRiffFinished = true;
            CombatInputHandler.Instance.ClearCombatInputs();
            lastRiffPlayed = currentBattle[battleIndex];
            battleIndex++;

            if (battleIndex >= currentBattle.Count())
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
        if (battleIndex < currentBattle.Count)
        {
            Debug.Log("Running again even when finished");
            return currentBattle[battleIndex].Count();
        }

        return 0;
    }

    public NoteList GetCurrentSongNotes()
    {
        return currentBattle[battleIndex];
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

        bRiffFinished = false;
        waitTicks = 16;
        battleIndex = 0;
        noteIndex = 0;
    }

    void InitBattles()
    {
        // Helper for readability
        NewNote N(AudioClip clip, InputDir dir, int eights = 4)
        {
            return new NewNote { audioClip = clip, inputDir = dir, eights = eights };
        }

        var calleRiff1 = new NoteList
        {
            notes = new List<NewNote> {
            N(E, InputDir.Left, 8),
            N(F, InputDir.Left, 8),
            N(G, InputDir.Right, 8),
            N(Gs, InputDir.Right, 8)
            
            }
        };

        // "Seven Nation Army" (E - G - A - E)
        var calleRiff2 = new NoteList
        {
            notes = new List<NewNote> {
            N(F, InputDir.Left, 2),
            N(F, InputDir.Left, 2),
            N(Gs, InputDir.Up, 4),
            N(G, InputDir.Up, 6),
            N(Gs, InputDir.Up, 6),
            N(A, InputDir.Right, 4),
            N(As, InputDir.Right, 8)
            }
        };

        // "Smoke on the Water" (simplified riff with E-G-A)
        var calleRiff3 = new NoteList
        {
            notes = new List<NewNote> {
             N(E, InputDir.Left, 4),
             N(G, InputDir.Up, 4),
             N(A, InputDir.Right, 6),
             N(E, InputDir.Left, 4),
             N(G, InputDir.Up, 4),
             N(As, InputDir.Right, 2),
             N(A, InputDir.Up, 8)
            }
        };
        var calleRiff4 = new NoteList
        {
            notes = new List<NewNote> {
            N(E, InputDir.Left, 4),
            N(G, InputDir.Up, 4),
            N(G, InputDir.Up, 2),
            N(A, InputDir.Right, 2),
            N(A, InputDir.Right, 8),
            N(As, InputDir.Right, 4),
            N(E, InputDir.Left, 8)
            }
        };

        var calleRiff5 = new NoteList
        {
            notes = new List<NewNote> {
            N(G, InputDir.Left, 4),
            N(Gs, InputDir.Up, 4),
            N(A, InputDir.Right, 4),
            N(As, InputDir.Right, 2),
            N(As, InputDir.Right, 2),
            N(F, InputDir.Up, 4),
            N(E, InputDir.Left, 4),
            N(A, InputDir.Right, 8),
            }
        };

        var calleRiff6 = new NoteList
        {
            notes = new List<NewNote> {
            N(B, InputDir.Right, 8),
            N(B, InputDir.Right, 4),
            N(Fs, InputDir.Left, 4),
            N(A, InputDir.Right, 4),
            N(G, InputDir.Right, 4),
            N(E, InputDir.Left, 8)
            }
        };

        var joelRiff2 = new NoteList
        {
            notes = new List<NewNote> {
            N(A, InputDir.Up, 4),
            N(A, InputDir.Up, 4),
            N(A, InputDir.Up, 8),
            N(E, InputDir.Left, 4),
            N(G, InputDir.Right, 4),
            N(A, InputDir.Up, 8),

            }
        };

        var joelRiff3 = new NoteList
        {
            notes = new List<NewNote> {
            
            N(E, InputDir.Left, 4),
            N(G, InputDir.Right, 4),
            N(E, InputDir.Left, 4),
            N(G, InputDir.Right, 4),
            N(A, InputDir.Up, 4),
            N(A, InputDir.Up, 4),
            N(A, InputDir.Up, 8)

            }
        };

        // Now combine them into a battle
        battle0 = new List<NoteList> { calleRiff1, calleRiff2, calleRiff3 };
        battle1 = new List<NoteList> { calleRiff4, calleRiff5, calleRiff6 };
        battle2 = new List<NoteList> { joelRiff2, joelRiff3 };
    }

    public int GetCurrentBattleNumber() { return currentBattleNumber; }
}
