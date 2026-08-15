using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Game/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public SoundEntry[] sounds;
}

[Serializable]
public class SoundEntry
{
    public string id;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool isUISound = false;
    public bool isAmbient = false;
}