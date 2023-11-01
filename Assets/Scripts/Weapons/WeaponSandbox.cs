using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public class WeaponSandbox : MonoBehaviour
    {
        protected float cooldownCounter = 0.0f;
        protected float durationCounter = 0.0f;
        
        public virtual void DoAttack()
        {
        }

    }
}
