using System;
using System.Collections.Generic;
using Gameplay.Configuration;
using Gameplay.Weapons;
using TypeReferences;
using UnityEngine;

namespace Gameplay.Player
{
    [Serializable]
    public class PlayerWeaponsComponent
    {
        [SerializeField] private List<WeaponInstance> weapons = new List<WeaponInstance>(8);

        public void InitializeWeapons(Transform weaponParent, WeaponConfiguration[] configs)
        {
            foreach (WeaponConfiguration config in configs)
            {
                var logicInstance = Activator.CreateInstance(config.Logic);
                
                WeaponInstance newInstance = new WeaponInstance();
                newInstance.Initialize(
                    WeaponView.Instantiate(config.ViewPrefab, weaponParent),
                    config, logicInstance as WeaponLogicSandbox
                    );
                weapons.Add(newInstance);
            }
        }

        public void OnDispose()
        {
            foreach (WeaponInstance weapon in weapons)
            {
                weapon.OnDispose();
            }
        }

        public void WeaponsUpdate(WeaponConfiguration.WeaponType validType)
        {
            if (weapons == null || weapons.Count == 0) return;

            foreach (WeaponInstance weapon in weapons)
            {
                weapon.WeaponUpdate(validType);
            }
        }
    }
}
