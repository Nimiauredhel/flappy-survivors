using System;
using System.Collections.Generic;
using DG.Tweening;
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
        private static readonly int DO_BARFILL_HASH = Shader.PropertyToID("_DoBarFill");
        private static readonly int GRADIENT_AMOUNT_HASH = Shader.PropertyToID("_GradientAmount");
        private static readonly int BARFILL_HASH = Shader.PropertyToID("_BarFill");
        private static readonly int BARFILL_SECONDARY_HASH = Shader.PropertyToID("_BarSecondaryFill");
        
        [SerializeField] private Image healthBar;
        [SerializeField] private Image xpBar;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private TextMeshProUGUI currentComboText;
        
        [SerializeField] private LayoutGroup weaponIconParent;
        [SerializeField] private WeaponUIView weaponIconPrefab;
        
        private List<WeaponUIView> weaponIcons = new List<WeaponUIView>(4);

        private Sequence healthTween = null;
        private Sequence xpTween = null;
        private float lastHealthPercent = 1.0f;
        private float lastXpPercent = 0.0f;
        private readonly float tolerance = Mathf.Epsilon;

        public void Initialize()
        {
            healthBar.material = new Material(healthBar.material);
            xpBar.material = new Material(xpBar.material);
            healthBar.material.SetInt(DO_BARFILL_HASH, 1);
            healthBar.material.SetFloat(GRADIENT_AMOUNT_HASH, 0.75f);
            healthBar.material.SetFloat(BARFILL_HASH, 1.0f);
            healthBar.material.SetFloat(BARFILL_SECONDARY_HASH, 1.0f);
            xpBar.material.SetInt(DO_BARFILL_HASH, 1);
            xpBar.material.SetFloat(GRADIENT_AMOUNT_HASH, 0.75f);
            xpBar.material.SetFloat(BARFILL_HASH, 0.0f);
            xpBar.material.SetFloat(BARFILL_SECONDARY_HASH, 0.0f);
        }

        public void UpdatePlayerHealthView(float percent)
        {
            float difference = percent - lastHealthPercent;
            if (Math.Abs(difference) < tolerance) return;
            lastHealthPercent = percent;
            
            if (healthTween != null)
            {
                healthTween.Kill();
            }
            
            healthTween = DOTween.Sequence();
            
            if (difference > 0)
            {
                healthTween.Append(healthBar.material.DOFloat(percent, BARFILL_SECONDARY_HASH, 0.2f).SetEase(Ease.OutCirc));
                healthTween.AppendInterval(0.1f);
                healthTween.Append(healthBar.material.DOFloat(percent, BARFILL_HASH, 0.2f).SetEase(Ease.OutCirc));
            }
            else
            {
                healthTween.Append(healthBar.material.DOFloat(percent, BARFILL_HASH, 0.2f).SetEase(Ease.OutCirc));
                healthTween.AppendInterval(0.1f);
                healthTween.Append(healthBar.material.DOFloat(percent, BARFILL_SECONDARY_HASH, 0.2f).SetEase(Ease.OutCirc));
            }
        }
        
        public void UpdatePlayerXPView(float percent)
        {
            float difference = percent - lastXpPercent;
            if (Math.Abs(difference) < tolerance) return;
            lastXpPercent = percent;
            
            if (xpTween != null)
            {
                xpTween.Kill();
            }
            
            xpTween = DOTween.Sequence();
            if (difference > 0)
            {
                xpTween.Append(xpBar.material.DOFloat(percent, BARFILL_SECONDARY_HASH, 0.2f).SetEase(Ease.OutCirc));
                xpTween.AppendInterval(0.1f);
                xpTween.Append(xpBar.material.DOFloat(percent, BARFILL_HASH, 0.2f).SetEase(Ease.OutCirc));
            }
            else
            {
                xpTween.Append(xpBar.material.DOFloat(percent, BARFILL_HASH, 0.2f).SetEase(Ease.OutCirc));
                xpTween.AppendInterval(0.1f);
                xpTween.Append(xpBar.material.DOFloat(percent, BARFILL_SECONDARY_HASH, 0.2f).SetEase(Ease.OutCirc));
            }
        }

        public void XPReverse()
        {
            xpBar.material.SetFloat(BARFILL_HASH, 0.0f);
            xpBar.material.SetFloat(BARFILL_SECONDARY_HASH, 1.0f);
            
            if (xpTween != null)
            {
                xpTween.Kill();
            }
            
            xpTween = DOTween.Sequence();
            xpTween.Append(xpBar.material.DOFloat(0.0f, BARFILL_SECONDARY_HASH, 7.5f).SetEase(Ease.OutCirc));
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
            newIcon.UpdateChargeIndicator(0.0f);
            newIcon.UpdateReadyIndicator(true);

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
