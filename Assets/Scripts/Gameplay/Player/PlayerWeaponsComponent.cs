using System;
using System.Collections.Generic;
using Configuration;
using Gameplay.Weapons;
using Gameplay.Weapons.WeaponLogic;
using TypeReferences;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Player
{
    [Serializable]
    public class PlayerWeaponsComponent
    {
        [SerializeField] private List<WeaponInstance> weapons = new List<WeaponInstance>(8);

        public void InitializeWeapons(Transform weaponParent, WeaponConfiguration[] configs, PlayerUIView uiView)
        {
            foreach (WeaponConfiguration config in configs)
            {
                AddWeapon(weaponParent, config, uiView);
            }
        }

        public void AddOrUpgradeWeapon(Transform weaponParent, WeaponConfiguration config, PlayerUIView uiView)
        {
            int upgradeIndex = -1;

            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].Stats.Name == config.Stats.Name)
                {
                    upgradeIndex = i;
                    break;
                }
            }

            if (upgradeIndex == -1)
            {
                AddWeapon(weaponParent, config, uiView);
            }
            else
            {
                UpgradeWeapon(upgradeIndex, weaponParent, config, uiView);
            }
        }

        public void OnDispose()
        {
            foreach (WeaponInstance weapon in weapons)
            {
                weapon.OnDispose();
            }
        }

        public void WeaponsUpdate(WeaponType validType)
        {
            if (weapons == null || weapons.Count == 0) return;

            foreach (WeaponInstance weapon in weapons)
            {
                weapon.WeaponUpdate(validType);
            }
        }

        public void WeaponsFixedUpdate(WeaponType validType)
        {
            if (weapons == null || weapons.Count == 0) return;
            
            foreach (WeaponInstance weapon in weapons)
            {
                weapon.WeaponFixedUpdate(validType);
            }
        }
        
        private void AddWeapon(Transform weaponParent, WeaponConfiguration config, PlayerUIView uiView)
        {
            WeaponUIView weaponUIView = uiView.AddOrReplaceWeaponIcon(config.IconSprite);
                
            weapons.Add(new WeaponInstance
            (
                Object.Instantiate(config.ViewPrefab, weaponParent),
                new WeaponStats(config.Stats),
                WeaponLogicBuilder.BuildWeaponLogicEntity(config.LogicComponents),
                weaponUIView
            ));
        }
        
        private void UpgradeWeapon(int upgradeIndex, Transform weaponParent, WeaponConfiguration config, PlayerUIView uiView)
        {
            weapons[upgradeIndex].ApplyUpgrade(config);
            //TODO: enable replacing the weapon view etc.
        }
    }
}
