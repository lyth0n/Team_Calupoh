using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private AudioClip interactSound;
    [SerializeField] private AudioClip fanfareSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    public void PlayRunningSound()
    {
        if (audioSource != null && runningSound != null)
        {
            audioSource.PlayOneShot(runningSound);
        }
    }
    public void PlayInteractSound()
    {
        if (audioSource != null && interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }
    }

    public void PlayFanfareSound()
    {
        if (audioSource != null && fanfareSound != null)
        {
            audioSource.PlayOneShot(fanfareSound);
        }
    }
}