using Gameplay.ScrolledObjects;
using UnityEngine;

namespace Gameplay.Weapons.WeaponLogic
{
    public class WeaponLogicEntity
    {
        private WeaponLogicComponent[] components;

        public WeaponLogicEntity(WeaponLogicComponent[] components)
        {
            this.components = components;
        }

        public bool Initialize(WeaponInstance instance)
        {
            if (components.Length == 0)
            {
                Debug.LogWarning("Attempted to initialize weapon with no components!");
                return false;
            }

            for (int i = 0; i < components.Length; i++)
            {
                components[i].Initialize(instance);
            }

            return true;
        }

        public void OnDispose(WeaponInstance instance)
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].OnDispose(instance);
            }
        }

        public void Draw(WeaponInstance instance)
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].Draw(instance);
            }
        }

        public void Sheathe(WeaponInstance instance)
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].Sheathe(instance);
            }
        }
        
        public void HitHandler(object sender, Collider2D other, WeaponInstance instance)
        {
            ScrolledObjectView SO = other.gameObject.GetComponentInParent<ScrolledObjectView>();

            if (SO != null && SO.Active)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    components[i].HitHandler(SO, instance);
                }
            }
        }
    }
}
