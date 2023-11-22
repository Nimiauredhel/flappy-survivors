using Gameplay.Upgrades;

namespace Configuration
{
    public static class ConfigSelectionMediator
    {
        private static LevelConfiguration selectedLevel = null;
        private static PlayerCharacterConfiguration selectedCharacter = null;
        private static UpgradeTree selectedUpgradeTree = null;

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

        public static void SetLevel(LevelConfiguration levelConfiguration)
        {
            selectedLevel = levelConfiguration;
        }

        public static void SetCharacterLoadout(PlayerCharacterConfiguration playerCharacterConfiguration, UpgradeTree upgradeTree)
        {
            selectedCharacter = playerCharacterConfiguration;
            selectedUpgradeTree = upgradeTree;
        }
    }
}
