using System;
using UnityEngine;

namespace Gameplay.Weapons
{
    public enum WeaponType
    {
        None,
        Climbing,
        Diving,
        Both
    }
    
    [Serializable]
    public class WeaponStats
    {
        public WeaponType Type => weaponType;
        public float Cooldown => cooldown;
        public float Duration => duration;
        public float BaseDamage => baseDamage;
        
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private float cooldown;
        [SerializeField] private float duration;
        [SerializeField] private float baseDamage;
    }
}
