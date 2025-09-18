using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public int beat = 1;

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
        while (true)
        {
            currentAudioClip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.PlayOneShot(currentAudioClip);
            yield return new WaitForSeconds(currentAudioClip.length);
            beat++;
        }
    }
}
