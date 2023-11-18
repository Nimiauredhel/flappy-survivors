using Gameplay.Upgrades;
using Gameplay.Weapons;
using Gameplay.Weapons.WeaponLogic;
using TypeReferences;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Weapon Config", menuName = "Config/Weapon Config", order = 0)]
    public class WeaponConfiguration : ScriptableObject, IUpgrade
    {
        public WeaponStats Stats => stats;
        public WeaponView ViewPrefab => viewPrefab;
        public TypeReference[] LogicComponents => logicComponents;
        public Sprite IconSprite => iconSprite;

        [SerializeField] private int levelRequirement = 1;
        [SerializeField][Range(1, 100)] private int commonness = 100;
        [SerializeField] private WeaponStats stats;
        [Inherits(typeof(WeaponLogicComponent))]
        [SerializeField] private TypeReference[] logicComponents;
        [SerializeField] private WeaponView viewPrefab;
        [SerializeField] private Sprite iconSprite;
        public UpgradeType Type()
        {
            return UpgradeType.Weapon;
        }

        public Sprite Icon()
        {
            return iconSprite;
        }

        public string Name()
        {
            return stats.Name;
        }

        public string Description()
        {
            return stats.Description;
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
