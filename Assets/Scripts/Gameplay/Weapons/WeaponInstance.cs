using System;
using Gameplay.Configuration;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponInstance
    {
        public WeaponView View => view;
        public WeaponConfiguration Config => config;
        public WeaponStatus Status => status;
        
        private WeaponView view;
        private WeaponLogicSandbox logic;
        private WeaponConfiguration config;
        
        private WeaponStatus status;
        
        public class WeaponStatus
        {
            public float timeSinceActivated = 0.0f;

            public WeaponStatus(WeaponConfiguration configuration)
            {
                timeSinceActivated = configuration.Cooldown;
            }
        }

        public void Initialize(WeaponView view, WeaponConfiguration config, WeaponLogicSandbox logic)
        {
            this.view = view;
            this.config = config;
            this.logic = logic;
            status = new WeaponStatus(this.config);
            logic.Sheathe(this);

            view.TriggerEnter += HitHandler;
        }

        public void OnDispose()
        {
            view.TriggerEnter -= HitHandler;
        }

        public void WeaponUpdate(WeaponConfiguration.WeaponType validType)
        {
            bool activated = false;
            
            if (config.Type == validType || config.Type == WeaponConfiguration.WeaponType.Both)
            {
                if (status.timeSinceActivated > config.Cooldown)
                {
                    activated = true;
                    logic.Draw(this);
                }
            }
            else
            {
                logic.Sheathe(this);
            }

            if (activated)
            {
                status.timeSinceActivated = 0.0f;
            }
            else
            {
                status.timeSinceActivated += Time.deltaTime;
            }
        }

        private void HitHandler(object sender, Collider2D other)
        {
            logic.HitHandler(sender, other, this);
        }
    }
}
