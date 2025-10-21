using UnityEngine;

public class DemonBattlePoint : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClipShreds;
    [SerializeField] private AudioClip[] audioClipSpeech;

    bool bPlayerInAudioRange;

    private SphereCollider col;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        col = GetComponent<SphereCollider>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            int randomShred = Random.Range(0, audioClipShreds.Length);
            int randomSpeech = Random.Range(0, audioClipSpeech.Length);

            Vector3 center = transform.TransformPoint(col.center);
            float worldRadius = col.radius * Mathf.Max(
                transform.lossyScale.x,
                transform.lossyScale.y,
                transform.lossyScale.z
            );

            float distanceToCenter = Vector3.Distance(other.transform.position, center);
            float normalized = Mathf.Clamp01(distanceToCenter / worldRadius);
            float volume = 1f - normalized;

            Vector3 relativePos = other.transform.position - center;
            float pan = Mathf.Clamp(relativePos.x / worldRadius, -1f, 1f);
            pan *= -1.0f;

            NavigationAudioHandler.Instance.PlayDemonShredClip(audioClipShreds[randomShred], volume, pan);
            NavigationAudioHandler.Instance.PlayDemonSpeechClip(audioClipSpeech[randomSpeech], volume, pan);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            GameLevelHandler.Instance.SetCurrentEncounteredDemon(gameObject);
        }
    }
}
