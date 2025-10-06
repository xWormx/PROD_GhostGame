//Detta Script är gjort av animeraren Carl Roos, så ChatGPT med andra ord då han inte kan koda så bra. 
//Används enbart i introt

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Scene-based music sequencer with gated input:
/// 1) Plays an intro clip when a chosen scene starts
/// 2) Crossfades into a second clip
/// 3) When the second clip finishes, starts a third clip that loops
/// 4) "Press any key" to advance is ONLY allowed after the intro has fully finished OR after a fallback timeout (e.g. 16s)
/// 5) Press 'Q' at ANY time to skip straight to the next scene immediately (test shortcut)
/// </summary>
public class SceneMusicSequencer : MonoBehaviour
{
    private static SceneMusicSequencer persistantInstance;

    [Header("When to run")]
    [Tooltip("IntroScene")]
    [SerializeField] private string targetSceneName = "";

    [Header("Clips")]
    [SerializeField] private AudioClip introClip;           // First clip
    [SerializeField] private AudioClip crossfadeToClip;     // Second clip (crossfaded in)
    [SerializeField] private AudioClip finalLoopClip;       // Third clip (loops)
    [SerializeField] private AudioClip buttonPressFeedback;       // Third clip (loops)

    [Header("Levels & Timing")]
    [Tooltip("Master volume for all sources (0–1).")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;

    [Tooltip("Seconds to crossfade from the intro to the second clip.")]
    [Min(0f)][SerializeField] private float crossfadeDuration = 3.5f;

    [Tooltip("Optional fade-in time for the very first clip.")]
    [Min(0f)][SerializeField] private float initialFadeIn = 0f;

    [Header("Proceed / Skip Settings")]
    [Tooltip("Seconds after which 'press any key' becomes available (in case intro is long).")]
    [Min(0f)][SerializeField] private float proceedFallbackSeconds = 16f;

    [Tooltip("If set, an empty nextSceneName will load the next scene in build index.")]
    [SerializeField] private bool useNextBuildIndexIfNameEmpty = true;

    [Tooltip("Explicit next scene name. Leave empty to use next build index if allowed.")]
    [SerializeField] private string nextSceneName = "";

    private AudioSource _a; // current/first
    private AudioSource _b; // next/second

    
    private bool _started;
    private bool _canProceed;      // becomes true after intro end OR fallback time
    private bool _sceneLoading;    // prevent double loads
    private float _sequenceStartTime;

    private void Awake()
    {
        _a = gameObject.AddComponent<AudioSource>();
        _b = gameObject.AddComponent<AudioSource>();

       foreach (var src in new[] { _a, _b })
       {
           src.playOnAwake = false;
           src.loop = false;
           src.volume = 0f;
           src.spatialBlend = 0f; // 2D
       }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            TryStartSequenceForActiveScene();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.IsNullOrEmpty(targetSceneName) && scene.name == targetSceneName)
        {
            TryStartSequenceForActiveScene();
        }
    }

    private void TryStartSequenceForActiveScene()
    {
        if (_started) return;
        if (introClip == null || crossfadeToClip == null || finalLoopClip == null)
        {
            Debug.LogWarning("SceneMusicSequencer: One or more AudioClips are missing.");
            return;
        }

        _started = true;
        _sequenceStartTime = Time.time;
        StartCoroutine(PlaySequence());
        StartCoroutine(UnlockProceedWhenReady());
    }

    /// <summary>
    /// OPTIONAL public API to trigger manually.
    /// </summary>
    public void StartNow()
    {
        if (!_started) TryStartSequenceForActiveScene();
    }

    private void Update()
    {
        // Test shortcut: 'Q' can always skip immediately
        if (Input.GetKeyDown(KeyCode.Q))
        {
            LoadNextSceneImmediate();
            return;
        }

        // Normal proceed: any key AFTER unlock
        if (_canProceed && !_sceneLoading && Input.anyKeyDown)
        {
            LoadNextSceneImmediate();
        }
    }


