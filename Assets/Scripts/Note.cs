using UnityEngine;

public class Note : MonoBehaviour
{
    private InputState direction;
    private float time;
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
}
