using UnityEngine;

public class Level : MonoBehaviour
{
    private Riff[] riffs;
    private float startTime;
    private int startBeat;

    private void Start()
    {
        startTime = Time.unscaledTime;
        startBeat = BeatManager.Instance.beat;
    }

    public bool CheckSuccess()
    {
        bool result = true;

        foreach (Riff riff in riffs)
        {
            if (riff.CheckSuccess() == false)
            {
                result = false;
            }
        }

        return result;
    }
}
