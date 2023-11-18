using UnityEngine;

namespace Gameplay.ScrolledObjects.Pickup
{
    public struct PickupDropOrder
    {
        public readonly int Value;
        public readonly PickupType Type;
        public readonly Vector3 Position;

        public PickupDropOrder(int value, PickupType type, Vector3 position)
        {
            this.Value = value;
            this.Type = type;
            this.Position = position;
        }
    }
}