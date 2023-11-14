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
        
        
        public float Power => power;
        public float Speed => speed;
        public float Duration => duration;
        public float Area => area;
        public float Cooldown => cooldown;
        public int Amount => amount;
        public string Name => name;
        public string Description => description;
        
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private float power;
        [SerializeField] private float speed;
        [SerializeField] private float duration;
        [SerializeField] private float area;
        [SerializeField] private float cooldown;
        [SerializeField] private int amount;
        
        
        [SerializeField] private string name;
        [SerializeField][TextArea] private string description;
    }
}
