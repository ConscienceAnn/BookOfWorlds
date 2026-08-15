using UnityEngine;
using Zenject;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private float uiVolume = 0.7f;
    [SerializeField] private float ambientVolume = 0.5f;

    private bool isPaused = false;

    private void Awake()
    {
        // Важно: НЕ используем DontDestroyOnLoad здесь
        // AudioManager будет управляться через Zenject
        SetupAudioSources();
        Debug.Log("[AudioManager] Initialized!");
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = sfxVolume;
            sfxSource.spatialBlend = 0f;
        }

        if (uiSource == null)
        {
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.volume = uiVolume;
            uiSource.spatialBlend = 0f;
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.volume = ambientVolume;
            ambientSource.spatialBlend = 0f;
        }
    }

    private void OnEnable()
    {
        EventBus.OnPauseStateChanged += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        EventBus.OnPauseStateChanged -= OnPauseStateChanged;
    }

    private void OnPauseStateChanged(bool paused)
    {
        if (paused)
            PauseAllAudio();
        else
            ResumeAllAudio();
    }

    private void PauseAllAudio()
    {
        if (isPaused) return;
        isPaused = true;

        musicSource.Pause();
        sfxSource.Pause();
        uiSource.Pause();
        ambientSource.Pause();
    }

    private void ResumeAllAudio()
    {
        if (!isPaused) return;
        isPaused = false;

        musicSource.UnPause();
        sfxSource.UnPause();
        uiSource.UnPause();
        ambientSource.UnPause();
    }

    // ===== ОСНОВНЫЕ МЕТОДЫ =====

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlaySFX: clip is null!");
            return;
        }
        if (isPaused) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayUI(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlayUI: clip is null!");
            return;
        }
        if (isPaused) return;
        uiSource.PlayOneShot(clip);
    }

    public void PlayAmbient(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlayAmbient: clip is null!");
            return;
        }
        if (isPaused) return;
        ambientSource.Stop();
        ambientSource.clip = clip;
        ambientSource.Play();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.Stop();
        musicSource.clip = clip;

        if (!isPaused)
            musicSource.Play();
    }

    public void StopAmbient()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
            ambientSource.clip = null;
        }
    }

    public void StopAll()
    {
        musicSource.Stop();
        sfxSource.Stop();
        uiSource.Stop();
        ambientSource.Stop();

        musicSource.clip = null;
        sfxSource.clip = null;
        uiSource.clip = null;
        ambientSource.clip = null;
    }

    // ===== НАСТРОЙКИ ГРОМКОСТИ =====

    public void SetMusicVolume(float volume)
    {
        if (isPaused) return;
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        if (isPaused) return;
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public void SetUIVolume(float volume)
    {
        if (isPaused) return;
        uiVolume = Mathf.Clamp01(volume);
        uiSource.volume = uiVolume;
    }

    public void SetAmbientVolume(float volume)
    {
        if (isPaused) return;
        ambientVolume = Mathf.Clamp01(volume);
        ambientSource.volume = ambientVolume;
    }

    public bool IsPaused => isPaused;
}