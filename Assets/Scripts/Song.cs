using UnityEngine;

[CreateAssetMenu(fileName = "NewSong", menuName = "SHREDEMPTION/Song", order = 1)]
public class Song : ScriptableObject
{
    [SerializeField] private Riff[] riffs;
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

    public Riff[] GetRiffs()
    {
        return riffs;
    }

    public void Reset()
    {
        foreach (Riff riff in riffs)
        {
            riff.Reset();
        }
    }
}
