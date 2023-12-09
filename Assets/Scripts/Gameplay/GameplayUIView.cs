using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class GameplayUIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private Image fadePanel;
        [SerializeField] private TextMeshProUGUI timerText;

        [SerializeField] private TextMeshProUGUI gameOverMessage;

        public void SetCanvasAlpha(float value, float duration)
        {
            if (duration == 0.0f)
            {
                mainCanvasGroup.alpha = value;
            }
            else
            {
                mainCanvasGroup.DOFade(value, duration);
            }
        }
        
        public void SetFadeAlpha(float value, float duration, float from = -1.0f)
        {
            if (Math.Abs(from - (-1.0f)) > float.Epsilon)
            {
                fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, from);
            }

            fadePanel.DOFade(value, duration);
        }

        public void ShowGameOverMessage(string message, float delay)
        {
            gameOverMessage.text = message;
            gameOverMessage.alpha = 0.0f;
            gameOverMessage.gameObject.SetActive(true);
            gameOverMessage.DOFade(1.0f, delay);
        }

        public void UpdateTimerText(int timeInSeconds)
        {
            TimeSpan timerTimespan = TimeSpan.FromSeconds(timeInSeconds);
            int selectedFormat = timerTimespan.Minutes > 9 ? 0 : timerTimespan.Minutes > 0 ? 1 : 2;
            timerText.text = TimeSpan.FromSeconds(timeInSeconds).ToString(Constants.TIMER_FORMATS[selectedFormat]);
        }
    }
}
