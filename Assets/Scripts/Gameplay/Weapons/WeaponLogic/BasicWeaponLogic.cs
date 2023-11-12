using System;
using System.Threading;
using System.Threading.Tasks;
using Gameplay.Configuration;
using JetBrains.Annotations;
using UnityEngine;

namespace Gameplay.Weapons
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

        public override void HitHandler(object sender, Collider2D other, WeaponInstance instance)
        {
            ScrolledObject SO = other.gameObject.GetComponentInParent<ScrolledObject>();

            if (SO != null && SO.Active)
            {
                SO.TakeDamage(instance.Stats.BaseDamage);
            }
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
