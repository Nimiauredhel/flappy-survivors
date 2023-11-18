using Gameplay.Player;
using Gameplay.Upgrades;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Character Config", menuName = "Config/Character Config", order = 0)]
    public class PlayerCharacterConfiguration : ScriptableObject
    {
        public PlayerStats Stats => stats;
        public WeaponConfiguration[] StartingWeapons => startingWeapons;
        public UpgradeTree GetUpgradeTree => upgradeTree.GetFreshUpgradeTree();

        [SerializeField] private PlayerStats stats;
        [SerializeField] private WeaponConfiguration[] startingWeapons;
        [SerializeField] private UpgradeTreeConfiguration upgradeTree;
    }
}
