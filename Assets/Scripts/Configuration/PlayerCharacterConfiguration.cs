using Gameplay.Player;
using Gameplay.Upgrades;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Character Config", menuName = "Config/Character Config", order = 0)]
    public class PlayerCharacterConfiguration : ScriptableObject
    {
        public PlayerStats GetStats => new PlayerStats(stats);
        public WeaponConfiguration[] StartingWeapons => startingWeapons;
        public UpgradeTree GetUpgradeTree => upgradeTree;

        [SerializeField] private PlayerStats stats;
        private WeaponConfiguration[] startingWeapons;
        private UpgradeTree upgradeTree;

        public void Initialize(PlayerStats newStats, WeaponConfiguration[] newStartingWeapons, UpgradeTree newUpgradeTree)
        {
            this.stats = newStats;
            this.startingWeapons = newStartingWeapons;
            this.upgradeTree = newUpgradeTree;
        }
    }
}
