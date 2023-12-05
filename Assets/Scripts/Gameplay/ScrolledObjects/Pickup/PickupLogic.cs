using System;
using Audio;
using Configuration;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace Gameplay.ScrolledObjects.Pickup
{
    public class PickupLogic : IScrolledObjectLogic
    {
        private readonly PickupType type;
        private object value;

        private float elapsedTime = 0.0f;
        private Spline path;

        public PickupLogic(PickupType type, object value)
        {
            this.type = type;
            this.value = value;
        }

        public void ScrolledObjectUpdate(ScrolledObjectView view)
        {
            elapsedTime += Time.deltaTime;
        }

        public void ScrolledObjectFixedUpdate(ScrolledObjectView view)
        {
            if (path == null)
            {
                view.Body.MovePosition(view.Body.position + new Vector2(-7.0f, 0.0f) * Time.fixedDeltaTime);
            }
            else
            {
                view.Body.MovePosition(path.EvaluatePosition(elapsedTime).xy);
            }
        }

        public void OnHitByWeapon(ScrolledObjectView view, int damage)
        {
            Debug.LogWarning("Pickup hit by weapon. This shouldn't happen.");
        }

        public void OnHitPlayer(ScrolledObjectView view, Action<int> hpAction, Action<int> xpAction, Action<UpgradeOption> upgradeAction)
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
            
            AudioService.Instance.PlayPickupCollected();
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
            
            AudioService.Instance.PlayPickupSpawned();
        }

        public void OnDeactivate(ScrolledObjectView view)
        {
            path = null;
        }
        
        public void SetPath(Spline path)
        {
            this.path = path;
        }
    }
}
