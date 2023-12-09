using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MainMenu
{
    public class LevelUIToggle : MonoBehaviour
    {
        public Toggle Toggle => toggle;
        public RawImage Image => image;
        public TextMeshProUGUI Text => text;
    
        [SerializeField] private Toggle toggle;
        [SerializeField] private RawImage image;
        [SerializeField] private TextMeshProUGUI text;
    }
}