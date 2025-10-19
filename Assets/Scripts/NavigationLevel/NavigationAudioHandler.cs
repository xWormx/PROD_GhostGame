using UnityEngine;
using UnityEngine.Audio;

public class NavigationAudioHandler : MonoBehaviour
{

    public static NavigationAudioHandler Instance;

    [SerializeField] private AudioSource audioSrcDemonShred;
    [SerializeField] private AudioSource audioSrcDemonSpeech;
    [SerializeField] private AudioSource audioSrcBackgroundAmbience;
    [SerializeField] private AudioMixerSnapshot snapshotToCombat;
    [SerializeField] private AudioMixerSnapshot snapshotToNavigation;
    [SerializeField] private float snapshotTransitionTime;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        audioSrcBackgroundAmbience.Play();
    }

    public void PlayDemonShredClip(AudioClip clip, float volume, float pan)
    {
        if(!audioSrcDemonShred.isPlaying)
        { 
            audioSrcDemonShred.clip = clip;
            audioSrcDemonShred.Play();
        }

        audioSrcDemonShred.volume = volume;
        audioSrcDemonShred.panStereo= pan;

    }

    public void PlayDemonSpeechClip(AudioClip clip, float volume, float pan)
    {
        if(!audioSrcDemonSpeech.isPlaying)
        {
            audioSrcDemonSpeech.clip = clip;
            audioSrcDemonSpeech.Play();
        }

        audioSrcDemonSpeech.volume = volume;
        audioSrcDemonSpeech.panStereo = pan;
    }

    public void SnapshotToCombat()
    {
        snapshotToCombat.TransitionTo(snapshotTransitionTime);
    }

    public void SnapshotToNavigation()
    {
        snapshotToNavigation.TransitionTo(snapshotTransitionTime);
    }

}
