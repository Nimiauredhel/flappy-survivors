using System;
using Gameplay.Configuration;
using UnityEngine;

namespace Gameplay.Weapons
{
    [Serializable]
    public class WeaponLogicSandbox
    {
        public virtual void Draw(WeaponInstance instance){}
        
        public virtual void Sheathe(WeaponInstance instance){}
        
        public virtual void HitHandler(object sender, Collider2D other, WeaponInstance instance){}

    }
}
