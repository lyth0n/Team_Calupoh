using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private AudioClip interactSound;
    [SerializeField] private AudioClip fanfareSound;
    [SerializeField] private AudioClip acceptSound;
    [SerializeField] private AudioClip cancelSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volumeScale);
        }
    }

    public void PlayJumpSound() => PlaySFX(jumpSound);
    public void PlayRunningSound() => PlaySFX(runningSound);
    public void PlayInteractSound() => PlaySFX(interactSound);
    public void PlayFanfareSound() => PlaySFX(fanfareSound);
    public void PlayAcceptSound() => PlaySFX(acceptSound);
    public void PlayCancelSound() => PlaySFX(cancelSound);
}