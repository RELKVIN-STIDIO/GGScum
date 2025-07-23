using UnityEngine;

[CreateAssetMenu(menuName = "GG, Scum/Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    public string soundName;
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    public bool spatial = false;
}
