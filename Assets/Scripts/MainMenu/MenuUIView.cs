using System;
using System.Collections.Generic;
using Gameplay.Upgrades;
using Configuration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class MenuUIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private Image fadePanel;
        [SerializeField] private Button playButton;
        
        [SerializeField] private LevelUIToggle[] levelToggles;
        [SerializeField] private LoadoutUIButton[] upgradeButtons;

        private Tween canvasAlphaTween = null;
        private Tween fadeAlphaTween = null;
        
        public void SetCanvasAlpha(float value, float duration)
        {
            if (canvasAlphaTween != null) canvasAlphaTween.Kill();
            
            if (duration == 0.0f)
            {
                mainCanvasGroup.alpha = value;
            }
            else
            {
                canvasAlphaTween = mainCanvasGroup.DOFade(value, duration);
            }
        }
        
        public void SetFadeAlpha(float value, float duration)
        {
            if (fadeAlphaTween != null) fadeAlphaTween.Kill();
            
            if (duration == 0.0f)
            {
                fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, value);
            }
            else
            {
                fadeAlphaTween = fadePanel.DOFade(value, duration);
            }
        }
        
        public void DisplayLevelsDialog(LevelConfiguration[] options, Action<LevelConfiguration> selectionCallback)
        {
            int numOptions = levelToggles.Length < options.Length ? levelToggles.Length : options.Length;
            
            for (int i = 0; i < numOptions; i++)
            {
                LevelConfiguration option = options[i];
                LevelUIToggle toggle = levelToggles[i];

                TimeSpan timerTimespan = TimeSpan.FromSeconds(option.RunTime);
                int selectedFormat = timerTimespan.Minutes > 9 ? 0 : 1;
                
                toggle.Image.texture = option.Thumbnail.texture;
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
                    
                button.Button.onClick.AddListener(delegate { OnUpgradeSelected(option, button, selectionCallback); });
                
                button.Checkmark.SetActive(false);
                button.gameObject.SetActive(true);
            }

            upgradeButtons[0].Button.onClick.Invoke();
        }

        public void SetupPlayButton(Action playCallback)
        {
            playButton.onClick.AddListener(playCallback.Invoke);
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

        private void OnUpgradeSelected(UpgradeOption option, LoadoutUIButton button, Action<UpgradeOption> selectionCallback)
        {
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                upgradeButtons[i].Button.interactable = true;
                upgradeButtons[i].Checkmark.SetActive(false);
            }

            button.Button.interactable = false;
            button.Checkmark.SetActive(true);
            
            selectionCallback?.Invoke(option);
        }
    }
}