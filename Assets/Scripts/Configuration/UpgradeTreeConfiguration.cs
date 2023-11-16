using Gameplay.Upgrades;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Upgrade Tree Config", menuName = "Config/Upgrade Tree Config", order = 0)]
    public class UpgradeTreeConfiguration : ScriptableObject
    {
        public UpgradeTree GetFreshUpgradeTree()
        {
            upgradeTree.ResetUpgradeTree();
            return upgradeTree;
        }

        [SerializeField] private UpgradeTree upgradeTree;
    }
}
