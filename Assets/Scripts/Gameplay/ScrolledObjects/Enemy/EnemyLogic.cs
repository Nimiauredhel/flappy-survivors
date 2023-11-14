using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.ScrolledObjects.Enemy
{
    [Serializable]
    public class EnemyLogic : IScrolledObjectLogic
    {
        private event EventHandler<int> EnemyKilled; 
        
        [SerializeField] private EnemyStats stats;

        private float currentHP;
        
        public EnemyLogic(EnemyStats stats, EventHandler<int> killedHandler)
        {
            this.stats = stats;
            EnemyKilled += killedHandler;
        }

        public void ScrolledObjectUpdate(ScrolledObjectView view)
        {
            
        }

        public void ScrolledObjectFixedUpdate(ScrolledObjectView view)
        {
            view.transform.Translate((new Vector2(-stats.Speed, 0.0f) * Time.fixedDeltaTime));
        }

        public void OnHitByWeapon(ScrolledObjectView view, float damage)
        {
            currentHP -= damage;

            if (currentHP <= 0.0f)
            {
                currentHP = 0.0f;
                EnemyKilled?.Invoke(view, stats.XPValue);
                view.Deactivate(true);
            }
        }

        public void OnHitByPlayer(ScrolledObjectView view, Action<float> hpAction, Action<int> xpAction)
        {
            hpAction?.Invoke(-stats.Power);
        }

        public void OnActivate()
        {
            currentHP = Random.Range(stats.MinHP, stats.MaxHP);
        }

        public void OnDeactivate()
        {
            
        }
    }
}
