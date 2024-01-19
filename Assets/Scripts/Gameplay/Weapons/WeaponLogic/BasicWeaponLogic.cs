using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Gameplay.ScrolledObjects;
using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Gameplay.Weapons.WeaponLogic
{
    [UsedImplicitly]
    public class BasicWeaponLogic : WeaponLogicComponent
    {
        private bool drawn = true;
        private int hits = 0;
        private float elapsedTime = 0.0f;
        private Awaitable weaponRoutine = null;

        private ObjectPool<GameObject> hitEffectPool = null;

        public override void Initialize(WeaponInstance instance)
        {
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

        public override void Draw(WeaponInstance instance)
        {
            if (drawn) return;
            drawn = true;
            
            if (weaponRoutine != null)
            {
                weaponRoutine.Cancel();
                weaponRoutine = null;
            }
            else
            {
                instance.View.PlayDrawSound();
            }
            
            weaponRoutine = AttackRoutine(instance);
        }

        public override void Sheathe(WeaponInstance instance)
        {
            if (!drawn) return;
            drawn = false;
            
            if (weaponRoutine != null)
            {
                instance.View.StopCoroutine(weaponRoutine);
                weaponRoutine = null;
            }
            else
            {
                instance.View.PlaySheatheSound();
            }
            
            hits = 0;
            elapsedTime = 0.0f;
            instance.View.Animator.Rebind();
            instance.View.Animator.Update(0.0f);
            instance.View.Graphic.enabled = false;
            instance.View.Hitbox.enabled = false;
        }

        public override void HitHandler(ScrolledObjectView hitObject, WeaponInstance instance)
        {
            if (hitEffectPool != null)
            {
                _ = DoHitEffect(Vector3.Lerp(instance.View.transform.position, hitObject.transform.position, 0.8f));
            }

            hitObject.HitByWeapon(instance.Stats.Power);

            if (instance.Stats.Hits > 0)
            {
                hits++;

                if (hits >= instance.Stats.Hits)
                {
                    Sheathe(instance);
                }
            }
        }

        private async Awaitable AttackRoutine(WeaponInstance instance)
        {
            hits = 0;
            elapsedTime = 0.0f;
            instance.View.Graphic.enabled = true;
            instance.View.Hitbox.enabled = true;

            while (elapsedTime <= instance.Stats.Duration)
            {
                await Awaitable.FixedUpdateAsync();
                elapsedTime += Time.fixedDeltaTime;
            }
            
            Sheathe(instance);
        }

        private async Awaitable DoHitEffect(Vector3 position)
        {
            GameObject effect = hitEffectPool.Get();
            effect.transform.position = position;
            effect.SetActive(true);
            await Awaitable.WaitForSecondsAsync(2.0f);
            effect.SetActive(false);
            hitEffectPool.Release(effect);
        }
    }
}
