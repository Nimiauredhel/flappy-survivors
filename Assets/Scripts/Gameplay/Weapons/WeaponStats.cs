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
        
        public float Power => power;
        public float Speed => speed;
        public float Duration => duration;
        public float Area => area;
        public float Cooldown => cooldown;
        public int Amount => amount;
        public int Hits => hits;
        public string Name => name;
        public string Description => description;
        
        [SerializeField] private WeaponType weaponType;
        [SerializeField][Range(0.0f, 30.0f)] private float power = 1.0f;
        [SerializeField][Range(0.0f, 30.0f)] private float speed = 1.0f;
        [SerializeField][Range(0.0f, 30.0f)] private float duration = 1.0f;
        [SerializeField][Range(0.0f, 10.0f)] private float area = 1.0f;
        [SerializeField][Range(-30.0f, 30.0f)] private float cooldown = 1.0f;
        [SerializeField][Range(0, 30)] private int amount = 1;
        [SerializeField] [Range(-30, 30)] private int hits = 0;
        
        
        [SerializeField] private string name;
        [SerializeField][TextArea] private string description;

        public WeaponStats(WeaponStats original)
        {
            weaponType = original.weaponType;
            power = original.power;
            speed = original.speed;
            duration = original.duration;
            area = original.area;
            cooldown = original.cooldown;
            amount = original.amount;
            hits = original.hits;

            name = original.Name;
            description = original.description;
        }

        public void ApplyUpgrade(WeaponStats upgrade)
        {
            if (upgrade.weaponType != WeaponType.None)
            {
                weaponType = upgrade.weaponType;
            }

            power += upgrade.power;
            speed += upgrade.speed;
            duration += upgrade.duration;
            area += upgrade.area;
            cooldown += upgrade.cooldown;
            amount += upgrade.amount;
            hits += upgrade.hits;
        }
    }
}
