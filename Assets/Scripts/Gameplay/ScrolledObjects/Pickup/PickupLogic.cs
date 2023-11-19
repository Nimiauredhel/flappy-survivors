using System;
using Configuration;
using Gameplay.Upgrades;
using UnityEngine;

namespace Gameplay.ScrolledObjects.Pickup
{
    public class PickupLogic : IScrolledObjectLogic
    {
        private readonly PickupType type;
        private object value;
        private readonly float lifetime;

        private float elapsedTime = 0.0f;

        public PickupLogic(PickupType type, object value, float lifetime)
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

        public void OnHitByPlayer(ScrolledObjectView view, Action<int> hpAction, Action<int> xpAction, Action<UpgradeOption> upgradeAction)
        {
            switch (type)
            {
                case PickupType.None:
                    break;
                case PickupType.XP:
                    xpAction?.Invoke((int)value);
                    break;
                case PickupType.Health:
                    hpAction?.Invoke((int)value);
                    break;
                case PickupType.Upgrade:
                    upgradeAction?.Invoke((UpgradeOption)value);
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

        public void OnActivate(ScrolledObjectView view, object newValue)
        {
            elapsedTime = 0.0f;
            value = newValue;

            switch (type)
            {
                case PickupType.None:
                    break;
                case PickupType.XP:
                    break;
                case PickupType.Health:
                    break;
                case PickupType.Upgrade:
                    UpgradeOption upgrade = (UpgradeOption)newValue;

                    view.SecondaryGraphic.sprite = upgrade.UpgradeConfig.Icon();
                    view.Text.text = upgrade.UpgradeConfig.Description();
                    
                    break;
                case PickupType.Gold:
                    break;
                case PickupType.Chest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDeactivate(ScrolledObjectView view)
        {
            
        }
    }
}
