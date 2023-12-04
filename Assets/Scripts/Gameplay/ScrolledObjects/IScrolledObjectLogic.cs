using System;
using Gameplay.Upgrades;
using UnityEngine.Splines;

namespace Gameplay.ScrolledObjects
{
    public interface IScrolledObjectLogic
    {
        public void ScrolledObjectUpdate(ScrolledObjectView view);

        public void ScrolledObjectFixedUpdate(ScrolledObjectView view);

        public void OnHitByWeapon(ScrolledObjectView view, int damage);

        public void OnHitPlayer(ScrolledObjectView view, Action<int> hpAction, Action<int> xpAction, Action<UpgradeOption> upgradeAction);
        
        public void OnActivate(ScrolledObjectView view, object value);

        public void OnDeactivate(ScrolledObjectView view);

        public void SetPath(Spline path);
    }
}
