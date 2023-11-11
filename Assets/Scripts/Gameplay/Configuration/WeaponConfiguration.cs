using Gameplay.Weapons;
using UnityEngine;
using TypeReferences;

namespace Gameplay.Configuration
{
    [CreateAssetMenu(fileName = "Weapon Config", menuName = "Config/Weapon Config", order = 0)]
    public class WeaponConfiguration : ScriptableObject
    {
        public enum WeaponType
        {
            None,
            Climbing,
            Diving,
            Both
        }

        public WeaponType Type => weaponType;
        public float Cooldown => cooldown;
        public float Duration => duration;
        public float BaseDamage => baseDamage;
        public WeaponView ViewPrefab => viewPrefab;
        public TypeReference Logic => logic;
        public WeaponConfiguration NextLevel => nextLevel;

        [SerializeField] private WeaponType weaponType;
        [SerializeField] private float cooldown;
        [SerializeField] private float duration;
        [Space] 
        [SerializeField] private float baseDamage;
        [Space] 
        [SerializeField] private WeaponView viewPrefab;
        [Inherits(typeof(WeaponLogicSandbox))]
        [SerializeField] private TypeReference logic;
        [SerializeField] private WeaponConfiguration nextLevel;
    }
}
