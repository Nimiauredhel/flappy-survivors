using Gameplay.Player;
using Gameplay.Upgrades;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Stat Upgrade Config", menuName = "Config/Stat Upgrade Config", order = 0)]
    public class StatUpgradeConfiguration : ScriptableObject, IUpgrade
    {

        public PlayerStats Stats => stats;

        [SerializeField] private int levelRequirement = 1;
        [SerializeField] private int commonness = 100;
        [SerializeField] private new string name;
        [SerializeField] private string description;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Sprite icon;
        
        public UpgradeType Type()
        {
            return UpgradeType.Stats;
        }

        public Sprite Icon()
        {
            return icon;
        }

        public string Name()
        {
            return name;
        }

        public string Description()
        {
            return description;
        }

        public int LevelRequirement()
        {
            return levelRequirement;
        }

        public int Commonness()
        {
            return commonness;
        }
    }
}
