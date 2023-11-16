using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Upgrades
{
    public class UpgradesUIButton : MonoBehaviour
    {
        public Button Button => button;
        public Image Image => image;
        public TextMeshProUGUI Text => text;
    
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI text;
    }
}
