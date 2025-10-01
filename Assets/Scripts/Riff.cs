using UnityEngine;

public class Riff : MonoBehaviour
{
    private Note[] notes;

    public bool CheckSuccess()
    {
        bool result = true;

        foreach (Note note in notes)
        {
            if (note.GetSuccess() == false)
            {
                result = false;
            }
        }

        return result;
    }
}
