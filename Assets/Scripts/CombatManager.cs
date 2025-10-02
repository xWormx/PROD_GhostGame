using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // Cache
    private AudioSource audioSource;
    [SerializeField] private AudioClip playersTurnSound;
    [SerializeField] private AudioClip enemysTurnSound;
    [SerializeField] private AudioClip noteHitSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;

    // Songs and riffs
    [SerializeField] private Song[] songs;
    private int currentSongIndex = 0;
    private Song currentSong;
    private Riff[] currentSongRiffs;
    private int currentRiffIndex = 0;
    private Riff currentRiff;

    // Turn-based combat
    private const int beatsPerTurn = 8;
    private int beatCounter = 0;
    private const int enemyWaitTime = 2;
    private int enemyWaitCounter = 0;
    private bool bEnemyIsPlaying = false;
    private bool bIsPlayersTurn = false;
    private List<InputState> inputs = new();
    private int enemyNotesPlayed = 0;
    private int turn = 0;
    private bool bAlreadyFailed = false;
    private bool bAllSongsBeaten = false;

    // Input
    InputSystem_Actions inputActions;
    private bool bCanPlay = false;

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
    }

    public void OnBeat()
    {
        if (!songs.Any() || bAllSongsBeaten) return;

        if (BeatManager.Instance.beat == 3) // First attack to get the ball rolling
        {
            EnemysTurn();
        }

        if (bIsPlayersTurn)
        {
            if (beatCounter == 0) audioSource.PlayOneShot(playersTurnSound);

            beatCounter++;
            if (beatCounter > beatsPerTurn)
            {
                beatCounter = 0;
                EnemysTurn();
            }
        }

        if (!bEnemyIsPlaying) return;

        // Enemy's turn
        if (enemyWaitCounter == 1) audioSource.PlayOneShot(enemysTurnSound);
        enemyWaitCounter++;


        if (enemyWaitCounter <= enemyWaitTime) return;

        if (enemyNotesPlayed < currentRiff.GetNotes().Length)
        {
            //Debug.Log("Enemy played Note at Beat #" + BeatManager.Instance.beat);
            InputState noteDirection = currentRiff.GetNotes()[enemyNotesPlayed].GetDirection();

            SoundHandler.Instance.SetAudioState(noteDirection);
            SoundHandler.Instance.PlayRandomAudioClip();
            enemyNotesPlayed++;

            switch(noteDirection)
            {
                case InputState.InputState_Left:
                    {
                        Debug.Log("Enemy attack: Left");
                        break;
                    }
                case InputState.InputState_Right:
                    {
                        Debug.Log("Enemy attack: Right");
                        break;
                    }
                case InputState.InputState_Up:
                    {
                        Debug.Log("Enemy attack: Middle");
                        break;
                    }
            }
        }

        if (enemyNotesPlayed >= currentRiff.GetNotes().Length)
        {
            PlayersTurn();
        }
    }

    private void EnemysTurn()
    {
        //Debug.Log("Enemy's Turn.");

        ConsoleUtility.Instance.ClearConsole();

        if (turn > 0 && !bAlreadyFailed && !currentRiff.CheckSuccess())
        {
            audioSource.PlayOneShot(failSound);
            inputs.Clear();
            currentRiff.Reset();
        }

        turn++;
        bCanPlay = false;
        enemyWaitCounter = 0;
        inputs.Clear();
        bIsPlayersTurn = false;
        bEnemyIsPlaying = true;
    }

    private void PlayersTurn()
    {
        //Debug.Log("Player's Turn.");

        bCanPlay = true;
        bAlreadyFailed = false;
        enemyNotesPlayed = 0;
        bEnemyIsPlaying = false;
        inputs.Clear();
        bIsPlayersTurn = true;
        beatCounter = 0;
    }

    private void SelectNextRiff()
    {
        if (currentRiffIndex >= currentSongRiffs.Length)
        {
            currentSongIndex++;

            if (currentSongIndex >= songs.Length)
            {
                //Debug.Log("WINNER!!! ALL SONGS BEATEN!!!");
                bAllSongsBeaten = true;
                BeatManager.Instance.Stop();
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
        }
    }

    void OnMove(InputAction.CallbackContext context) // Read input
    {
        //Debug.Log("OnMove : input detected.");

        if (!bIsPlayersTurn || !bCanPlay) return;

        Vector2 inputVector = context.ReadValue<Vector2>();

        if (inputVector.x < 0)
        {
            Debug.Log("Player input: Left");
            inputs.Add(InputState.InputState_Left);
            CompareLatestInput(InputState.InputState_Left);
        }
        else if (inputVector.x > 0)
        {
            Debug.Log("Player input: Right");
            inputs.Add(InputState.InputState_Right);
            CompareLatestInput(InputState.InputState_Right);
        }
        else if (inputVector.y != 0)
        {
            Debug.Log("Player input: Middle");
            inputs.Add(InputState.InputState_Up);
            CompareLatestInput(InputState.InputState_Up);
        }
    }

    private void CompareLatestInput(InputState input)
    {
        if (!currentRiff.CompareInput(input))
        {
            audioSource.PlayOneShot(failSound);
            inputs.Clear();
            currentRiff.Reset();
            bCanPlay = false;
            bAlreadyFailed = true;
            EnemysTurn();
            return;
        }

        audioSource.PlayOneShot(noteHitSound);

        if (currentRiff.CheckSuccess())
        {
            //Debug.Log("RIFF PLAYED SUCCESSFULLY!!!");
            audioSource.PlayOneShot(successSound);

            turn = 0;
            currentRiffIndex++;
            SelectNextRiff();
            EnemysTurn();
        }
    }

}
