using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Singleton

    [Header("Audio Clips")]
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private AudioClip excitedSound;
    [SerializeField] private AudioClip sleepySound;
    [SerializeField] private AudioClip backgroundMusic;

    private AudioSource movementAudio;
    private AudioSource effectAudio;
    private AudioSource musicAudio;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup audio sources
        movementAudio = gameObject.AddComponent<AudioSource>();
        movementAudio.loop = true;
        movementAudio.volume = 0.3f;

        effectAudio = gameObject.AddComponent<AudioSource>();
        effectAudio.playOnAwake = false;
        effectAudio.volume = 0.5f;

        musicAudio = gameObject.AddComponent<AudioSource>();
        musicAudio.loop = true;
        musicAudio.volume = 0.2f;

        // Start background music if available
        if (backgroundMusic != null)
        {
            musicAudio.clip = backgroundMusic;
            musicAudio.Play();
        }
    }

    public void PlayMovementSound(bool playing)
    {
        if (movementSound == null) return;

        if (playing && !movementAudio.isPlaying)
        {
            movementAudio.clip = movementSound;
            movementAudio.Play();
        }
        else if (!playing && movementAudio.isPlaying)
        {
            movementAudio.Stop();
        }
    }

    public void PlayInteractionSound()
    {
        if (interactionSound != null)
        {
            effectAudio.PlayOneShot(interactionSound);
        }
        else
        {
            // Fallback beep
            effectAudio.PlayOneShot(CreateBeep(440, 0.1f), 0.5f);
        }
    }

    public void PlayStateSound(BotState state)
    {
        AudioClip clip = null;
        switch (state)
        {
            case BotState.Excited:
                clip = excitedSound;
                if (clip == null) clip = CreateBeep(880, 0.2f);
                break;
            case BotState.Sleepy:
                clip = sleepySound;
                if (clip == null) clip = CreateBeep(220, 0.3f);
                break;
            // Add more states
        }

        if (clip != null)
        {
            effectAudio.PlayOneShot(clip, 0.4f);
        }
    }

    // Simple procedural sound generation
    private AudioClip CreateBeep(float frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * Mathf.Exp(-i * 5f / sampleCount); // Decay
        }

        AudioClip clip = AudioClip.Create("Beep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void SetMusicVolume(float volume)
    {
        musicAudio.volume = volume;
    }
}
