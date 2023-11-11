using System;
using UnityEngine;

namespace Gameplay
{
    public class ScrolledObject : MonoBehaviour
    {
        public bool Active => active;
        public float Health => health;
        public float MeleeDamage => meleeDamage;
        public int Value => value;
        public ScrolledObjectType ObjectType => objectType;
        public event EventHandler<int> EnemyKilled; 

        private bool active = false;
        [SerializeField] private int value;
        [SerializeField] private float health;
        [SerializeField] private float meleeDamage;
        [SerializeField] private ScrolledObjectType objectType;
        [SerializeField] private SpriteRenderer graphic;
        [SerializeField] private GameObject explosion;
    
        public enum ScrolledObjectType
        {
            None,
            Enemy,
            Experience,
        
        }

        public void TakeDamage(float damage)
        {
            health -= damage;

            if (health <= 0.0f)
            {
                health = 0.0f;
                Die();
            }
        }

        public void Activate()
        {
            graphic.gameObject.SetActive(true);
            explosion.SetActive(false);
            active = true;
        }
        
        public void Deactivate()
        {
            graphic.gameObject.SetActive(false);
            active = false;
        }

        private void Die()
        {
            explosion.SetActive(true);
            Deactivate();
            
            EnemyKilled?.Invoke(this, value);
        }
    }
}
