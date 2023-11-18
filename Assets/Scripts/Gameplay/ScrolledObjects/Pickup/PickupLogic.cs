using System;
using Configuration;
using UnityEngine;

namespace Gameplay.ScrolledObjects.Pickup
{
    public class PickupLogic : IScrolledObjectLogic
    {
        private readonly PickupType type;
        private int value;
        private readonly float lifetime;

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

        public void OnHitByWeapon(ScrolledObjectView view, int damage)
        {
            Debug.LogWarning("Pickup hit by weapon. This shouldn't happen.");
        }

        public void OnHitByPlayer(ScrolledObjectView view, Action<int> hpAction, Action<int> xpAction)
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
                case PickupType.Upgrade:
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
