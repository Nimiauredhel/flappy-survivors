using System;
using System.Collections;
using Gameplay.ScrolledObjects;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Gameplay.Weapons.WeaponLogic
{
    [UsedImplicitly]
    public class SheatheProjectileWeaponLogic : WeaponLogicComponent
    {
        private ObjectPool<WeaponView> projectilePool;
        private ObjectPool<GameObject> hitEffectPool = null;
        private WaitForSeconds projectileGap = new WaitForSeconds(0.15f);
        
        public override void Initialize(WeaponInstance instance)
        {
            projectilePool = new ObjectPool<WeaponView>(delegate
            {
                WeaponView projectileInstance = Object.Instantiate<WeaponView>(instance.View.ProjectilePrefab);
                projectileInstance.Graphic.enabled = false;
                projectileInstance.Hitbox.enabled = false;
                return projectileInstance;
            }, null, ReleaseProjectile);

            if (instance.View.HitEffectPrefab != null)
            {
                hitEffectPool = new ObjectPool<GameObject>(
                    delegate
                    {
                        GameObject go = Object.Instantiate(instance.View.HitEffectPrefab);
                        go.SetActive(false);
                        return go;
                    });
            }
        }

        public override void OnDispose(WeaponInstance instance)
        {
            projectilePool.Dispose();
            
            if (hitEffectPool != null) hitEffectPool.Dispose();
        }

        public override void Draw(WeaponInstance instance)
        {
            
        }

        public override void Sheathe(WeaponInstance instance)
        {
            if (instance.Status.currentCharge <= instance.Stats.ChargeUseThreshold)
            {
                instance.View.StartCoroutine(FireMultipleProjectiles(instance));
            }
        }

        public override void HitHandler(ScrolledObjectView hitObject, WeaponInstance instance)
        {
            if (hitEffectPool != null)
            {
                _ = DoHitEffect(hitObject.transform.position, instance.View.transform.position);
            }
            
            hitObject.HitByWeapon(instance.Stats.Power);
        }

        internal IEnumerator FireMultipleProjectiles(WeaponInstance instance)
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

        private void ReleaseProjectile(WeaponView projectile)
        {
            projectile.StopAllCoroutines();
            projectile.ResetEventSubscription();
            projectile.Graphic.enabled = false;
            projectile.Hitbox.enabled = false;
        }
        
        private async Awaitable DoHitEffect(Vector3 position, Vector3 origin)
        {
            GameObject effect = hitEffectPool.Get();
            Quaternion rotation = Quaternion.LookRotation(
                position - origin,
                effect.transform.TransformDirection(Vector3.up));
            effect.transform.position = position;
            effect.transform.rotation = new Quaternion( 0 , 0 , rotation.z , rotation.w );
            effect.SetActive(true);
            await Awaitable.WaitForSecondsAsync(5.0f);
            effect.SetActive(false);
            hitEffectPool.Release(effect);
        }
    }
}