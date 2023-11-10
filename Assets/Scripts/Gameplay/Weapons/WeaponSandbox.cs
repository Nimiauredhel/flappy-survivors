using Gameplay.Data;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponSandbox : MonoBehaviour
    {
        public virtual void Draw(WeaponData weaponData){}
        
        public virtual void Sheathe(WeaponData weaponData){}

    }
}
