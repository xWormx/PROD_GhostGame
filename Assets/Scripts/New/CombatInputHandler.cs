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

    public InputDir Direction { get; }
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

    void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
    }

    void OnMove(InputAction.CallbackContext context) // Read input
    {
        Vector2 inputVector = context.ReadValue<Vector2>();

        if (!NewCombatManager.Instance.bInCombat)
        {
            return;
        }

        if (NewCombatManager.Instance.CurrentPhase == CombatPhase.EnemyTurn)
        {
            return;
        }

        if (inputVector.x < 0) // Left
        {
            combatInputs.Add(new CombatInput(InputDir.Left, AudioSettings.dspTime));
        }
        else if (inputVector.x > 0) // Right
        {
            combatInputs.Add(new CombatInput(InputDir.Right, AudioSettings.dspTime));
        }
        else if (inputVector.y != 0) // Up or Down
        {
            combatInputs.Add(new CombatInput(InputDir.Up, AudioSettings.dspTime));
        }
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
