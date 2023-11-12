using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Weapons
{
    public class WeaponUIView : MonoBehaviour
    {

        [SerializeField] private Image weaponIcon;
        [SerializeField] private Image weaponCooldownIndicator;

        public void SetWeaponIconSprite(Sprite newIconSprite)
        {
            weaponIcon.sprite = newIconSprite;
        }

        public void UpdateCooldownIndicator(float percent)
        {
            weaponCooldownIndicator.fillAmount = percent;
        }
    }
}
