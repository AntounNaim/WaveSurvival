using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip ambientSound;
    
    [Header("Weapon Sounds")]
    public AudioClip arShootSound;
    public AudioClip rifleShootSound;
    public AudioClip pistolShootSound;


    [Header("Enemy Sounds")]
    public AudioClip enemyDeathSound;
    public AudioClip enemyExploderSound;
    public AudioClip enemyHitSound;
    
    [Header("Player Sounds")]
    public AudioClip playerHurtSound;
    
    [Header("Pickup Sounds")]
    public AudioClip pickupSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float ambientVolume = 0.3f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource sfxSource;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Create audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        ambientSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        // Configure sources
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }
    
    private void Start()
    {
        PlayMusic(backgroundMusic);
        PlayAmbient(ambientSound);
        ApplyVolumes();
    }
    
    public void PlayMusic(AudioClip music)
    {
        if (music == null) return;
        
        musicSource.clip = music;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }
    
    public void PlayAmbient(AudioClip ambient)
    {
        if (ambient == null) return;
        
        ambientSource.clip = ambient;
        ambientSource.volume = ambientVolume * masterVolume;
        ambientSource.Play();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }
    
    private void ApplyVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        
        if (ambientSource != null)
            ambientSource.volume = ambientVolume * masterVolume;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
    }
    
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
        }
    }
}