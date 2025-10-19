using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor.Actions;
using UnityEngine.Rendering;
using UnityEngine.Audio;

/*
 *  TODO (Calle):   1. g�r tv�, en med vanlig och en med inst�llning n�r man tryckt p� en knapp.
 * 
 * 
 */

public class IntroHandler : MonoBehaviour
{
    [SerializeField] private AudioSource pressAnyKey;
    [SerializeField] private AudioSource introThemeWelcome;
    [SerializeField] private AudioSource introThemeLoop;
    [SerializeField] private AudioSource buttonPressFeedback;
    
    [SerializeField] private AudioMixerSnapshot snapShotFadeAtLevelChange;

    [SerializeField] private float startAfterSecondsPressAnyKey;
    [SerializeField] private float repeatAfterSecondsPressAnyKey;

    [SerializeField] private bool skipWelcomeIntro;

    bool welcomeEnded  = false;
    bool changingScene = false;
    bool pressAnyFuckingKeyPlayedOnce = false;
    bool pressAnyFuckingKeyStarted = false;

    void Start()
    {
        if (skipWelcomeIntro)
        {
            welcomeEnded = true;
        }
        else
        {
            introThemeWelcome.Play();
        }
    }

    void Update()
    {
        if ((IntroWelcomeClipStoppedPlaying() && IntroWelcomeEnded()))
        {
            StartIntroThemeLoop();
            StartRepeatingPressAnyKey();
        }
        else if(skipWelcomeIntro)
        {
            skipWelcomeIntro = false;
            StartIntroThemeLoop();
            StartRepeatingPressAnyKey();
        }

        if (IntroWelcomeEnded() && !ChangingScene() && PressAnyFuckingKeyClipPlayedOnce())
        {
            if (Input.anyKeyDown)
            {
                StartCoroutine(PlayFeedBackAndChangeScene());
            }
        }
    }

    bool IntroWelcomeClipStoppedPlaying()
    {
        bool result = false;

        if (welcomeEnded)
            return result;

        if(!introThemeWelcome.isPlaying)
        {
            result = true;
            welcomeEnded = true;
        }
        else
        {
            result = false;
        }
        
        return result;
    }

    bool ChangingScene()
    {
        return changingScene;
    }

    bool IntroWelcomeEnded()
    {
        return welcomeEnded;
    }

    void PressAnyFuckingKeyClip()
    {
        pressAnyKey.Play();
        pressAnyFuckingKeyStarted = true;
    }

    bool PressAnyFuckingKeyClipPlayedOnce()
    {
        bool result = false;

        if (!pressAnyFuckingKeyPlayedOnce && pressAnyFuckingKeyStarted)
        {
            if (!pressAnyKey.isPlaying)
            {
                result = true;
                pressAnyFuckingKeyPlayedOnce = true;
            }
        }
        else
        {
            result = true;
        }

            return result;
    }

    void StartIntroThemeLoop()
    {
        Debug.Log("Loop play");
        introThemeLoop.Play();
    }

    void StartRepeatingPressAnyKey()
    {
        // B�rjar spela och repetera att man ska trycka p� en knapp f�r att starta spelet
        InvokeRepeating("PressAnyFuckingKeyClip", startAfterSecondsPressAnyKey, repeatAfterSecondsPressAnyKey);
    }

    private IEnumerator PlayFeedBackAndChangeScene()
    {
        pressAnyKey.Stop();
        snapShotFadeAtLevelChange.TransitionTo(1.0f);
        changingScene = true;
        buttonPressFeedback.Play();
        yield return new WaitForSeconds(1.4f);
        SceneManager.LoadScene(1);
    }
}
