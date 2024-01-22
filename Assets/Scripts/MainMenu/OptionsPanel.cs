using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class OptionsPanel : MonoBehaviour
    {
        [SerializeField] private Toggle flashVFXToggle;
        [SerializeField] private Toggle explosionVFXToggle;
        [SerializeField] private Toggle contrastVFXToggle;

        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider masterVolumeSlider;

        

        private void OnEnable()
        {
            sfxVolumeSlider.SetValueWithoutNotify(Preferences.SFXVolume);
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
            musicVolumeSlider.SetValueWithoutNotify(Preferences.MusicVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            masterVolumeSlider.SetValueWithoutNotify(Preferences.MasterVolume);
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        
            flashVFXToggle.SetIsOnWithoutNotify(Preferences.FlashVFX);
            flashVFXToggle.onValueChanged.AddListener(SetFlashVFX);
            explosionVFXToggle.SetIsOnWithoutNotify(Preferences.ExplosionVFX);
            explosionVFXToggle.onValueChanged.AddListener(SetExplosionVFX);
            contrastVFXToggle.SetIsOnWithoutNotify(Preferences.ContrastVFX);
            contrastVFXToggle.onValueChanged.AddListener(SetContrastVFX);
        }

        public void SetFlashVFX(bool value)
        {
            Preferences.FlashVFX = value;
        }
    
        public void SetExplosionVFX(bool value)
        {
            Preferences.ExplosionVFX = value;
        }
    
        public void SetContrastVFX(bool value)
        {
            Preferences.ContrastVFX = value;
        }

        public void SetSfxVolume(float value)
        {
            Preferences.SFXVolume = value;
        }
    
        public void SetMusicVolume(float value)
        {
            Preferences.MusicVolume = value;
        }
    
        public void SetMasterVolume(float value)
        {
            Preferences.MasterVolume = value;
        }
    }
}
