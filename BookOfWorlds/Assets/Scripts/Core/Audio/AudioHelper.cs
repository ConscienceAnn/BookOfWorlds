using UnityEngine;
using Zenject;

public class AudioHelper
{
    private AudioManager _audioManager;
    private SoundLibrary _soundLibrary;
    private bool _isReady = false;

    public bool IsReady => _isReady;

    [Inject]
    public void Construct(AudioManager audioManager, SoundLibrary soundLibrary)
    {
        _audioManager = audioManager;
        _soundLibrary = soundLibrary;

        if (_audioManager != null && _soundLibrary != null)
        {
            _isReady = true;
            Debug.Log("[AudioHelper] Initialized and ready!");
        }
        else
        {
            Debug.LogWarning("[AudioHelper] Initialization failed!");
        }
    }

    public void PlaySound(string id)
    {
        if (!_isReady)
        {
            Debug.LogWarning($"[AudioHelper] Not ready! Sound '{id}' skipped.");
            return;
        }

        if (_audioManager == null || _soundLibrary == null)
        {
            Debug.LogWarning("[AudioHelper] AudioManager or SoundLibrary not initialized!");
            return;
        }

        foreach (var entry in _soundLibrary.sounds)
        {
            if (entry.id == id)
            {
                if (entry.clip == null)
                {
                    Debug.LogWarning($"[AudioHelper] Sound '{id}' has null clip!");
                    return;
                }

                if (entry.isUISound)
                    _audioManager.PlayUI(entry.clip);
                else if (entry.isAmbient)
                    _audioManager.PlayAmbient(entry.clip);
                else
                    _audioManager.PlaySFX(entry.clip);
                return;
            }
        }

        Debug.LogWarning($"[AudioHelper] Sound '{id}' not found!");
    }

    public void PlayUISound(string id)
    {
        if (!_isReady) return;
        if (_audioManager == null || _soundLibrary == null) return;

        foreach (var entry in _soundLibrary.sounds)
        {
            if (entry.id == id)
            {
                if (entry.clip == null) return;
                _audioManager.PlayUI(entry.clip);
                return;
            }
        }
    }

    public void PlaySFX(string id)
    {
        if (!_isReady) return;
        if (_audioManager == null || _soundLibrary == null) return;

        foreach (var entry in _soundLibrary.sounds)
        {
            if (entry.id == id)
            {
                if (entry.clip == null) return;
                _audioManager.PlaySFX(entry.clip);
                return;
            }
        }
    }

    public void StopAmbient()
    {
        if (!_isReady) return;
        _audioManager?.StopAmbient();
    }

    public void StopAll()
    {
        if (!_isReady) return;
        _audioManager?.StopAll();
    }

    public bool IsPaused => _isReady && _audioManager != null && _audioManager.IsPaused;
}