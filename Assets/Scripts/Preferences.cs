using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public static class Preferences
{
    private const string FLASH_VFX = "FlashVFX";
    private const string EXPLOSION_VFX = "ExplosionVFX";
    private const string CONTRAST_VFX = "ContrastVFX";
    
    private const string SFX_VOLUME = "SFXVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string MASTER_VOLUME = "MasterVolume";
    
    public static bool FlashVFX
    {
        get
        {
            if (!initialized) Initialize();
            return flashVFX;
        }
        set
        {
            if (!initialized) Initialize();
            flashVFX = value;
            PlayerPrefs.SetInt(FLASH_VFX, value ? 1 : 0);
        }
    }
    
    public static bool ExplosionVFX
    {
        get
        {
            if (!initialized) Initialize();
            return explosionVFX;
        }
        set
        {
            if (!initialized) Initialize();
            explosionVFX = value;
            PlayerPrefs.SetInt(EXPLOSION_VFX, value ? 1 : 0);
        }
    }
    
    public static bool ContrastVFX
    {
        get
        {
            if (!initialized) Initialize();
            return contrastVFX;
        }
        set
        {
            if (!initialized) Initialize();
            contrastVFX = value;
            PlayerPrefs.SetInt(CONTRAST_VFX, value ? 1 : 0);
        }
    }
    
    public static float SFXVolume
    {
        get
        {
            if (!initialized) Initialize();
            return sfxVolume;
        }
        set
        {
            if (!initialized) Initialize();
            sfxVolume = value;
            sfxBus.setVolume(value);
            PlayerPrefs.SetFloat(SFX_VOLUME, value);
        }
    }
    
    public static float MusicVolume
    {
        get
        {
            if (!initialized) Initialize();
            return musicVolume;
        }
        set
        {
            if (!initialized) Initialize();
            musicVolume = value;
            musicBus.setVolume(value);
            PlayerPrefs.SetFloat(MUSIC_VOLUME, value);
        }
    }
    
    public static float MasterVolume
    {
        get
        {
            if (!initialized) Initialize();
            return masterVolume;
        }
        set
        {
            if (!initialized) Initialize();
            masterVolume = value;
            masterBus.setVolume(value);
            PlayerPrefs.SetFloat(MASTER_VOLUME, value);
        }
    }
    
    private static bool initialized = false;
    
    private static bool flashVFX;
    private static bool explosionVFX;
    private static bool contrastVFX;

    private static float sfxVolume;
    private static float musicVolume;
    private static float masterVolume;
    
    private static Bus sfxBus;
    private static Bus musicBus;
    private static Bus masterBus;

    public static void Initialize()
    {
        flashVFX = PlayerPrefs.GetInt(FLASH_VFX, 1) == 1 ? true : false;
        explosionVFX = PlayerPrefs.GetInt(EXPLOSION_VFX, 1) == 1 ? true : false;
        contrastVFX = PlayerPrefs.GetInt(CONTRAST_VFX, 1) == 1 ? true : false;
        
        sfxBus = RuntimeManager.GetBus("bus:/Master/SFX");
        musicBus = RuntimeManager.GetBus("bus:/Master/Music");
        masterBus = RuntimeManager.GetBus("bus:/Master");

        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME, 1.0f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME, 1.0f);
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME, 1.0f);
        
        sfxBus.setVolume(sfxVolume);
        musicBus.setVolume(musicVolume);
        masterBus.setVolume(masterVolume);
        
        initialized = true;
    }
}
