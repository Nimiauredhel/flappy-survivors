using System;
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
        private List<CancellationTokenSource> projectileTokenSources = new List<CancellationTokenSource>(8);
        
        public override void Initialize(WeaponInstance instance)
        {
            projectilePool = new ObjectPool<WeaponView>(CreateProjectile);
        }

        public override void OnDispose(WeaponInstance instance)
        {
            foreach (CancellationTokenSource tokenSource in projectileTokenSources)
            {
                tokenSource.Cancel();
            }
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

        private async void FireMultipleProjectiles(WeaponInstance instance)
        {
            for (int i = 0; i < instance.Stats.Amount; i++)
            {
                projectilePool.Get(out WeaponView projectile);

                if (projectile != null)
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    projectileTokenSources.Add(tokenSource);
                    
                    FireSingleProjectile(instance, projectile, tokenSource);
                    await Task.Delay(100);
                }
            }
        }

        private async Task FireSingleProjectile(WeaponInstance instance, WeaponView projectile, CancellationTokenSource token)
        {
            EventHandler<Collider2D> hitAction = delegate(object sender, Collider2D other)
            {
                ScrolledObjectView SO = other.GetComponentInParent<ScrolledObjectView>();

                if (SO != null)
                {
                    HitHandler(SO, instance);
                    token.Cancel();
                }
            };
            
            projectile.TriggerEnter += hitAction;
            
            float time = 0.0f;

            projectile.transform.position = instance.View.transform.position;
            projectile.Graphic.enabled = true;
            projectile.Hitbox.enabled = true;
            projectile.transform.Rotate(Vector3.forward, Random.Range(-180.0f, 180.0f));
            
            try
            {
                while (time < instance.Stats.Duration && !token.IsCancellationRequested)
                {
                    float deltaTime = Time.deltaTime;
                    projectile.transform.position += (Vector3.right * 16.0f * deltaTime);
                    projectile.transform.Rotate(Vector3.forward, 540.0f * deltaTime);
                    time += deltaTime;
                    await Task.Yield();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                projectile.TriggerEnter -= hitAction;
                projectileTokenSources.Remove(token);
                projectile.Graphic.enabled = false;
                projectile.Hitbox.enabled = false;
                projectilePool.Release(projectile);
            }
        }
        
        private WeaponView CreateProjectile()
        {
            WeaponView projectileInstance = Object.Instantiate<WeaponView>(Resources.Load<WeaponView>("Projectiles/BasicProjectile"));
            projectileInstance.Graphic.enabled = false;
            projectileInstance.Hitbox.enabled = false;
            return projectileInstance;
        }
    }
}
