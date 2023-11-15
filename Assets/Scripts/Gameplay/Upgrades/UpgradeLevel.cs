using System;
using UnityEngine;

namespace Gameplay.Upgrades
{
    [Serializable]
    public class UpgradeLevel
    {
        public UpgradeOption[] UpgradeOptions => upgradeOptions;
        
        [SerializeField] private UpgradeOption[] upgradeOptions;
    }
}