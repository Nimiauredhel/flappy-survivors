using System;
using System.Collections.Generic;
using Gameplay.Configuration;
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
                WeaponUIView weaponUIView = uiView.AddOrReplaceWeaponIcon(config.IconSprite);
                
                weapons.Add(new WeaponInstance
                (
                    Object.Instantiate(config.ViewPrefab, weaponParent),
                    config.Stats,
                    WeaponLogicBuilder.BuildWeaponLogicEntity(config.LogicComponents),
                    config.NextLevel, weaponUIView
                ));
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
    }
}
