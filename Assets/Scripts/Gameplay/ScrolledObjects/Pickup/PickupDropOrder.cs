using UnityEngine;

namespace Gameplay.ScrolledObjects.Pickup
{
    public struct PickupDropOrder
    {
        public readonly int Value;
        public readonly Vector3 Position;

        public PickupDropOrder(int value, Vector3 position)
        {
            this.Value = value;
            this.Position = position;
        }
    }
}