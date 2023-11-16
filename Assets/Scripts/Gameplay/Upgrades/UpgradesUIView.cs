using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Upgrades
{
    public class UpgradesUIView : MonoBehaviour
    {
        [SerializeField] private UpgradesUIButton[] upgradeButtons;
        
        public void DisplayUpgradesDialog(List<UpgradeOption> options, Action<UpgradeOption> selectionCallback)
        {
            int numOptions = upgradeButtons.Length < options.Count ? upgradeButtons.Length : options.Count;

            for (int i = 0; i < numOptions; i++)
            {
                UpgradeOption option = options[i];
                UpgradesUIButton button = upgradeButtons[i];

                button.Image.sprite = option.UpgradeConfig.IconSprite;
                button.Text.text = option.UpgradeConfig.Stats.Description;
                    
                button.Button.onClick.AddListener(delegate { OnUpgradeSelected(option, selectionCallback); });
                
                button.gameObject.SetActive(true);
            }
        }

        private void OnUpgradeSelected(UpgradeOption option, Action<UpgradeOption> selectionCallback)
        {
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                upgradeButtons[i].Button.onClick.RemoveAllListeners();
                upgradeButtons[i].gameObject.SetActive(false);
            }
            
            gameObject.SetActive(false);
            
            selectionCallback?.Invoke(option);
        }
    }
}
