using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Gameplay.ScrolledObjects.Pickup
{
    [Serializable]
    public class PickupLogic : IScrolledObjectLogic
    {
        [SerializeField] private PickupType type;
        [SerializeField] private int value;
        [SerializeField] private float lifetime;

        private float elapsedTime = 0.0f;

        public void ScrolledObjectUpdate(ScrolledObjectView view)
        {
            elapsedTime = Time.deltaTime;

            if (elapsedTime >= lifetime)
            {
                view.Deactivate();
            }
        }

        public void ScrolledObjectFixedUpdate(ScrolledObjectView view)
        {
            
        }

        public void OnHitByWeapon(ScrolledObjectView view, float damage)
        {
            
        }

        public void OnHitByPlayer(ScrolledObjectView view, Action<float> hpAction, Action<int> xpAction)
        {
            switch (type)
            {
                case PickupType.None:
                    break;
                case PickupType.XP:
                    xpAction?.Invoke(value);
                    break;
                case PickupType.Health:
                    hpAction?.Invoke(value);
                    break;
                case PickupType.Gold:
                    break;
                case PickupType.Chest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            view.Deactivate();
        }

        public void OnActivate()
        {
            elapsedTime = 0.0f;
        }

        public void OnDeactivate()
        {
            
        }
    }
}
