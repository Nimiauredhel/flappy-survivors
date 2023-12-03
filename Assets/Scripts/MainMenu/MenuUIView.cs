using System;
using System.Collections.Generic;
using Gameplay.Upgrades;
using Configuration;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MainMenu
{
    public class MenuUIView : MonoBehaviour
    {
        [SerializeField] private LevelUIToggle[] levelToggles;
        
        [SerializeField] private Slider loadingBar;
        [SerializeField] private LoadoutUIButton[] upgradeButtons;
        
        public void DisplayLevelsDialog(LevelConfiguration[] options, Action<LevelConfiguration> selectionCallback)
        {
            int numOptions = levelToggles.Length < options.Length ? levelToggles.Length : options.Length;
            
            for (int i = 0; i < numOptions; i++)
            {
                LevelConfiguration option = options[i];
                LevelUIToggle toggle = levelToggles[i];

                TimeSpan timerTimespan = TimeSpan.FromSeconds(option.RunTime);
                int selectedFormat = timerTimespan.Minutes > 9 ? 0 : timerTimespan.Minutes > 0 ? 1 : 2;
                
                toggle.Image.sprite = option.Thumbnail;
                toggle.Text.text = option.Name;
                toggle.Text.text += "\n" + TimeSpan.FromSeconds(option.RunTime).ToString(Constants.TIMER_FORMATS[selectedFormat]);
                toggle.Toggle.onValueChanged.AddListener(delegate { OnLevelSelected(option, toggle.Toggle, selectionCallback); });
                toggle.gameObject.SetActive(true);
            }

            levelToggles[0].Toggle.interactable = false;
            levelToggles[0].Toggle.isOn = true;
        }
        
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

        private void OnLevelSelected(LevelConfiguration level, Toggle toggle, Action<LevelConfiguration> selectionCallback)
        {
            for (int i = 0; i < levelToggles.Length; i++)
            {
                levelToggles[i].Toggle.interactable = true;
                levelToggles[i].Toggle.SetIsOnWithoutNotify(false);
            }
            
            toggle.interactable = false;
            toggle.SetIsOnWithoutNotify(true);
            selectionCallback?.Invoke(level);
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