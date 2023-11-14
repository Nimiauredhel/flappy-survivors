using System;

namespace Gameplay.ScrolledObjects
{
    public interface IScrolledObjectLogic
    {
        public void ScrolledObjectUpdate(ScrolledObjectView view);

        public void ScrolledObjectFixedUpdate(ScrolledObjectView view);

        public void OnHitByWeapon(ScrolledObjectView view, float damage);

        public void OnHitByPlayer(ScrolledObjectView view, Action<float> hpAction, Action<int> xpAction);

        public void OnActivate();

        public void OnDeactivate();
    }
}
