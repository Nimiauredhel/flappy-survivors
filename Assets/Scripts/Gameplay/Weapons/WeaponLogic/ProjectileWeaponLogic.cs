using System;
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
        
        public override void Initialize(WeaponInstance instance)
        {
            projectilePool = new ObjectPool<WeaponView>(CreateProjectile);
        }

        public override void OnDispose(WeaponInstance instance)
        {
            
        }

        public override void Draw(WeaponInstance instance)
        {
            WeaponView projectile;
            projectilePool.Get(out projectile);

            if (projectile != null)
            {
                CancellationTokenSource token = new CancellationTokenSource();
                FireProjectile(instance, projectile, token);
            }
        }

        public override void Sheathe(WeaponInstance instance)
        {
            
        }

        public override void HitHandler(object sender, Collider2D other, WeaponInstance instance)
        {
            ScrolledObjectView SO = other.gameObject.GetComponentInParent<ScrolledObjectView>();

            if (SO != null && SO.Active)
            {
                SO.HitByWeapon(instance.Stats.BaseDamage);
            }
        }

        private async Task FireProjectile(WeaponInstance instance, WeaponView projectile, CancellationTokenSource token)
        {
            EventHandler<Collider2D> hitAction = delegate(object sender, Collider2D other)
            {
                token.Cancel();
                HitHandler(sender, other, instance);
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
                    projectile.transform.position += (Vector3.right * 16.0f * Time.deltaTime);
                    projectile.transform.Rotate(Vector3.forward, 540.0f * Time.deltaTime);
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
