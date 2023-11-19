using System;
using Configuration;
using Gameplay.Weapons.WeaponLogic;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponInstance
    {
        public readonly WeaponView View;
        public readonly WeaponStats Stats;
        public readonly WeaponStatus Status;
        
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
            WeaponUIView uiView)
        {
            View = view;
            Stats = stats;
            Status = new WeaponStatus(Stats);

            this.uiView = uiView;
            this.logic = logic;
            this.logic.Initialize(this);
            this.logic.Sheathe(this);
            
            View.SetHitArea(Stats.Area);
            View.TriggerEnter += HitHandler;
        }

        public void OnDispose()
        {
            logic.OnDispose(this);
            View.TriggerEnter -= HitHandler;
        }

        public void WeaponUpdate(WeaponType validType)
        {
            // Skip this if Game Phase is either Intro or Upgrading
            if ((int)GameModel.CurrentGamePhase < 2) return;
            
            logic.OnUpdate(this);
            uiView.UpdateCooldownIndicator(1.0f-(Status.timeSinceActivated/Stats.Cooldown));
        }

        public void WeaponFixedUpdate(WeaponType validType)
        {
            // Skip this if Game Phase is either Intro or Upgrading
            if ((int)GameModel.CurrentGamePhase < 2) return;
            
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

        public void ApplyUpgrade(WeaponConfiguration upgradeConfig)
        {
            Stats.ApplyUpgrade(upgradeConfig.Stats);
            View.SetHitArea(Stats.Area);
            logic.IncorporateLogicUpgrade(WeaponLogicBuilder.BuildWeaponLogicComponents(upgradeConfig.LogicComponents));
        }

        private void HitHandler(object sender, Collider2D other)
        {
            logic.HitHandler(sender, other, this);
        }
    }
}
