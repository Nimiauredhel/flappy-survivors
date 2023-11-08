using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public class WeaponSandbox : MonoBehaviour
    {
        public virtual void Draw(WeaponData weaponData){}
        
        public virtual void Sheathe(WeaponData weaponData){}

    }
}
