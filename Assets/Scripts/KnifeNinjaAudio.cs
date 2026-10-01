using UnityEngine;

public class KnifeNinjaAudio : MonoBehaviour
{
    public static KnifeNinjaAudio Instance { get; private set; }

    [Header("Music")]
    public AudioClip levelTheme;

    [Header("SFX")]
    public AudioClip jumpClip;
    public AudioClip throwClip;
    public AudioClip hitClip;
    public AudioClip coinClip;
    public AudioClip hurtClip;
    public AudioClip gateClip;
    public AudioClip winClip;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = 0.34f;
        sfxSource.volume = 0.75f;

        if (levelTheme != null)
        {
            musicSource.clip = levelTheme;
            musicSource.Play();
        }
    }

    void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayJump() => Play(jumpClip, 0.65f);
    public void PlayThrow() => Play(throwClip, 0.7f);
    public void PlayHit() => Play(hitClip, 0.8f);
    public void PlayCoin() => Play(coinClip, 0.75f);
    public void PlayHurt() => Play(hurtClip, 0.75f);
    public void PlayGate() => Play(gateClip, 0.75f);
    public void PlayWin() => Play(winClip, 0.9f);
}
