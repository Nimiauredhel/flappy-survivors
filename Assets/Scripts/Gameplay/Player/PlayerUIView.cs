using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Player
{
    public class PlayerUIView : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider xpSlider;

        public void UpdatePlayerHealthView(float percent)
        {
            healthSlider.value = percent;
        }
        
        public void UpdatePlayerXPView(float percent)
        {
            xpSlider.value = percent;
        }
    }
}
