using System.Collections.Generic;
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

        public void IncorporateLogicUpgrade(List<WeaponLogicComponent> upgradeComponents)
        {
            List<WeaponLogicComponent> newList = new List<WeaponLogicComponent>(components.Length + upgradeComponents.Count);
            newList.AddRange(components);
            newList.AddRange(upgradeComponents);

            components = newList.ToArray();
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

        public void OnUpdate(WeaponInstance instance)
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].OnUpdate(instance);
            }
        }
        
        public void OnFixedUpdate(WeaponInstance instance)
        {
            for (int i = 0; i < components.Length; i++)
            {
                components[i].OnFixedUpdate(instance);
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
            HitTrigger hitTrigger = other.gameObject.GetComponent<HitTrigger>();

            if (hitTrigger != null && hitTrigger.HitReceiver != null)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    components[i].HitHandler(hitTrigger.HitReceiver, instance);
                }
            }
        }
    }
}
