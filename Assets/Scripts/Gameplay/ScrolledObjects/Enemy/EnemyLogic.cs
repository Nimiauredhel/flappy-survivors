using System;
using Gameplay.Upgrades;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.ScrolledObjects.Enemy
{
    public class EnemyLogic : IScrolledObjectLogic
    {
        private event Action<int, Vector3> EnemyKilled; 
        
        private int currentHP;
        private EnemyStats stats;
        
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

        public void OnHitByWeapon(ScrolledObjectView view, int damage)
        {
            currentHP -= damage;

            if (currentHP <= 0.0f)
            {
                int overkill = damage - stats.MaxHP;
                int pickupValue = stats.XPValue;
                
                if (overkill > stats.MaxHP || Random.Range(0.0f, 1.0f) > 0.85f)
                {
                    pickupValue *= -2;
                }
                
                EnemyKilled?.Invoke(pickupValue, view.transform.position);
                
                currentHP = 0;
                view.Deactivate(true);
            }
        }

        public void OnHitByPlayer(ScrolledObjectView view, Action<int> hpAction, Action<int> xpAction, Action<UpgradeOption> upgradeOption)
        {
            hpAction?.Invoke(-stats.Power);
        }

        public void OnActivate(ScrolledObjectView view, object value)
        {
            currentHP = Random.Range(stats.MinHP, stats.MaxHP);
        }

        public void OnDeactivate(ScrolledObjectView view)
        {
            
        }
    }
}
