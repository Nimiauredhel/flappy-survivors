using System;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        public int Power => power;
        public int Amount => amount;
        public int Hits => hits;
        public float Speed => speed;
        public float Duration => duration;
        public float Area => area;
        public float ChargeCapacity => chargeCapacity;
        public float ChargeUseThreshold => chargeUseThreshold;
        public float ChargeDepletionRate => chargeDepletionRate;
        public float ChargeReplenishmentRate => chargeReplenishmentRate;
        
        public string Name => name;
        public string Description => description;
        
        [SerializeField] private WeaponType weaponType;
        [SerializeField][Range(0, 30)] private int power = 1;
        [SerializeField][Range(0, 30)] private int amount = 1;
        [SerializeField] [Range(-30, 30)] private int hits = 0;
        [SerializeField][Range(0.0f, 30.0f)] private float speed = 1.0f;
        [SerializeField][Range(0.0f, 30.0f)] private float duration = 1.0f;
        [SerializeField][Range(0.0f, 10.0f)] private float area = 1.0f;
        [FormerlySerializedAs("cooldown")][SerializeField][Range(0.0f, 30.0f)] private float chargeCapacity = 1.0f;
        [SerializeField] private float chargeDepletionRate = 1.0f;
        [SerializeField] private float chargeReplenishmentRate = 1.0f;
        [SerializeField] private float chargeUseThreshold = 1.0f;
        
        [SerializeField] private string name;
        [SerializeField][TextArea] private string description;

        public WeaponStats(WeaponStats original)
        {
            weaponType = original.weaponType;
            power = original.power;
            speed = original.speed;
            duration = original.duration;
            area = original.area;
            chargeCapacity = original.chargeCapacity;
            chargeUseThreshold = original.chargeUseThreshold;
            chargeDepletionRate = original.chargeDepletionRate;
            chargeReplenishmentRate = original.ChargeReplenishmentRate;
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
            chargeCapacity += upgrade.chargeCapacity;
            chargeUseThreshold += upgrade.chargeUseThreshold;
            chargeDepletionRate += upgrade.chargeDepletionRate;
            chargeReplenishmentRate += upgrade.ChargeReplenishmentRate;
            amount += upgrade.amount;
            hits += upgrade.hits;
        }
    }
}
