using System;
using Gameplay.ScrolledObjects;

namespace Gameplay.Weapons.WeaponLogic
{
    [Serializable]
    public class WeaponLogicComponent
    {
        public virtual void Initialize(WeaponInstance instance){}
        
        public virtual void OnDispose(WeaponInstance instance){}
        
        public virtual void Draw(WeaponInstance instance){}
        
        public virtual void Sheathe(WeaponInstance instance){}
        
        public virtual void HitHandler(ScrolledObjectView hitObject, WeaponInstance instance){}

    }
}
