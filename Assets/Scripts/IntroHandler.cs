using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class IntroHandler : MonoBehaviour
{

    /*
     *  1. Spela Introt fram till "SHREDEMPTION"
     *  
     *  2. B�rja loopa introThemeLoop
     *  
     *  3. Efter en tid, spela upp pressAnyKey
     *  
     */

    [SerializeField] private AudioSource pressAnyKey;
    [SerializeField] private AudioSource introThemeWelcome;
    [SerializeField] private AudioSource introThemeLoop;
    [SerializeField] private AudioSource buttonPressFeedback;

    [SerializeField] private float startAfterSecondsPressAnyKey;
    [SerializeField] private float repeatAfterSecondsPressAnyKey;

    [SerializeField] private bool skipWelcomeIntro;

    bool welcomeEnded  = false;
    bool changingScene = false;

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


        // TODO (Calle): Skrive om IntroWelcomeHasEnded och welcomeEnded boolen s� att det blir mindre tvetydgit.
        //               kanske separera i mindr funktioner som s�ger om welcome spelar och welcome ended
        if (IntroWelcomeHasEnded())
        {
            PlayIntroThemeLoop();
            StartRepeatingPressAnyKey();

   
        }

        if(welcomeEnded && !changingScene)
        {
            if (Input.anyKeyDown)
            {
                StartCoroutine(PlayFeedBackAndChangeScene());
            }
        }
   
    }

    bool IntroWelcomeHasEnded()
    {
        bool result = false;

        if ((!introThemeWelcome.isPlaying && !welcomeEnded) || skipWelcomeIntro)
        {
            result = true;
            welcomeEnded = true;
            skipWelcomeIntro = false;
        }
        else
        {
            result = false;
        }

        return result;
    }

    void PressAnyFuckingKeyClip()
    {
        pressAnyKey.Play();
    }

    void PlayIntroThemeLoop()
    {
        introThemeLoop.Play();
    }

    void StartRepeatingPressAnyKey()
    {
        // B�rjar spela och repetera att man ska trycka p� en knapp f�r att starta spelet
        InvokeRepeating("PressAnyFuckingKeyClip", startAfterSecondsPressAnyKey, repeatAfterSecondsPressAnyKey);
    }

    private IEnumerator PlayFeedBackAndChangeScene()
    {
        changingScene = true;
        buttonPressFeedback.Play();
        yield return new WaitForSeconds(1.4f);
        SceneManager.LoadScene(1);
    }
}
