using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.InputSystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;
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

    private const int beatsPerTurn = 8;

    private AudioSource audioSource;
    [SerializeField] private AudioClip successSound;
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
    private int lastHandledBeat = -1;
    private int enemyNotesPlayed = 0;

    // Input
    InputSystem_Actions inputActions;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;

        if (!songs.Any())
        {
            Debug.LogError("CombatManager.songs is empty!");
            return;
        }

        currentSong = songs[currentSongIndex];
        currentSong.Reset();
        currentSongRiffs = currentSong.GetRiffs();
        currentRiff = currentSongRiffs[currentRiffIndex];
        currentRiff.Reset();
        notesToMatch = currentRiff.GetNotes();
    }

    private void EnemysTurn()
    {
        Debug.Log("Enemy's Turn.");

        bIsPlayersTurn = false;
        bEnemyIsPlaying = true;

        SelectNextRiff();
    }

    private void SelectNextRiff()
    {
        if (currentRiffIndex >= currentSongRiffs.Length)
        {
            currentSongIndex++;

            if (currentSongIndex >= songs.Length)
            {
                // All songs won at this point
            }
            else
            {
                // Select next song
                currentSong = songs[currentSongIndex];
                currentSong.Reset();
                currentSongRiffs = currentSong.GetRiffs();
                currentRiffIndex = 0;
                currentRiff = currentSongRiffs[currentRiffIndex];
                currentRiff.Reset();
            }
        }
        else
        {
            // Select next riff
            currentRiff = currentSongRiffs[currentRiffIndex];
            currentRiff.Reset();
            notesToMatch = currentRiff.GetNotes();
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log("OnMove : input detected.");

        if (!bIsPlayersTurn) return;

        Vector2 inputVector = context.ReadValue<Vector2>();

        if (inputVector.x < 0)
        {
            inputs.Add(InputState.InputState_Left);
            Debug.Log("Left");
        }
        else if (inputVector.x > 0)
        {
            inputs.Add(InputState.InputState_Right);
            Debug.Log("Right");
        }
        else if (inputVector.y != 0)
        {
            inputs.Add(InputState.InputState_Up);
            Debug.Log("Middle");
        }

        if (CheckRiffSuccess() == true)
        {
            bIsPlayersTurn = false;
        }
    }

    private bool CheckRiffSuccess()
    {
        //Debug.Log("CheckRiffSuccess()");

        if (!inputs.Any())
        {
            //Debug.Log("CheckRiffSuccess()::No input found!");
            return false;
        }

        if (currentRiff.CheckSuccess() == true)
        {
            //Debug.Log("CheckRiffSuccess()::Riff already marked as completed.");
            return false;
        }

        //Debug.Log("CheckRiffSuccess()::Inputs found, and riff not yet completed.");

        // Kontrollera om sekvensen av inputs överensstämmer med noterna som ska spelas
        // Noterna markeras automatiskt som avklarade om de stämmer
        for (int i = 0; i < notesToMatch.Length; i++)
        {
            if (notesToMatch.Length != inputs.Count)
            {
                //Debug.Log("CheckRiffSuccess()::Couldn't find all needed inputs.");
                return false;
            }

            if (!notesToMatch[i].CompareInput(inputs[i]))
            {
                //Debug.Log("CheckRiffSuccess()::Wrong input found!");
                inputs.Clear();
                return false;
            }
        }

        if (currentRiff.CheckSuccess())
        {
            Debug.Log("CheckRiffSuccess()::RIFF PLAYED SUCCESSFULLY!!!");
            audioSource.PlayOneShot(successSound);
            currentRiffIndex++;
            return true;
        }

        return false;
    }

    public void OnBeat()
    {
        if (!songs.Any()) return;

        int currentBeat = BeatManager.Instance.beat;

        if (currentBeat != 1 && currentBeat % beatsPerTurn == 1 && !bEnemyIsPlaying && currentBeat != lastHandledBeat)
        {
            lastHandledBeat = currentBeat;
            EnemysTurn();
        }

        if (!bEnemyIsPlaying) return;

        if (enemyNotesPlayed < notesToMatch.Length)
        {
            SoundHandler.Instance.SetAudioState(notesToMatch[enemyNotesPlayed].GetDirection());
            SoundHandler.Instance.PlayRandomAudioClip();
            enemyNotesPlayed++;
        }
        
        if (enemyNotesPlayed >= notesToMatch.Length)
        {
            enemyNotesPlayed = 0;
            bEnemyIsPlaying = false;
            if (inputs.Any()) inputs.Clear();
            bIsPlayersTurn = true;
            Debug.Log("Player's Turn.");
        }
    }
}
