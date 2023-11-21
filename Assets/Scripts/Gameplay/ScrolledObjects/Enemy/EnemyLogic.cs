using System;
using Gameplay.Upgrades;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Gameplay.ScrolledObjects.Enemy
{
    public class EnemyLogic : IScrolledObjectLogic
    {
        private event Action<int, Vector3> EnemyKilled; 
        
        private int currentHP;
        private float elapsedTime = 0.0f;
        private float expectedTime;
        private EnemyStats stats;
        private Spline path = null;
        
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
            if (path == null)
            {
                view.Body.MovePosition(view.Body.position + new Vector2(-stats.Speed, 0.0f) * Time.fixedDeltaTime);
            }
            else
            {
                elapsedTime += Time.fixedDeltaTime;
                float percent = elapsedTime / expectedTime;

                //failsafe for when the spline messes with the pooling
                if (percent > 3.0f)
                {
                    view.Deactivate();
                    return;
                }

                Vector2 targetPosition = path.EvaluatePosition(percent).xy;
                Vector3 targetRotation = -path.EvaluateAcceleration(percent).zzy;
                targetPosition += Constants.STAGE_OFFSET;
                view.Body.MovePosition(targetPosition);
                view.transform.rotation = (Quaternion.Euler(targetRotation));
            }
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
            elapsedTime = 0.0f;
            expectedTime = Constants.STAGE_WIDTH / stats.Speed;
            currentHP = Random.Range(stats.MinHP, stats.MaxHP);
        }

        public void OnDeactivate(ScrolledObjectView view)
        {
            elapsedTime = 0.0f;
            path = null;
        }

        public void SetPath(Spline path)
        {
            this.path = path;
        }
    }
}
