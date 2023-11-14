using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.ScrolledObjects.Enemy
{
    public class EnemyLogic : IScrolledObjectLogic
    {
        private event Action<int, Vector3> EnemyKilled; 
        
        private EnemyStats stats;

        private float currentHP;
        
        public EnemyLogic(EnemyStats stats, Action<int, Vector3> killedHandler)
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
                EnemyKilled?.Invoke(stats.XPValue, view.transform.position);
                view.Deactivate(true);
            }
        }

        public void OnHitByPlayer(ScrolledObjectView view, Action<float> hpAction, Action<int> xpAction)
        {
            hpAction?.Invoke(-stats.Power);
        }

        public void OnActivate(int value = 0)
        {
            currentHP = Random.Range(stats.MinHP, stats.MaxHP);
        }

        public void OnDeactivate()
        {
            
        }
    }
}
