using UnityEngine;

namespace Gameplay
{
    public class ScrolledObject : MonoBehaviour
    {
        public bool Active => active;
        public float Health => health;
        public float MeleeDamage => meleeDamage;
        public float Value => value;
        public ScrolledObjectType ObjectType => objectType;

        private bool active = false;
        [SerializeField] private float value;
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
            //TODO: call event to give player XP
            explosion.SetActive(true);
            Deactivate();
        }
    }
}
