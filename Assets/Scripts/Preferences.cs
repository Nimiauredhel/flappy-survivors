using UnityEngine;

public static class Preferences
{
    private const string FLASH_VFX = "FlashVFX";
    private const string EXPLOSION_VFX = "ExplosionVFX";
    private const string CONTRAST_VFX = "ContrastVFX";
    
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
    
    private static bool initialized = false;
    
    private static bool flashVFX;
    private static bool explosionVFX;
    private static bool contrastVFX;

    private static void Initialize()
    {
        flashVFX = PlayerPrefs.GetInt("FlashVFX", 1) == 1 ? true : false;
        
        initialized = true;
    }
}
