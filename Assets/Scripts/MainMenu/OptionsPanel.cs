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

        private Bus sfxBus;
        private Bus musicBus;
        private Bus masterBus;

        private void OnEnable()
        {
            sfxBus = RuntimeManager.GetBus("bus:/Master/SFX");
            musicBus = RuntimeManager.GetBus("bus:/Master/Music");
            masterBus = RuntimeManager.GetBus("bus:/Master");

            float volume;
            sfxBus.getVolume(out volume);
            sfxVolumeSlider.SetValueWithoutNotify(volume);
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
            musicBus.getVolume(out volume);
            musicVolumeSlider.SetValueWithoutNotify(volume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            masterBus.getVolume(out volume);
            masterVolumeSlider.SetValueWithoutNotify(volume);
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
            sfxBus.setVolume(value);
        }
    
        public void SetMusicVolume(float value)
        {
            musicBus.setVolume(value);
        }
    
        public void SetMasterVolume(float value)
        {
            masterBus.setVolume(value);
        }
    }
}