    private IEnumerator PlayFeedbackAndChangeScene()
    {
        AudioSource.PlayClipAtPoint(buttonPressFeedback, Camera.main.transform.position);
        yield return new WaitForSeconds(buttonPressFeedback.length);
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator PlaySequence()
    {
        // 1) Play intro
        _a.clip = introClip;
        _a.time = 0f;
        _a.volume = 0f;
        _a.loop = false;
        _a.Play();

        if (initialFadeIn > 0f)
            yield return FadeVolume(_a, 0f, masterVolume, initialFadeIn);
        else
            _a.volume = masterVolume;

        // Wait until it's time to start crossfading (just before the intro ends)
        float waitBeforeCrossfade = Mathf.Max(0f, introClip.length - crossfadeDuration);
        yield return new WaitForSeconds(waitBeforeCrossfade);

        // 2) Crossfade to second clip
        _b.clip = crossfadeToClip;
        _b.time = 0f;
        _b.volume = 0f;
        _b.loop = false;
        _b.Play();

        yield return Crossfade(_a, _b, crossfadeDuration, masterVolume);

        _a.Stop();
        _a.volume = 0f;

        // 3) Wait for second clip to fully end (minus the crossfade time already elapsed)
        float remainingSecond = Mathf.Max(0f, crossfadeToClip.length - crossfadeDuration);
        yield return new WaitForSeconds(remainingSecond);

        // 4) Switch to final looping clip
        _a.clip = finalLoopClip;
        _a.loop = true;
        _a.time = 0f;
        _a.volume = 0f;
        _a.Play();

        // Gentle fade-in for safety
        yield return FadeVolume(_a, 0f, masterVolume, 0.1f);

        _b.Stop();
        _b.volume = 0f;
        _b.clip = null;
    }

    /// <summary>
    /// Unlocks the proceed gate either when the intro has COMPLETELY finished
    /// or when the fallback timeout has passed (whichever comes first).
    /// </summary>
    private IEnumerator UnlockProceedWhenReady()
    {
        // Time until intro end (not just crossfade start)
        float timeUntilIntroEnd = Mathf.Max(0f, introClip.length);
        float timeUntilFallback = Mathf.Max(0f, proceedFallbackSeconds);

        // Wait for whichever happens first
        float elapsed;
        while (true)
        {
            if (_sceneLoading) yield break;
            elapsed = Time.time - _sequenceStartTime;
            if (elapsed >= timeUntilIntroEnd || elapsed >= timeUntilFallback)
            {
                _canProceed = true;
                yield break;
            }
            yield return null;
        }
    }

    private void LoadNextSceneImmediate()
    {
        if (_sceneLoading) return;
        _sceneLoading = true;

        // Optional: stop audio cleanly
        if (_a != null) _a.Stop();
        if (_b != null) _b.Stop();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            StartCoroutine(PlayFeedbackAndChangeScene());
        }
        else if (useNextBuildIndexIfNameEmpty)
        {
            int current = SceneManager.GetActiveScene().buildIndex;
            int next = current + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(next);
            }
            else
            {
                Debug.LogWarning("SceneMusicSequencer: Next build index is out of range.");
            }
        }
        else
        {
            Debug.LogWarning("SceneMusicSequencer: No next scene specified.");
        }
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float duration, float targetVol)
    {
        if (duration <= 0f)
        {
            from.volume = 0f;
            to.volume = targetVol;
            yield break;
        }

        float t = 0f;
        float startFrom = from.volume;
        float startTo = to.volume;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            from.volume = Mathf.Lerp(startFrom, 0f, k);
            to.volume = Mathf.Lerp(startTo, targetVol, k);
            yield return null;
        }

        from.volume = 0f;
        to.volume = targetVol;
    }

    private IEnumerator FadeVolume(AudioSource src, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            src.volume = to;
            yield break;
        }

        float t = 0f;
        src.volume = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            src.volume = Mathf.Lerp(from, to, k);
            yield return null;
        }
        src.volume = to;
    }
}
