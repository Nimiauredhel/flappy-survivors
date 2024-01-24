using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Gameplay.Weapons
{
    public class WeaponUIView : MonoBehaviour
    {
        private static readonly int FLASH_HASH = Shader.PropertyToID("_FlashAmount");
        private static readonly int EMISSION_HASH = Shader.PropertyToID("_Emission");
        
        [SerializeField] private Image weaponIcon;
        [SerializeField] private Image weaponCooldownIndicator;

        private Sequence readyTweener = null;

        private bool ready = false;
        
        public void SetWeaponIconSprite(Sprite newIconSprite)
        {
            weaponIcon.sprite = newIconSprite;
            weaponIcon.material = new Material(weaponIcon.material);
        }

        public void UpdateChargeIndicator(float percent)
        {
            weaponCooldownIndicator.fillAmount = percent;
        }

        public void UpdateReadyIndicator(bool newReady)
        {
            if (newReady == ready) return;
            
            ready = newReady;

            if (readyTweener != null)
            {
                readyTweener.Kill();
            }

            if (newReady)
            {
                readyTweener = DOTween.Sequence();
                readyTweener.Append(weaponIcon.material.DOFloat(1.0f, EMISSION_HASH,0.05f));
                readyTweener.Append(weaponIcon.material.DOFloat(1.0f, FLASH_HASH,0.1f));
                readyTweener.Append(weaponIcon.material.DOFloat(0.0f, FLASH_HASH,0.35f));
            }
            else
            {
                weaponIcon.material.SetFloat(FLASH_HASH, 0.0f);
                readyTweener = DOTween.Sequence();
                readyTweener.Append(weaponIcon.material.DOFloat(0.0f, EMISSION_HASH,0.25f));
                
            }
        }
    }
}
