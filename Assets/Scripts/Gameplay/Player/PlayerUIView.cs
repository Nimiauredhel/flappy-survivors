using System.Collections.Generic;
using Gameplay.Weapons;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

namespace Gameplay.Player
{
    public class PlayerUIView : MonoBehaviour
    {
        private const string LVL_TEXT_FORMAT = "LVL {0}";
        
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        
        [SerializeField] private LayoutGroup weaponIconParent;
        [SerializeField] private WeaponUIView weaponIconPrefab;

        private List<WeaponUIView> weaponIcons = new List<WeaponUIView>(4);
        
        public void UpdatePlayerHealthView(float percent)
        {
            healthSlider.value = percent;
        }
        
        public void UpdatePlayerXPView(float percent)
        {
            xpSlider.value = percent;
        }

        public void UpdatePlayerCurrentLevelText(int currentLevel)
        {
            currentLevelText.text = string.Format(LVL_TEXT_FORMAT, currentLevel.ToString());
        }

        public void UpdateWeaponCooldownView(int index, float percent)
        {
            if (weaponIcons == null || weaponIcons.Count <= index) return;

            weaponIcons[index].UpdateCooldownIndicator(percent);
        }

        public WeaponUIView AddOrReplaceWeaponIcon(Sprite iconSprite, WeaponUIView toReplace = null)
        {
            weaponIcons ??= new List<WeaponUIView>(4);

            WeaponUIView newIcon = Instantiate(weaponIconPrefab, weaponIconParent.transform);
            newIcon.SetWeaponIconSprite(iconSprite);
            newIcon.UpdateCooldownIndicator(0.0f);

            if (toReplace == null || !weaponIcons.Contains(toReplace))
            {
                weaponIcons.Add(newIcon);
            }
            else
            {
                int index = weaponIcons.IndexOf(toReplace);
                Destroy(toReplace.gameObject);
                weaponIcons[index] = newIcon;
            }

            return newIcon;
        }

        public async void RefreshWeaponsLayout()
        {
            weaponIconParent.enabled = true;
            await Task.Yield();
            weaponIconParent.enabled = false;
        }
    }
}
