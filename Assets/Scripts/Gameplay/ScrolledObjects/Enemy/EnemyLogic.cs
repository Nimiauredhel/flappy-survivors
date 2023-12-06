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
        private event Action<bool, int, int, Vector3> EnemyHit; 
        
        private int currentHP;
        private float elapsedTime = 0.0f;
        private float expectedTime;
        private EnemyStats stats;
        private Spline path = null;
        
        public EnemyLogic(EnemyStats stats, Action<bool, int, int, Vector3> hitHandler)
        {
            this.stats = stats;
            EnemyHit += hitHandler;
        }

        public void ScrolledObjectUpdate(ScrolledObjectView view)
        {
            elapsedTime += Time.deltaTime;
        }

        public void ScrolledObjectFixedUpdate(ScrolledObjectView view)
        {
            if (path == null)
            {
                view.Body.MovePosition(view.Body.position + new Vector2(-stats.Speed, 0.0f) * Time.fixedDeltaTime);
            }
            else
            {
                
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
            int value = stats.XPValue;
            bool killed = false;
            
            if (currentHP <= 0.0f)
            {
                killed = true;
                int overkill = damage - stats.MaxHP;
                
                if (overkill > stats.MaxHP || Random.Range(0.0f, 1.0f) > 0.85f)
                {
                    value *= -2;
                }
                
                currentHP = 0;
                view.Deactivate();
            }
            
            EnemyHit?.Invoke(killed, damage, value, view.transform.position);
        }

        public void OnHitPlayer(ScrolledObjectView view, Action<int> hpAction, Action<int> xpAction, Action<UpgradeOption> upgradeOption)
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
