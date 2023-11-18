using Gameplay.Player;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.Serialization;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Character Config", menuName = "Config/Character Config", order = 0)]
    public class PlayerCharacterConfiguration : ScriptableObject
    {
        public PlayerStats GetStats => new PlayerStats(stats);
        public WeaponConfiguration[] StartingWeapons => startingWeapons;
        public UpgradeTree GetUpgradeTree => upgradeTree.GetFreshUpgradeTree();

        [SerializeField] private PlayerStats stats;
        [SerializeField] private WeaponConfiguration[] startingWeapons;
        [SerializeField] private UpgradeTreeConfiguration upgradeTree;
    }
}
