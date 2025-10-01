using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    // Singleton pattern
    public static CombatManager Instance;
    private void Awake()
    {
        if (CombatManager.Instance != null && CombatManager.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            CombatManager.Instance = this;
        }
    }
    // End of Singleton pattern

    [SerializeField] private Song[] songs;
    private int currentSongIndex = 0;
    private Song currentSong;
    private Riff[] currentSongRiffs;
    private int currentRiffIndex = 0;
    private Riff currentRiff;
    private bool bIsBeingPlayed = false;

    private void Start()
    {
        if (!songs.Any())
        {
            Debug.LogError("CombatManager.songs is empty!");
            return;
        }

        currentSong = songs[currentSongIndex];
        currentSongRiffs = currentSong.GetRiffs();
        currentRiff = currentSongRiffs[currentRiffIndex];
    }

    private void Update()
    {
        if (!songs.Any()) return;

        if (BeatManager.Instance.beat % 8  == 0 && !bIsBeingPlayed)
        {
            Debug.Log("8 beats have passed...");

            bIsBeingPlayed = true;

            StartCoroutine(PlayRiff(currentRiff));
            currentRiffIndex++;
            
            if (currentRiffIndex >= currentSongRiffs.Length)
            {
                currentSongIndex++;
                
                if (currentSongIndex >= songs.Length)
                {
                    // All songs beat
                }
                else
                {
                    currentSong = songs[currentSongIndex];
                    currentSongRiffs = currentSong.GetRiffs();
                    currentRiffIndex = 0;
                    currentRiff = currentSongRiffs[currentRiffIndex];
                }
            }
            else
            {
                currentRiff = currentSongRiffs[currentRiffIndex];
            }
        }
    }

    private IEnumerator PlayRiff(Riff riff)
    {
        foreach (Note note in riff.GetNotes())
        {
            SoundHandler.Instance.SetAudioState(note.GetDirection());
            SoundHandler.Instance.PlayRandomAudioClip();
            yield return new WaitForSeconds(1f);
        }

        bIsBeingPlayed = false;
    }
}
