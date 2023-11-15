using System;
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
        private CancellationTokenSource attackCancellationTokenSource = new CancellationTokenSource();
        
        public override void Draw(WeaponInstance instance)
        {
            AttackAsync(instance);
        }

        public override void Sheathe(WeaponInstance instance)
        {
            attackCancellationTokenSource.Cancel();
            hits = 0;
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

        private async Task AttackAsync(WeaponInstance instance)
        {
            hits = 0;
            instance.View.Graphic.enabled = true;
            instance.View.Hitbox.enabled = true;
            await Task.Delay(TimeSpan.FromSeconds(instance.Stats.Duration), attackCancellationTokenSource.Token);
            Sheathe(instance);
        }
    }
}
