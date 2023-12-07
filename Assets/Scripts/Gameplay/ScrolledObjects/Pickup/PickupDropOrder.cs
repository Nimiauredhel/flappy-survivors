using UnityEngine;

namespace Gameplay.ScrolledObjects.Pickup
{
    public struct PickupDropOrder
    {
        public object Value => value;
        public PickupType Type => type;
        public Vector3 Position => position;

        private object value;
        private PickupType type;
        private Vector3 position;

        public PickupDropOrder(object value, PickupType type, Vector3 position)
        {
            this.value = value;
            this.type = type;
            this.position = position;
        }

        public void SetNewValue(object value)
        {
            this.value = value;
        }
    }
}