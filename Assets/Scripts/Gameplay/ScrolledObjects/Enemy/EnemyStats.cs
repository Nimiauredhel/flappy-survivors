using System;
using UnityEngine;

namespace Gameplay.ScrolledObjects.Enemy
{
    [Serializable]
    public class EnemyStats
    {
        public int XPValue => xpValue;
        public float MaxHP => maxHP;
        public float MinHP => minHP;
        public float Power => power;
        public float Speed => speed;
    
        [SerializeField] private int xpValue;
        [SerializeField] private float maxHP;
        [SerializeField] private float minHP;
        [SerializeField] private float power;
        [SerializeField] private float speed;
    }
}
