using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MainMenu
{
    public class LoadoutUIButton : MonoBehaviour
    {
        public Button Button => button;
        public Image Image => image;
        public TextMeshProUGUI Text => text;
    
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI text;
    }
}