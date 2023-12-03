using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MainMenu
{
    public class LevelUIToggle : MonoBehaviour
    {
        public Toggle Toggle => toggle;
        public Image Image => image;
        public TextMeshProUGUI Text => text;
    
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI text;
    }
}