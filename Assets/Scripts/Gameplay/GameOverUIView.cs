using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gameplay
{
    public class GameOverUIView : MonoBehaviour
    {
        public CanvasGroup CanvasGroup => canvasGroup;
        public Image Backdrop => backdrop;
        public Image Frame => frame;
        public TextMeshProUGUI Title => title;
        public TextMeshProUGUI TotalScoreNumber => totalScoreNumber;
        public TextMeshProUGUI ModifierPrefix => modifierPrefix;
        public TextMeshProUGUI ModifierNumber => modifierNumber;
        public Button BigButton => bigButton;
        public TextMeshProUGUI BigButtonText => bigButtonText;
        public Button SmallButton => smallButton;
        public TextMeshProUGUI SmallButtonText => smallButtonText;

        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Image backdrop;
        [SerializeField] private Image frame;
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI totalScoreNumber;
        [SerializeField] private TextMeshProUGUI modifierPrefix;
        [SerializeField] private TextMeshProUGUI modifierNumber;

        [SerializeField] private Button bigButton;
        [SerializeField] private TextMeshProUGUI bigButtonText;
        [SerializeField] private Button smallButton;
        [SerializeField] private TextMeshProUGUI smallButtonText;

        public void SetUp(bool won, UnityAction smallButtonAction, UnityAction bigButtonAction)
        {
            smallButton.onClick.RemoveAllListeners();
            smallButton.onClick.AddListener(DisableAllButtons);
            smallButton.onClick.AddListener(smallButtonAction);
            
            bigButton.onClick.RemoveAllListeners();
            bigButton.onClick.AddListener(DisableAllButtons);
            bigButton.onClick.AddListener(bigButtonAction);
            
            //TODO: set correct color palette according to won/lost

            smallButtonText.text = won ? "Redo" : "Retreat";
            bigButtonText.text = won ? "Retire" : "Retry";
            title.text = won ? "You Won" : "You Died";

            modifierPrefix.text = "";
            modifierNumber.text = "";
            totalScoreNumber.text = "";

            smallButton.interactable = false;
            bigButton.interactable = false;
        }

        private void DisableAllButtons()
        {
            bigButton.interactable = false;
            smallButton.interactable = false;
        }
    }
}
