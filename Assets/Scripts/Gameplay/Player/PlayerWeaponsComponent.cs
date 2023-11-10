using System;
using System.Collections.Generic;
using Gameplay.Data;
using Gameplay.Weapons;
using UnityEngine;

namespace Gameplay.Player
{
    [Serializable]
    public class PlayerWeaponsComponent
    {
        [SerializeField] private List<WeaponInstance> weapons = new List<WeaponInstance>(8);

        public void InitializeWeapons()
        {
            foreach (WeaponInstance weapon in weapons)
            {
                weapon.Initialize();
            }
        }

        public void WeaponsUpdate(WeaponData.WeaponType validType)
        {
            if (weapons == null || weapons.Count == 0) return;

            foreach (WeaponInstance weapon in weapons)
            {
                weapon.WeaponUpdate(validType);
            }
        }
    }
}
