using Gameplay.ScrolledObjects;
using Gameplay.ScrolledObjects.Pickup;
using UnityEngine;

namespace Gameplay.Configuration
{
    [CreateAssetMenu(fileName = "Pickup Config", menuName = "Config/Pickup Config", order = 0)]
    public class PickupConfiguration : ScriptableObject
    {
        public PickupType Type => type;
        public Sprite PickupSprite => pickupSprite;

        [SerializeField] private PickupType type;
        [SerializeField] private Sprite pickupSprite;
    }
}