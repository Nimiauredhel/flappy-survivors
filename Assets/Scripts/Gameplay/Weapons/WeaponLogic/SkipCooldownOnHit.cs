using Gameplay.ScrolledObjects;
using JetBrains.Annotations;

namespace Gameplay.Weapons.WeaponLogic
{
    [UsedImplicitly]
    public class SkipCooldownOnHit : WeaponLogicComponent
    {
        private bool skipCooldown = false;
        
        public override void Draw(WeaponInstance instance)
        {
            skipCooldown = false;
        }

        public override void Sheathe(WeaponInstance instance)
        {
            if (skipCooldown)
            {
                instance.Status.currentCharge = instance.Stats.ChargeCapacity;
            }
        }

        public override void HitHandler(ScrolledObjectView hitObject, WeaponInstance instance)
        {
            skipCooldown = true;
        }
    }
}