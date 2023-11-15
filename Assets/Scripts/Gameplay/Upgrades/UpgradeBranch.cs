using System;
using UnityEngine;

namespace Gameplay.Upgrades
{
    [Serializable]
    public class UpgradeBranch
    {
        public UpgradeLevel[] UpgradeLevels => upgradeLevels;
        
        [SerializeField] private string name;
        [SerializeField] private UpgradeLevel[] upgradeLevels;
        
    }
}