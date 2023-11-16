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
        private int hits = 0;
        private float elapsedTime = 0.0f;
        private Coroutine weaponRoutine = null;
        
        public override void Draw(WeaponInstance instance)
        {
            if (weaponRoutine != null)
            {
                instance.View.StopCoroutine(weaponRoutine);
                weaponRoutine = null;
            }
            
            weaponRoutine = instance.View.StartCoroutine(AttackRoutine(instance));
        }

        public override void Sheathe(WeaponInstance instance)
        {
            if (weaponRoutine != null)
            {
                instance.View.StopCoroutine(weaponRoutine);
                weaponRoutine = null;
            }
            
            hits = 0;
            elapsedTime = 0.0f;
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

        private IEnumerator AttackRoutine(WeaponInstance instance)
        {
            hits = 0;
            elapsedTime = 0.0f;
            instance.View.Graphic.enabled = true;
            instance.View.Hitbox.enabled = true;

            while (elapsedTime <= instance.Stats.Duration)
            {
                yield return Constants.WaitForFixedUpdate;
                elapsedTime += Time.fixedDeltaTime;
            }
            
            Sheathe(instance);
        }
    }
}
