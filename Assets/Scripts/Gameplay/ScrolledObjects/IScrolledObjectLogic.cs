using System;

namespace Gameplay.ScrolledObjects
{
    public interface IScrolledObjectLogic
    {
        public void ScrolledObjectUpdate(ScrolledObjectView view);

        public void ScrolledObjectFixedUpdate(ScrolledObjectView view);

        public void OnHitByWeapon(ScrolledObjectView view, float damage);

        public void OnHitByPlayer(ScrolledObjectView view, Action<int> hpAction, Action<int> xpAction);

        public void OnActivate(int value);

        public void OnDeactivate();
    }
}
