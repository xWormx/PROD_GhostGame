using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    // Singleton pattern
    public static BeatManager Instance;
    private void Awake()
    {
        if (BeatManager.Instance != null && BeatManager.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            BeatManager.Instance = this;
        }
    }
    // End of Singleton pattern

    public int beat = 1;
    private bool looping = true;
    [SerializeField] private AudioClip[] audioClips;
    private AudioClip currentAudioClip;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(LoopAudioClips());
    }

    private IEnumerator LoopAudioClips()
    {
        while (looping)
        {
            CombatManager.Instance.OnBeat();
            currentAudioClip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.PlayOneShot(currentAudioClip);
            yield return new WaitForSeconds(currentAudioClip.length);
            beat++;
        }
    }

    public void Stop()
    {
        looping = false;
    }
}
