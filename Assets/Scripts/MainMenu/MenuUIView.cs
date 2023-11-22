using System;
using System.Collections.Generic;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MainMenu
{
    public class MenuUIView : MonoBehaviour
    {
        [SerializeField] private Slider loadingBar;
        [SerializeField] private LoadoutUIButton[] upgradeButtons;
        
        public void DisplayLoadoutsDialog(List<UpgradeOption> options, Action<UpgradeOption> selectionCallback)
        {
            int numOptions = upgradeButtons.Length < options.Count ? upgradeButtons.Length : options.Count;

            for (int i = 0; i < numOptions; i++)
            {
                UpgradeOption option = options[i];
                LoadoutUIButton button = upgradeButtons[i];

                button.Image.sprite = option.UpgradeConfig.Icon();
                button.Text.text = option.UpgradeConfig.Description();
                    
                button.Button.onClick.AddListener(delegate { OnUpgradeSelected(option, selectionCallback); });
                
                button.gameObject.SetActive(true);
            }
        }

        public void SetLoadingBar(float percent)
        {
            loadingBar.value = percent;
        }

        private void OnUpgradeSelected(UpgradeOption option, Action<UpgradeOption> selectionCallback)
        {
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                upgradeButtons[i].Button.onClick.RemoveAllListeners();
                upgradeButtons[i].gameObject.SetActive(false);
            }
            
            //gameObject.SetActive(false);
            
            selectionCallback?.Invoke(option);
        }
    }
}