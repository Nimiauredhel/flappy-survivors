using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gameplay
{
    public class GameplayUIView : MonoBehaviour
    {
        public GameOverUIView GameOverUIUIView => gameOverUIView;
        
        public Button.ButtonClickedEvent PauseButtonClicked
        {
            get
            {
                if (pauseButtonClicked == null)
                {
                    pauseButtonClicked = new Button.ButtonClickedEvent();

                    foreach (Button button in pauseButtons)
                    {
                        button.onClick.AddListener(pauseButtonClicked.Invoke);
                    }
                }

                return pauseButtonClicked;
            }
        }

        [SerializeField] private CanvasGroup hudCanvasGroup;
        [FormerlySerializedAs("gameOverView")] [SerializeField] private GameOverUIView gameOverUIView;
        [SerializeField] private Image fadePanel;
        [SerializeField] private TextMeshProUGUI timerText;

        [SerializeField] private GameObject pausePanel;
        
        [SerializeField] private Button[] pauseButtons;

        private Button.ButtonClickedEvent pauseButtonClicked = null;

        public void SetShowPausePanel(bool value)
        {
            pausePanel.gameObject.SetActive(value);
        }

        public void SetCanvasAlpha(float value, float duration)
        {
            if (duration == 0.0f)
            {
                hudCanvasGroup.alpha = value;
            }
            else
            {
                hudCanvasGroup.DOFade(value, duration);
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

        public void SetGamePhaseText(string phaseString)
        {
            timerText.text = phaseString;
        }

        public void UpdateTimerText(int timeInSeconds)
        {
            TimeSpan timerTimespan = TimeSpan.FromSeconds(timeInSeconds);
            int selectedFormat = timerTimespan.Minutes > 9 ? 0 : timerTimespan.Minutes > 0 ? 1 : 2;
            timerText.text = TimeSpan.FromSeconds(timeInSeconds).ToString(Constants.TIMER_FORMATS[selectedFormat]);
        }
    }
}
