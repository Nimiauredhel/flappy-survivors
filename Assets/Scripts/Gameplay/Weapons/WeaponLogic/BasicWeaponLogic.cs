using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Gameplay.ScrolledObjects;
using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Gameplay.Weapons.WeaponLogic
{
    [UsedImplicitly]
    public class BasicWeaponLogic : WeaponLogicComponent
    {
        private bool drawn = true;
        private int hits = 0;
        private float elapsedTime = 0.0f;
        private Awaitable weaponRoutine = null;
        
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
    }
}
