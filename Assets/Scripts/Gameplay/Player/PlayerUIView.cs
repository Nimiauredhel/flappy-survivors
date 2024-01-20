using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using DG.Tweening;
using Gameplay.Weapons;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace Gameplay.Player
{
    public class PlayerUIView : MonoBehaviour
    {
        private const string LVL_TEXT_FORMAT = "LVL {0}";
        private const string COMBO_TEXT_FORMAT = "x{0}";
        private static readonly int BARFILL_HASH = Shader.PropertyToID("_BarFill");
        private static readonly int BARFILL_SMOOTHING_HASH = Shader.PropertyToID("_BarFillSmoothing");
        
        [SerializeField] private Image healthBar;
        [SerializeField] private Image xpBar;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private TextMeshProUGUI currentComboText;
        
        [SerializeField] private LayoutGroup weaponIconParent;
        [SerializeField] private WeaponUIView weaponIconPrefab;
        
        private List<WeaponUIView> weaponIcons = new List<WeaponUIView>(4);

        private Sequence healthTween = null;
        private Sequence xpTween = null;

        public void Initialize()
        {
            healthBar.material = new Material(healthBar.material);
            xpBar.material = new Material(xpBar.material);
            healthBar.material.SetFloat(BARFILL_SMOOTHING_HASH, 0.05f);
            xpBar.material.SetFloat(BARFILL_SMOOTHING_HASH, 0.05f);
        }

        public void UpdatePlayerHealthView(float percent)
        {
            if (healthTween != null)
            {
                healthTween.Kill();
            }
            
            healthTween = DOTween.Sequence();
            healthTween.Append(healthBar.material.DOFloat(percent, BARFILL_HASH, 0.25f).SetEase(Ease.OutCirc));
            healthTween.Join(healthBar.material.DOFloat(percent/4, BARFILL_SMOOTHING_HASH, 0.25f).SetEase(Ease.OutCirc));
        }
        
        public void UpdatePlayerXPView(float percent)
        {
            if (xpTween != null)
            {
                xpTween.Kill();
            }
            
            xpTween = DOTween.Sequence();
            xpTween.Append(xpBar.material.DOFloat(percent, BARFILL_HASH, 0.25f).SetEase(Ease.OutCirc));
            xpTween.Join(xpBar.material.DOFloat(percent/4, BARFILL_SMOOTHING_HASH, 0.25f).SetEase(Ease.OutCirc));
        }

        public void XPReverse()
        {
            xpBar.material.SetFloat(BARFILL_HASH, 1.0f);
            xpBar.material.SetFloat(BARFILL_SMOOTHING_HASH, 0.0f);
            
            if (xpTween != null)
            {
                xpTween.Kill();
            }
            
            xpTween = DOTween.Sequence();
            xpTween.Append(xpBar.material.DOFloat(0.0f, BARFILL_HASH, 10.0f).SetEase(Ease.OutCirc));
            xpTween.Join(xpBar.material.DOFloat(0.2f, BARFILL_SMOOTHING_HASH, 10.0f).SetEase(Ease.OutCirc));
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

            _ = RefreshWeaponsLayout();
            
            return newIcon;
        }

        private async Awaitable RefreshWeaponsLayout()
        {
            weaponIconParent.enabled = true;
            await Awaitable.WaitForSecondsAsync(1.0f);
            weaponIconParent.enabled = false;
        }
    }
}
