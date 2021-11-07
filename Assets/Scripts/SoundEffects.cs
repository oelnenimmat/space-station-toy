using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class SoundEffect
{
    public AudioClip clip;

    public bool randomPitch = false;
    public float minPitch   = 1;
    public float maxPitch   = 1;
    public float volume     = 1;
}

[RequireComponent(typeof(AudioSource))]
public class SoundEffects : MonoBehaviour
{
    public SoundEffect add;
    public SoundEffect remove;
    public SoundEffect ui;
    public SoundEffect error;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(SoundEffect s)
    {
        Assert.AreNotEqual(s, null);

        audioSource.pitch = s.randomPitch ? Random.Range(s.minPitch, s.maxPitch) : 1;
        audioSource.volume = s.volume;
        audioSource.PlayOneShot(s.clip);
    }
}