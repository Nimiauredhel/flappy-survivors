using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gameplay.ScrolledObjects;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Gameplay.Weapons.WeaponLogic
{
    [UsedImplicitly]
    public class ProjectileWeaponLogic : WeaponLogicComponent
    {
        private ObjectPool<WeaponView> projectilePool;
        private WaitForSeconds projectileGap = new WaitForSeconds(0.15f);
        
        public override void Initialize(WeaponInstance instance)
        {
            projectilePool = new ObjectPool<WeaponView>(CreateProjectile, null, ReleaseProjectile);
        }

        public override void OnDispose(WeaponInstance instance)
        {
            projectilePool.Dispose();
        }

        public override void Draw(WeaponInstance instance)
        {
            instance.View.StartCoroutine(FireMultipleProjectiles(instance));
        }

        public override void Sheathe(WeaponInstance instance)
        {
            
        }

        public override void HitHandler(ScrolledObjectView hitObject, WeaponInstance instance)
        {
            hitObject.HitByWeapon(instance.Stats.Power);
        }

        private IEnumerator FireMultipleProjectiles(WeaponInstance instance)
        {
            for (int i = 0; i < instance.Stats.Amount; i++)
            {
                projectilePool.Get(out WeaponView projectile);

                if (projectile != null)
                {
                    projectile.transform.position = instance.View.transform.position;
                    projectile.SetHitArea(instance.Stats.Area);
                    projectile.Graphic.enabled = true;
                    projectile.Hitbox.enabled = true;
                    projectile.transform.Rotate(Vector3.forward, Random.Range(-180.0f, 180.0f));
                    
                    projectile.PlayDrawSound();
                    projectile.StartCoroutine(FireSingleProjectile(instance, projectile));
                    yield return projectileGap;
                }
            }
        }

        private IEnumerator FireSingleProjectile(WeaponInstance instance, WeaponView projectile)
        {
            int hits = 0;
            
            EventHandler<Collider2D> hitAction = delegate(object sender, Collider2D other)
            {
                ScrolledObjectView SO = other.GetComponentInParent<ScrolledObjectView>();
                
                if (SO != null)
                {
                    if (instance.Stats.Hits > 0)
                    {
                        hits++;
                    }
                    
                    HitHandler(SO, instance);
                    
                    if (hits >= instance.Stats.Hits)
                    {
                        projectilePool.Release(projectile);
                    }
                }
            };
            
            projectile.TriggerEnter += hitAction;
            
            float time = 0.0f;

            while (time < instance.Stats.Duration)
            {
                float fixedDeltaTime = Time.fixedDeltaTime;
                projectile.transform.position += (Vector3.right * (instance.Stats.Speed * fixedDeltaTime));
                projectile.transform.Rotate(Vector3.forward, 30.0f * instance.Stats.Speed * fixedDeltaTime);
                time += fixedDeltaTime;
                yield return Constants.WaitForFixedUpdate;
            }
            
            projectilePool.Release(projectile);
        }
        
        private WeaponView CreateProjectile()
        {
            WeaponView projectileInstance = Object.Instantiate<WeaponView>(Resources.Load<WeaponView>("Projectiles/BasicProjectile"));
            projectileInstance.Graphic.enabled = false;
            projectileInstance.Hitbox.enabled = false;
            return projectileInstance;
        }

        private void ReleaseProjectile(WeaponView projectile)
        {
            projectile.StopAllCoroutines();
            projectile.ResetEventSubscription();
            projectile.Graphic.enabled = false;
            projectile.Hitbox.enabled = false;
        }
    }
}
