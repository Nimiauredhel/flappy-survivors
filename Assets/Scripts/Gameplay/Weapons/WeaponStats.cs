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
        public int Hits => hits;
        public string Name => name;
        public string Description => description;
        
        [SerializeField] private WeaponType weaponType;
        [SerializeField][Range(1.0f, 30.0f)] private float power;
        [SerializeField][Range(1.0f, 30.0f)] private float speed;
        [SerializeField][Range(1.0f, 30.0f)] private float duration;
        [SerializeField][Range(1.0f, 10.0f)] private float area;
        [SerializeField][Range(1.0f, 30.0f)] private float cooldown;
        [SerializeField][Range(1, 30)] private int amount;
        [SerializeField] [Range(0, 30)] private int hits;
        
        
        [SerializeField] private string name;
        [SerializeField][TextArea] private string description;
    }
}
