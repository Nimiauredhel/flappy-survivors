using Gameplay.Weapons;
using Gameplay.Weapons.WeaponLogic;
using UnityEngine;
using TypeReferences;

namespace Gameplay.Configuration
{
    [CreateAssetMenu(fileName = "Weapon Config", menuName = "Config/Weapon Config", order = 0)]
    public class WeaponConfiguration : ScriptableObject
    {
        public int Commonness => commonness;
        public WeaponStats Stats => stats;
        public WeaponView ViewPrefab => viewPrefab;
        public TypeReference[] LogicComponents => logicComponents;
        public Sprite IconSprite => iconSprite;
        public WeaponConfiguration NextLevel => nextLevel;

        [SerializeField][Range(1, 100)] private int commonness = 100;
        [SerializeField] private WeaponStats stats;
        [Inherits(typeof(WeaponLogicComponent))]
        [SerializeField] private TypeReference[] logicComponents;
        [SerializeField] private WeaponView viewPrefab;
        [SerializeField] private Sprite iconSprite;
        [SerializeField] private WeaponConfiguration nextLevel;
    }
}
