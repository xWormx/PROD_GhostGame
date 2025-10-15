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
    private int playerBeatCounter = 0;
    private const int enemyWaitTime = 2;
    private int enemyWaitCounter = 0;
    private bool bEnemyIsPlaying = false;
    private bool bIsPlayersTurn = false;
    private List<InputState> inputs = new();
    private int enemyNotesPlayed = 0;
    private int turn = 0;
    private bool bAlreadyFailed = false;
    private bool bAllSongsBeaten = false;
    private bool bInCombat;
    private int internalBeatCounter = 0;
    private InputState enemyJustPlayed = InputState.InputState_None;

    // Input
    InputSystem_Actions inputActions;
    private bool bCanPlay = false;

    // Tutorial
    private bool bIsTutorialLevel = true;

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

        Invoke("StartCombat", 2f);
    }

    public void StartCombat()
    {
        ResetCombat();
        bInCombat = true;
        BeatManager.Instance.Run();

        if (bIsTutorialLevel)
        {
            GoblinOfGuidance.Instance.PlayNextAudioClip();
        }
    }

    public void EndCombat()
    {
        ResetCombat();
        bInCombat = false;
        BeatManager.Instance.Stop();
        SoundHandler.Instance.SetActiveRandomMelody();
    }

    public void OnBeat()
    {
        if (!bInCombat || !songs.Any() || bAllSongsBeaten || GoblinOfGuidance.Instance.CheckIfPlaying()) return;

        if (!GoblinOfGuidance.Instance.CheckIfPlaying() && bIsTutorialLevel && BeatManager.Instance.beat > 1)
        {
            /*
            if (GoblinOfGuidance.Instance.CheckHasPlayedFinalClip())
            {
                GoblinOfGuidance.Instance.PlayRepatMyselfVoiceLine();
            }
            */

            GoblinOfGuidance.Instance.PlayNextAudioClip();
        }

        internalBeatCounter++;

        if (internalBeatCounter == 3) // First attack to get the ball rolling
        {
            EnemysTurn();
        }

        if (bIsPlayersTurn)
        {
            if (playerBeatCounter == 0) audioSource.PlayOneShot(playersTurnSound);

            if (bIsTutorialLevel && currentRiffIndex < 2 && playerBeatCounter == 2)
            {
                switch(enemyJustPlayed)
                {
                    case InputState.InputState_Left:
                        {
                            GoblinOfGuidance.Instance.PlayPressLeft();
                            break;
                        }
                    case InputState.InputState_Right:
                        {
                            GoblinOfGuidance.Instance.PlayPressRight();
                            break;
                        }
                }
            }

            playerBeatCounter++;
            if (playerBeatCounter > beatsPerTurn)
            {
                playerBeatCounter = 0;
                EnemysTurn();
            }
        }

        if (!bEnemyIsPlaying) return;

        // Enemy's turn
        // TODO (Calle): Här vill vi sätta vilken melodi som ska spelas förutsatt
        //               att en combat inte är slut
        if (enemyWaitCounter == 1) audioSource.PlayOneShot(enemysTurnSound);
        enemyWaitCounter++;


        if (enemyWaitCounter <= enemyWaitTime) return;

        if (enemyNotesPlayed < currentRiff.GetNotes().Length)
        {
            //Debug.Log("Enemy played Note at Beat #" + BeatManager.Instance.beat);
            InputState noteDirection = currentRiff.GetNotes()[enemyNotesPlayed].GetDirection();

            SoundHandler.Instance.SetAudioState(noteDirection);
            // TODO (Calle): Istället för att spela ett random audioClip vill vi spela upp nästa ton i 
            //               nuvarande melodin.
            SoundHandler.Instance.PlayeNextNoteInActiveMelody();

            //SoundHandler.Instance.PlayRandomAudioClip();
            enemyNotesPlayed++;

            switch (noteDirection)
            {
                case InputState.InputState_Left:
                    {
                        Debug.Log("Enemy attack: Left");
                        enemyJustPlayed = InputState.InputState_Left;
                        break;
                    }
                case InputState.InputState_Right:
                    {
                        Debug.Log("Enemy attack: Right");
                        enemyJustPlayed = InputState.InputState_Right;
                        break;
                    }
                case InputState.InputState_Up:
                    {
                        Debug.Log("Enemy attack: Middle");
                        enemyJustPlayed = InputState.InputState_Up;
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
        playerBeatCounter = 0;
    }

    private void SelectNextRiff()
    {
        if (currentRiffIndex >= currentSongRiffs.Length) // Out of riffs for current song, combat end
        {
            EndCombat();
            bIsTutorialLevel = false;
            currentSongIndex++;

            if (currentSongIndex >= songs.Length)
            {
                //Debug.Log("WINNER!!! ALL SONGS BEATEN!!!");
                bAllSongsBeaten = true;
            }
            else // Select next song
            {
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

    private void ResetCombat()
    {
        playerBeatCounter = 0;
        enemyWaitCounter = 0;
        enemyNotesPlayed = 0;
        turn = 0;
        bAlreadyFailed = false;
        bIsPlayersTurn = false;
        bEnemyIsPlaying = false;
        bCanPlay = false;
        inputs.Clear();
    }
}
