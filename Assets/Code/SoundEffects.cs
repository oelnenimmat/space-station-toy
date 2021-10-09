using UnityEngine;

public enum SoundEffect { Add, Remove, Error }

[RequireComponent(typeof(AudioSource))]
public class SoundEffects : MonoBehaviour
{
    // Todo(Leo): Audio things definetly not here :)
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
    public float sfxVolume = 0.8f;
    public float errorVolume = 0.5f;
    public AudioClip addSfx;
    public AudioClip removeSfx;
    public AudioClip errorSfx;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(SoundEffect s)
    {
    	switch(s)
    	{
    		case SoundEffect.Add:
				audioSource.pitch = Random.Range(minPitch, maxPitch);
				audioSource.volume = sfxVolume;    
				audioSource.PlayOneShot(addSfx);
    			break;

    		case SoundEffect.Remove:
				audioSource.pitch = Random.Range(minPitch, maxPitch);
				audioSource.volume = sfxVolume;
				audioSource.PlayOneShot(removeSfx);
    			break;

    		case SoundEffect.Error:
	            audioSource.pitch = 1;
            	audioSource.volume = errorVolume;
            	audioSource.PlayOneShot(errorSfx);
    			break;
    	}
    }
}