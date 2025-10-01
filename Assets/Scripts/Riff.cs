using UnityEngine;

[CreateAssetMenu(fileName = "NewRiff", menuName = "SHREDEMPTION/Riff", order = 1)]
public class Riff : ScriptableObject
{
    [SerializeField] private Note[] notes;
    private int currentNoteIndex = 0;

    public bool CompareInput(InputState input)
    {
        if (currentNoteIndex >= notes.Length)
            return false;

        if (notes[currentNoteIndex].CompareInput(input))
        {
            currentNoteIndex++;
            return true;
        }

        return false;
    }

    public bool CheckSuccess()
    {
        return currentNoteIndex >= notes.Length;
    }

    public void Reset()
    {
        currentNoteIndex = 0;
    }

    public Note[] GetNotes()
    {
        return notes;
    }
}
