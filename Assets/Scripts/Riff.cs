using UnityEngine;

[CreateAssetMenu(fileName = "NewRiff", menuName = "SHREDEMPTION/Riff", order = 1)]
public class Riff : ScriptableObject
{
    [SerializeField] private Note[] notes;

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

    public Note[] GetNotes()
    {
        return notes;
    }
}
