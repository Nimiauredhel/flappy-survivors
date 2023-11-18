using System;
using AYellowpaper;
using UnityEngine;

namespace Gameplay.Upgrades
{
    [Serializable]
    public class UpgradeOption
    {
        public int LevelRequirement => upgradeConfig.Value.LevelRequirement();
        [HideInInspector]
        public string name;
        [HideInInspector]
        public bool Taken = false;
        public IUpgrade UpgradeConfig => upgradeConfig.Value;
        
        [SerializeField] private InterfaceReference<IUpgrade, ScriptableObject> upgradeConfig;
        
        public void SetName(string newName)
        {
            this.name = newName;
        }
    }

    public enum  UpgradeType
    {
        None,
        Stats,
        Weapon
    }
    
    public interface IUpgrade
    {
        public UpgradeType Type();
        public Sprite Icon();
        public string Name();
        public string Description();
        public int LevelRequirement();
        public int Commonness();
    }
}