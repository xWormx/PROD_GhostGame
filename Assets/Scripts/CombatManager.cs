using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
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

    // Input
    InputSystem_Actions inputActions;

    private void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;

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
            CheckRiffSuccess();
            EnemysTurn();
        }
    }

    private void EnemysTurn()
    {
        Debug.Log("Enemy's Turn.");

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
        //currentRiffIndex++;
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
        Debug.Log("Player's Turn.");
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
    }

    private void CheckRiffSuccess()
    {
        Debug.Log("CheckRiffSuccess()");

        if (!inputs.Any())
        {
            Debug.Log("CheckRiffSuccess()::No input found!");
            return;
        }

        if (currentRiff.CheckSuccess() == true)
        {
            Debug.Log("CheckRiffSuccess()::Riff already marked as completed.");
        }

        Debug.Log("CheckRiffSuccess()::Inputs found, and riff not yet completed.");

        // Kontrollera om sekvensen av inputs överensstämmer med noterna som ska spelas
        // Noterna markeras automatiskt som avklarade om de stämmer
        for (int i = 0; i < notesToMatch.Length; i++)
        {
            if (!inputs.Any())
            {
                Debug.Log("CheckRiffSuccess()::No input found!");
                return;
            }

            if (!notesToMatch[i].CompareInput(inputs[i]))
            {
                Debug.Log("CheckRiffSuccess()::Wrong input found!");
                inputs.Clear();
                return;
            }

            // Riff avklarat! Spela upp coolt ljud
            // TO-DO: LÄGG TILL EN BOOL SÅ ATT MAN BARA KLARAR RIFFET EN GÅNG
            Debug.Log("CheckRiffSuccess()::NOTE PLAYED SUCCESSFULLY!");
        }

        if (currentRiff.CheckSuccess())
        {
            Debug.Log("CheckRiffSuccess()::RIFF PLAYED SUCCESSFULLY!!!");
        }
    }
}
