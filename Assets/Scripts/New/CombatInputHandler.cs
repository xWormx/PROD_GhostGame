using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputDir
{
    Left,
    Right,
    Up,
    None
}

public class CombatInput
{
    public CombatInput(InputDir dir, double time)
    {
        Direction = dir;
        DSPTime = time;
    }

    public InputDir Direction;
    public double DSPTime { get; }
}

public class CombatInputHandler : MonoBehaviour
{
    public static CombatInputHandler Instance;

    private void Awake()
    {
        if (CombatInputHandler.Instance != null && CombatInputHandler.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            CombatInputHandler.Instance = this;
        }
    }

    private List<CombatInput> combatInputs = new();
    InputSystem_Actions inputActions;
    private List<CombatInput> enemyInputs;
    private AudioSource audioSource;
    [SerializeField] private AudioClip miss;

    void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;

        NewCombatManager.Instance.OnPlayerTurnStart.AddListener(OnPlayerTurnStart);

        audioSource = GetComponent<AudioSource>();
    }

    void OnMove(InputAction.CallbackContext context) // Read input
    {
        Vector2 inputVector = context.ReadValue<Vector2>();

        if (!NewCombatManager.Instance.bInCombat)
        {
            return;
        }

        if (NewCombatManager.Instance.CurrentPhase != CombatPhase.PlayerTurn)
        {
            return;
        }

        CombatInput input = new CombatInput(InputDir.None, AudioSettings.dspTime);

        if (inputVector.x < 0) // Left
        {
            input.Direction = InputDir.Left;
        }
        else if (inputVector.x > 0) // Right
        {
            input.Direction = InputDir.Right;
        }
        else if (inputVector.y != 0) // Up or Down
        {
            input.Direction = InputDir.Up;
        }

        combatInputs.Add(input);

        enemyInputs = Enemy.Instance.GetExpectedResponses();

        if (combatInputs.Count <= enemyInputs.Count)
        {
            InputAccuracy hit = CombatEvaluator.Instance.CompareInput(enemyInputs[combatInputs.Count - 1], combatInputs[combatInputs.Count - 1], false);
            List<NewNote> enemyNotes = Enemy.Instance.GetCurrentSongNotes().notes;

            switch(hit)
            {
                case InputAccuracy.Perfect:
                    {
                        audioSource.volume = 0.2f;
                        audioSource.PlayOneShot(enemyNotes[combatInputs.Count - 1].audioClip);
                        break;
                    }
                case InputAccuracy.Good:
                    {
                        audioSource.volume = 0.2f;
                        audioSource.PlayOneShot(enemyNotes[combatInputs.Count - 1].audioClip);
                        break;
                    }
                case InputAccuracy.Miss:
                    {
                        audioSource.volume = 1.0f;
                        audioSource.PlayOneShot(miss);
                        break;
                    }
            }
        }
    }

    private void OnPlayerTurnStart()
    {
        enemyInputs = Enemy.Instance.GetExpectedResponses();
    }

    public void ClearCombatInputs() => combatInputs.Clear();

    public List<CombatInput> GetCombatInputs()
    {
        return combatInputs;
    }

    public void AddNullInput()
    {
        combatInputs.Add(new CombatInput(InputDir.None, 0.0));
    }
}
