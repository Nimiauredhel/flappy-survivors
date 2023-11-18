using System;
using UnityEngine;

namespace Gameplay.Upgrades
{
    [Serializable]
    public class UpgradeLevel
    {
        public UpgradeOption[] UpgradeOptions => upgradeOptions;
        
        [HideInInspector] public string name;
        [SerializeField] private UpgradeOption[] upgradeOptions;
        
        
        public void SetName(string newName)
        {
            this.name = newName;
        }
    }
}