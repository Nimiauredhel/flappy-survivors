using System;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Gameplay.ScrolledObjects.Enemy
{
    public class EnemyLogic : IScrolledObjectLogic
    {
        private event Action<bool, int, int, SpriteRenderer[]> EnemyHit; 
        
        private int currentHP;
        private float elapsedTime = 0.0f;
        private float expectedTime;
        private EnemyStats stats;
        private Spline path = null;
        private bool rotateWithPath = true;
        private bool rotationForbidden = false;
        
        public EnemyLogic(EnemyStats stats, Action<bool, int, int, SpriteRenderer[]> hitHandler, bool rotateWithPath)
        {
            this.stats = stats;
            this.rotateWithPath = rotateWithPath;
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
                if (percent > 10.0f)
                {
                    _ = view.Deactivate();
                    return;
                }

                Vector2 targetPosition = path.EvaluatePosition(percent).xy;
                
                targetPosition += Constants.STAGE_OFFSET;
                view.Body.MovePosition(targetPosition);
                
                if (rotateWithPath & !rotationForbidden)
                {
                    Vector3 targetRotation = -path.EvaluateAcceleration(percent).zzy;
                    view.transform.rotation = (Quaternion.Euler(targetRotation));
                }
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
                _ = view.Deactivate();
            }
            
            EnemyHit?.Invoke(killed, damage, value, view.Graphics);
        }

        public void OnHitPlayer(ScrolledObjectView view, Action<int> hpAction, Action<int> xpAction, Action<UpgradeOption> upgradeOption)
        {
            hpAction?.Invoke(-stats.Power);
        }

        public void OnActivate(ScrolledObjectView view, object value, bool forbidPathRotation = false, float speedOverride = 0.0f)
        {
            rotationForbidden = forbidPathRotation;
            elapsedTime = 0.0f;
            expectedTime = Constants.STAGE_WIDTH /
                           (speedOverride == 0.0f ? stats.Speed : speedOverride);
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
