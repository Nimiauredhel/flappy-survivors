using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private const float projectileGap = 0.15f;

        private List<CancellationTokenSource> projectileTokenSources = new List<CancellationTokenSource>(8);
        
        public override void Initialize(WeaponInstance instance)
        {
            projectilePool = new ObjectPool<WeaponView>(CreateProjectile, null, ReleaseProjectile);
        }

        public override void OnDispose(WeaponInstance instance)
        {
            foreach (CancellationTokenSource tokenSource in projectileTokenSources)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
            
            projectilePool.Dispose();
        }

        public override void Draw(WeaponInstance instance)
        {
            FireMultipleProjectiles(instance);
        }

        public override void Sheathe(WeaponInstance instance)
        {
            
        }

        public override void HitHandler(ScrolledObjectView hitObject, WeaponInstance instance)
        {
            hitObject.HitByWeapon(instance.Stats.Power);
        }

        private async Awaitable FireMultipleProjectiles(WeaponInstance instance)
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
                    projectile.gameObject.SetActive(true);
                    projectile.transform.Rotate(Vector3.forward, Random.Range(-180.0f, 180.0f));

                    CancellationTokenSource newTokenSource = new CancellationTokenSource();
                    projectileTokenSources.Add(newTokenSource);
                    FireSingleProjectile(instance, projectile, newTokenSource);
                    await Awaitable.WaitForSecondsAsync(projectileGap);
                }
            }
        }

        private async Task FireSingleProjectile(WeaponInstance instance, WeaponView projectile, CancellationTokenSource tokenSource)
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
                        tokenSource.Cancel();
                        //projectilePool.Release(projectile);
                    }
                }
            };
        
            projectile.TriggerEnter += hitAction;
        
            float time = 0.0f;

            try
            {
                while (!tokenSource.IsCancellationRequested && time < instance.Stats.Duration)
                {
                    float fixedDeltaTime = Time.fixedDeltaTime;
                    projectile.transform.position += (Vector3.right * (instance.Stats.Speed * fixedDeltaTime));
                    projectile.transform.Rotate(Vector3.forward, 30.0f * instance.Stats.Speed * fixedDeltaTime);
                    time += fixedDeltaTime;
                    await Awaitable.FixedUpdateAsync();
                }

                projectilePool.Release(projectile);
            }
            catch (OperationCanceledException e)
            {
                projectilePool.Release(projectile);
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                projectilePool.Release(projectile);
                projectileTokenSources.Remove(tokenSource);
                tokenSource.Cancel();
            }
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
            projectile.ResetEventSubscription();
            projectile.Graphic.enabled = false;
            projectile.Hitbox.enabled = false;
            projectile.gameObject.SetActive(false);
        }
    }
}
