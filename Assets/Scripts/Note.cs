using UnityEngine;

[CreateAssetMenu(fileName = "NewNote", menuName = "SHREDEMPTION/Note", order = 1)]
public class Note : ScriptableObject
{
    [SerializeField] private InputState direction;

    public bool CompareInput(InputState input)
    {
        return input == direction;
    }

    public InputState GetDirection()
    {
        return direction;
    }
}
