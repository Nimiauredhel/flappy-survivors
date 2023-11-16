using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Upgrades
{
    [Serializable]
    public class UpgradeTree
    {
        [SerializeField] private UpgradeBranch[] branches;

        public void ResetUpgradeTree()
        {
            foreach (UpgradeBranch branch in branches)
            {
                foreach (UpgradeLevel upgradeLevel in branch.UpgradeLevels)
                {
                    foreach (UpgradeOption upgradeOption in upgradeLevel.UpgradeOptions)
                    {
                        upgradeOption.Taken = false;
                    }
                }
            }
        }

        public List<UpgradeOption> GetAllCurrentOptions()
        {
            List<UpgradeOption> currentOptions = new List<UpgradeOption>(16);
            UpgradeBranch currentBranch = null;
            
            for (int i = 0; i < branches.Length; i++)
            {
                currentBranch = branches[i];
                UpgradeLevel currentLevelOfBranch = null;

                for (int j = 0; j < currentBranch.UpgradeLevels.Length; j++)
                {
                    if (!IsLevelComplete(currentBranch.UpgradeLevels[j]))
                    {
                        currentLevelOfBranch = currentBranch.UpgradeLevels[j];
                        break;
                    }
                }

                if (currentLevelOfBranch != null)
                {
                    for (int j = 0; j < currentLevelOfBranch.UpgradeOptions.Length; j++)
                    {
                        if (!currentLevelOfBranch.UpgradeOptions[j].Taken)
                        {
                            currentOptions.Add(currentLevelOfBranch.UpgradeOptions[j]);
                        }
                    }
                }
            }

            return currentOptions;
        }

        private bool IsLevelComplete(UpgradeLevel level)
        {
            for (int i = 0; i < level.UpgradeOptions.Length; i++)
            {
                if (!level.UpgradeOptions[i].Taken)
                {
                    return false;
                }
            }

            return true;
        }
    }
}