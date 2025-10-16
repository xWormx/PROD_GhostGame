using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InputDir
{
    Left,
    Right,
    Up
}

[System.Serializable]
public class NewNote
{
    public AudioClip audioClip;
    public InputDir inputDir;
    public float duration;
}

[System.Serializable]
public class NoteList
{
    public List<NewNote> notes;
}


public class NotePlayer : MonoBehaviour
{
    public static NotePlayer Instance;

    private void Awake()
    {
        if (NotePlayer.Instance != null && NotePlayer.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            NotePlayer.Instance = this;
        }
    }

    [SerializeField] private List<NoteList> songs;

    private int songIndex = 0;
    private int noteIndex = 0;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        BeatMachine.Instance.tick.AddListener(Tick);
        BeatMachine.Instance.Run(true);
    }

    private void Tick()
    {
        if (!songs.Any())
        {
            return;
        }

        if (!songs[songIndex].notes.Any())
        {
            return;
        }

        NewNote n = null;

        if (noteIndex < songs[songIndex].notes.Count)
        {
            n = songs[songIndex].notes[noteIndex];
            //Debug.Log($"Note {noteIndex} found!");
        }

        if (n == null)
        {
            //Debug.Log($"Note {noteIndex} MISSING!");
            return;
        }

        if (BeatMachine.Instance.GetTick() % 8 == 0)
        {
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

            audioSource.PlayOneShot(n.audioClip);
            noteIndex++;
        }
    }
}
