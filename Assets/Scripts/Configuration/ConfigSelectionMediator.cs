using System.Collections.Generic;
using Gameplay.Upgrades;
using Unity.VisualScripting;

namespace Configuration
{
    public static class ConfigSelectionMediator
    {
        private static LevelConfiguration selectedLevel = null;
        private static PlayerCharacterConfiguration selectedCharacter = null;
        private static UpgradeTree selectedUpgradeTree = null;
        private static UpgradeOption selectedStartingLoadout = null;

        public static LevelConfiguration GetLevelConfig()
        {
            return selectedLevel;
        }
        
        public static PlayerCharacterConfiguration GetCharacterConfiguration()
        {
            return selectedCharacter;
        }
        
        public static UpgradeTree GetUpgradeTree()
        {
            return selectedUpgradeTree;
        }

        public static UpgradeOption GetStartingLoadout()
        {
            return selectedStartingLoadout;
        }

        public static void SetLevel(LevelConfiguration levelConfiguration)
        {
            selectedLevel = levelConfiguration;
        }

        public static void SetCharacterLoadout(PlayerCharacterConfiguration playerCharacterConfiguration, UpgradeTree upgradeTree, UpgradeOption startingLoadout)
        {
            selectedCharacter = playerCharacterConfiguration;
            selectedUpgradeTree = upgradeTree;
            selectedStartingLoadout = startingLoadout;
        }
    }
}
