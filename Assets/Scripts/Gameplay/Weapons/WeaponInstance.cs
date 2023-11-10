using System;
using Gameplay.Data;
using UnityEngine;

namespace Gameplay.Weapons
{
    [Serializable]
    public class WeaponInstance
    {
        [SerializeField] private WeaponSandbox logic;
        [SerializeField] private WeaponData data;
        
        private WeaponState state;

        public void Initialize()
        {
            state = new WeaponState(data);
        }

        public void WeaponUpdate(WeaponData.WeaponType validType)
        {
            bool activated = false;
            
            if (data.Type == validType || data.Type == WeaponData.WeaponType.Both)
            {
                if (state.timeSinceActivated > data.Cooldown)
                {
                    activated = true;
                    logic.Draw(data);
                }
            }
            else
            {
                logic.Sheathe(data);
            }

            if (activated)
            {
                state.timeSinceActivated = 0.0f;
            }
            else
            {
                state.timeSinceActivated += Time.deltaTime;
            }
        }
    }

    public class WeaponState
    {
        public float timeSinceActivated = 0.0f;

        public WeaponState(WeaponData data)
        {
            timeSinceActivated = data.Cooldown;
        }
    }
}
