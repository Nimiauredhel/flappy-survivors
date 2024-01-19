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
            public float currentCharge = 0.0f;

            public WeaponStatus(WeaponStats stats)
            {
                currentCharge = stats.ChargeCapacity;
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
            // Skip this if Game Phase is not relevant
            int gamePhase = (int)GameModel.CurrentGamePhase;
            if (gamePhase < 2 || gamePhase > 3) return;
            
            logic.OnUpdate(this);
            uiView.UpdateCooldownIndicator(1.0f-(Status.currentCharge/Stats.ChargeCapacity));
        }

        public void WeaponFixedUpdate(WeaponType validType)
        {
            // Skip this if Game Phase is not relevant
            int gamePhase = (int)GameModel.CurrentGamePhase;
            if (gamePhase < 2 || gamePhase > 3) return;
            
            bool activated = false;
            float fixedDeltaTime = Time.fixedDeltaTime;
            
            if (Stats.Type == validType || Stats.Type == WeaponType.Both)
            {
                if (Status.currentCharge >= Stats.ChargeUseThreshold)
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
                Status.currentCharge -= Stats.ChargeDepletionRate;
            }
            else
            {
                Status.currentCharge += Stats.ChargeReplenishmentRate * fixedDeltaTime;
            }

            Status.currentCharge = Mathf.Clamp(Status.currentCharge, 0.0f, Stats.ChargeCapacity);
            
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
