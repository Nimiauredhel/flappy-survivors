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
        [SerializeField][Range(0.0f, 30.0f)] private float cooldown = 1.0f;
        [SerializeField][Range(0, 30)] private int amount = 1;
        [SerializeField] [Range(0, 30)] private int hits = 0;
        
        
        [SerializeField] private string name;
        [SerializeField][TextArea] private string description;
    }
}
