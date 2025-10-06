using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNote", menuName = "SHREDEMPTION/Note", order = 1)]
public class Note : ScriptableObject
{
    [SerializeField] private InputState direction;
    private bool bSuccess = false;

    public bool CompareInput(InputState input)
    {
        if (input == direction)
        {
            bSuccess = true;

        }

        return bSuccess;
    }

    public bool GetSuccess()
    {
        return bSuccess;
    }

    public InputState GetDirection()
    {
        return direction;
    }

    public void Reset()
    {
        bSuccess = false;
    }
}
