using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Windows.WebCam.VideoCapture;

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
    private Riff[] currentSongRiffs;
    private int currentRiffIndex = 0;
    private Riff currentRiff;
    private bool bEnemyIsPlaying = false;
    private bool bIsPlayersTurn = false;
    private Note[] notesToMatch;
    private List<InputState> inputs = new();

    private void Start()
    {
        if (!songs.Any())
        {
            Debug.LogError("CombatManager.songs is empty!");
            return;
        }

        currentSong = songs[currentSongIndex];
        currentSongRiffs = currentSong.GetRiffs();
        currentRiff = currentSongRiffs[currentRiffIndex];
        notesToMatch = currentRiff.GetNotes();
    }

    private void Update()
    {
        if (!songs.Any()) return;

        if (BeatManager.Instance.beat % 8  == 0 && !bEnemyIsPlaying)
        {
            Debug.Log("8 beats have passed...");

            EnemysTurn();
        }

        PlayersTurn();
    }

    private void EnemysTurn()
    {
        bIsPlayersTurn = false;
        bEnemyIsPlaying = true;

        if (currentRiffIndex >= currentSongRiffs.Length)
        {
            currentSongIndex++;

            if (currentSongIndex >= songs.Length)
            {
                // All songs beat
            }
            else
            {
                currentSong = songs[currentSongIndex];
                currentSongRiffs = currentSong.GetRiffs();
                currentRiffIndex = 0;
                currentRiff = currentSongRiffs[currentRiffIndex];
            }
        }
        else
        {
            currentRiff = currentSongRiffs[currentRiffIndex];
            notesToMatch = currentRiff.GetNotes();
        }

        StartCoroutine(PlayRiff(currentRiff));
        currentRiffIndex++;
    }

    private IEnumerator PlayRiff(Riff riff)
    {
        foreach (Note note in riff.GetNotes())
        {
            SoundHandler.Instance.SetAudioState(note.GetDirection());
            SoundHandler.Instance.PlayRandomAudioClip();
            yield return new WaitForSeconds(1f);
        }

        bEnemyIsPlaying = false;
        if (inputs.Any()) inputs.Clear();
        bIsPlayersTurn = true;
    }

    private void PlayersTurn()
    {
        if (!bIsPlayersTurn) return;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            inputs.Add(InputState.InputState_Left);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            inputs.Add(InputState.InputState_Right);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            inputs.Add(InputState.InputState_Up);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            inputs.Add(InputState.InputState_Up);
        }

        if (!inputs.Any()) return;

        // Kontrollera om sekvensen av inputs överensstämmer med noterna som ska spelas
        // Noterna markeras automatiskt som avklarade om de stämmer
        for (int i = 0; i < notesToMatch.Length; i++)
        {
            if (!notesToMatch[i].CompareInput(inputs[i]))
            {
                inputs.Clear();
                return;
            }

            // Riff avklarat! Spela upp coolt ljud
            // TO-DO: LÄGG TILL EN BOOL SÅ ATT MAN BARA KLARAR RIFFET EN GÅNG
            Debug.Log("CombatManager::PlayersTurn() : Riff played successfully!");
        }
    }
}
