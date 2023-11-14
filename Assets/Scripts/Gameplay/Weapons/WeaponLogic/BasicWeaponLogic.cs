using System;
using System.Threading.Tasks;
using Gameplay.ScrolledObjects;
using JetBrains.Annotations;
using UnityEngine;

namespace Gameplay.Weapons.WeaponLogic
{
    [UsedImplicitly]
    public class BasicWeaponLogic : WeaponLogicComponent
    {
        public override void Draw(WeaponInstance instance)
        {
            AttackAsync(instance);
        }

        public override void Sheathe(WeaponInstance instance)
        {
            instance.View.Graphic.enabled = false;
            instance.View.Hitbox.enabled = false;
        }

        public override void HitHandler(ScrolledObjectView hitObject, WeaponInstance instance)
        {
                hitObject.HitByWeapon(instance.Stats.Power);
        }

        private async Task AttackAsync(WeaponInstance instance)
        {
            instance.View.Graphic.enabled = true;
            instance.View.Hitbox.enabled = true;
            await Task.Delay(TimeSpan.FromSeconds(instance.Stats.Duration));
            Sheathe(instance);
        }
    }
}
