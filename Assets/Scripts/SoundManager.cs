using UnityEngine;
using UnityEngine.Audio;

public class GlobalSoundManager : MonoBehaviour
{
    public static GlobalSoundManager Instance;
    public AudioSource bgMusicSource;
    public AudioMixer masterMixer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetVolumeFromLength(float currentHeight, float minHeight, float maxHeight)
    {
        float t = Mathf.InverseLerp(minHeight, maxHeight, currentHeight);
        float volumeDb = Mathf.Lerp(-40f, 0f, t);
        masterMixer.SetFloat("MasterVolume", volumeDb);
        float pitch = Mathf.Lerp(0.7f, 1.3f, t);
        bgMusicSource.pitch = pitch;
    }
}