using System;
using UnityEngine;

namespace Gameplay.ScrolledObjects.Enemy
{
    [Serializable]
    public class EnemyStats
    {
        public int XPValue => xpValue;
        public int MaxHP => maxHP;
        public int MinHP => minHP;
        public int Power => power;
        public float Speed => speed;
    
        [SerializeField] private int xpValue;
        [SerializeField] private int maxHP;
        [SerializeField] private int minHP;
        [SerializeField] private int power;
        [SerializeField] private float speed;
    }
}
