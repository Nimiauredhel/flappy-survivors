using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Weapons;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gameplay.Player
{
    public class PlayerUIView : MonoBehaviour
    {
        private const string LVL_TEXT_FORMAT = "LVL {0}";
        private const string COMBO_TEXT_FORMAT = "x{0}";

        private static readonly string[] TIMER_FORMATS = new string[3]
        {
            @"mm\:ss",
            @"m\:ss",
            @"%s"
        };
        
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private TextMeshProUGUI currentComboText;
        
        [SerializeField] private LayoutGroup weaponIconParent;
        [SerializeField] private WeaponUIView weaponIconPrefab;
        
        private List<WeaponUIView> weaponIcons = new List<WeaponUIView>(4);

        public void UpdateTimerText(int timeInSeconds)
        {
            TimeSpan timerTimespan = TimeSpan.FromSeconds(timeInSeconds);
            int selectedFormat = timerTimespan.Minutes > 9 ? 0 : timerTimespan.Minutes > 0 ? 1 : 2;
            timerText.text = TimeSpan.FromSeconds(timeInSeconds).ToString(TIMER_FORMATS[selectedFormat]);
        }

        public void UpdatePlayerHealthView(float percent)
        {
            if (healthSlider.value == percent) return;
            healthSlider.value = percent;
        }
        
        public void UpdatePlayerXPView(float percent)
        {
            if (xpSlider.value == percent) return;
            xpSlider.value = percent;
        }

        public void UpdatePlayerCurrentLevelText(int currentLevel)
        {
            currentLevelText.text = string.Format(LVL_TEXT_FORMAT, currentLevel.ToString());
        }

        public void UpdatePlayerCurrentComboText(int currentCombo)
        {
            if (currentCombo > 1)
            {
                if (!currentComboText.enabled)
                {
                    currentComboText.enabled = true;
                }

                currentComboText.text = string.Format(COMBO_TEXT_FORMAT, currentCombo.ToString());
            }
            else
            {
                currentComboText.enabled = false;
            }

            
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

            StartCoroutine(RefreshWeaponsLayout());
            
            return newIcon;
        }

        public IEnumerator RefreshWeaponsLayout()
        {
            weaponIconParent.enabled = true;
            yield return null;
            weaponIconParent.enabled = false;
        }
    }
}
