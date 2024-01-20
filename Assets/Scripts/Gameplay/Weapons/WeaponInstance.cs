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

        private bool drawn = false;
        
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

        public void WeaponUpdate(PlayerState validType)
        {
            // Skip this if Game Phase is not relevant
            int gamePhase = (int)GameModel.CurrentGamePhase;
            if (gamePhase < 2 || gamePhase > 3) return;
            
            logic.OnUpdate(this);
            uiView.UpdateCooldownIndicator(1.0f-(Status.currentCharge/Stats.ChargeCapacity));
        }

        public void WeaponFixedUpdate(PlayerState playerCurrentState)
        {
            // Skip this if Game Phase is not relevant
            int gamePhase = (int)GameModel.CurrentGamePhase;
            if (gamePhase is < 2 or > 3) return;
            
            float fixedDeltaTime = Time.fixedDeltaTime;
            
            //TODO: simplify the logic here that got a bit convoluted after changing "cooldown" to "charge"
            
            if (Status.currentCharge <= 0.0f)
            {
                Sheathe();
            }
            else if (Stats.DrawState == playerCurrentState || Stats.DrawState == PlayerState.Both)
            {
                if (Status.currentCharge >= Stats.ChargeUseThreshold)
                {
                    Draw();
                }
            }
            else
            {
                Sheathe();
            }

            if (drawn)
            {
                if (Stats.ChargeDepletionRate > 0)
                {
                    Status.currentCharge -= Stats.ChargeDepletionRate * fixedDeltaTime;
                }
                else if (Status.currentCharge >= Stats.ChargeUseThreshold)
                {
                    Status.currentCharge = 0;
                }
            }
            else if (Stats.ChargeState == playerCurrentState || Stats.ChargeState == PlayerState.Both)
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

        private void Draw()
        {
            if (drawn) return;
            drawn = true;
            logic.Draw(this);
        }

        private void Sheathe()
        {
            if (!drawn) return;
            drawn = false;
            logic.Sheathe(this);
        }
    }
}
