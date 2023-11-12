using System;
using UnityEngine;

namespace Gameplay.Weapons
{
    [Serializable]
    public class WeaponLogicComponent
    {
        public virtual void Initialize(WeaponInstance instance){}
        
        public virtual void OnDispose(WeaponInstance instance){}
        
        public virtual void Draw(WeaponInstance instance){}
        
        public virtual void Sheathe(WeaponInstance instance){}
        
        public virtual void HitHandler(object sender, Collider2D other, WeaponInstance instance){}

    }
}
