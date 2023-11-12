using System;
using System.Collections.Generic;
using TypeReferences;

namespace Gameplay.Weapons.WeaponLogic
{
    public static class WeaponLogicBuilder
    {
        public static WeaponLogicEntity BuildWeaponLogicEntity(TypeReference[] componentTypes)
        {
            List<WeaponLogicComponent> components = new List<WeaponLogicComponent>(4);

            for (int i = 0; i < componentTypes.Length; i++)
            {
                var componentInstance = Activator.CreateInstance(componentTypes[i]);
                components.Add(componentInstance as WeaponLogicComponent);
            }

            return new WeaponLogicEntity(components.ToArray());
        }
    }
}
