using System;
using Configuration;
using UnityEngine;

namespace Gameplay.Upgrades
{
    [Serializable]
    public class UpgradeOption
    {
        public bool Taken => taken;
        public WeaponConfiguration UpgradeConfig => upgradeConfig;
        
        private bool taken = false;
        [SerializeField] private WeaponConfiguration upgradeConfig;
    }
}