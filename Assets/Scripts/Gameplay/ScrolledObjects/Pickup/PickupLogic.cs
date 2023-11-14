using System;
using UnityEngine;

namespace Gameplay.ScrolledObjects.Pickup
{
    public class PickupLogic : IScrolledObjectLogic
    {
        private PickupType type;
        private int value;
        private float lifetime;

        private float elapsedTime = 0.0f;

        public PickupLogic(PickupType type, int value, float lifetime)
        {
            this.type = type;
            this.value = value;
            this.lifetime = lifetime;
        }

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
            view.transform.Translate((new Vector2(-7.0f, 0.0f) * Time.fixedDeltaTime));
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

        public void OnActivate(int newValue)
        {
            elapsedTime = 0.0f;
            value = newValue;
        }

        public void OnDeactivate()
        {
            
        }
    }
}
