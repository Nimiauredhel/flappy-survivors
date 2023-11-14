using System;
using Gameplay.Configuration;
using Gameplay.Weapons.WeaponLogic;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponInstance
    {
        public readonly WeaponView View;
        public readonly WeaponStats Stats;
        public readonly WeaponStatus Status;
        public readonly WeaponConfiguration NextLevel;
        
        private readonly WeaponLogicEntity logic;
        private readonly WeaponUIView uiView;
        
        public class WeaponStatus
        {
            public float timeSinceActivated = 0.0f;

            public WeaponStatus(WeaponStats stats)
            {
                timeSinceActivated = stats.Cooldown;
            }
        }

        public WeaponInstance(WeaponView view, WeaponStats stats, WeaponLogicEntity logic,
            WeaponConfiguration nextLevel, WeaponUIView uiView)
        {
            View = view;
            Stats = stats;
            Status = new WeaponStatus(Stats);

            this.uiView = uiView;
            this.logic = logic;
            this.logic.Initialize(this);
            this.logic.Sheathe(this);
            View.TriggerEnter += HitHandler;
        }

        public void OnDispose()
        {
            logic.OnDispose(this);
            View.TriggerEnter -= HitHandler;
        }

        public void WeaponUpdate(WeaponType validType)
        {
            logic.OnUpdate(this);
            uiView.UpdateCooldownIndicator(1.0f-(Status.timeSinceActivated/Stats.Cooldown));
        }

        public void WeaponFixedUpdate(WeaponType validType)
        {
            bool activated = false;
            
            if (Stats.Type == validType || Stats.Type == WeaponType.Both)
            {
                if (Status.timeSinceActivated >= Stats.Cooldown)
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
                Status.timeSinceActivated = 0.0f;
            }
            else
            {
                Status.timeSinceActivated += Time.fixedDeltaTime;
            }
            
            logic.OnFixedUpdate(this);
        }

        private void HitHandler(object sender, Collider2D other)
        {
            logic.HitHandler(sender, other, this);
        }
    }
}
