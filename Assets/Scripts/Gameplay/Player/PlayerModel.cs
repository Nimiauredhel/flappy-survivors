using System;
using Configuration;
using Gameplay.Upgrades;
using UnityEngine;
using VContainer;

namespace Gameplay.Player
{
    public class PlayerModel
    {
        public int CurrentLevel => currentLevel;
        public int TotalXP => totalXp;
        public int NextLevelXPReq => nextLevelXpReq;
        public int PreviousLevelXPReq => previousLevelXpReq;
        public int MaxHealth => stats.MaxHealth;
        public int CurrentHealth => currentHealth;
        public float CurrentXSpeed => currentXSpeed;
        public float CurrentYSpeed => currentYSpeed;
        public UpgradeTree UpgradeTree => upgradeTree;

        public event Action<float> HealthPercentChanged;
        public event Action<float> XPPercentChanged;
        
        private int currentLevel = 1;
        private int totalXp = 0;
        private int nextLevelXpReq = 5;
        private int previousLevelXpReq = 0;
        
        private int currentHealth = 100;
        
        private float currentXSpeed;
        private float currentYSpeed;

        private PlayerStats stats;
        private UpgradeTree upgradeTree;

        public void InitializeModel(PlayerCharacterConfiguration config)
        {
            this.stats = config.GetStats;
            upgradeTree = config.GetUpgradeTree;
        }

        public void SetXSpeed(float value)
        {
            currentXSpeed = value;
        }
        
        public void SetYSpeed(float value)
        {
            currentYSpeed = value;
        }

        public void ChangeHealth(int value)
        {
            currentHealth = Mathf.Clamp(currentHealth + value, 0, stats.MaxHealth);

            float healthPercent = Mathf.InverseLerp(0, stats.MaxHealth, currentHealth);
            HealthPercentChanged?.Invoke(healthPercent);
        }

        public void ChangeXP(int value, out bool levelUp)
        {
            levelUp = false;
            totalXp += value;

            if (totalXp >= nextLevelXpReq)
            {
                levelUp = true;
                PerformLevelUp();
            }

            float xpPercent = Mathf.InverseLerp(previousLevelXpReq, nextLevelXpReq, totalXp);
            XPPercentChanged?.Invoke(xpPercent);
        }

        public void UpgradeStats(PlayerStats upgradeStats)
        {
            stats.UpgradeStats(upgradeStats);
            // change health by 0 to trigger the UI update
            ChangeHealth(0);
        }

        private void PerformLevelUp()
        {
            currentLevel++;

            // Calculate new next level requirement
            previousLevelXpReq = nextLevelXpReq;
            nextLevelXpReq = 5
                             + (ClampLevel(currentLevel - 1) * 10)
                             + (ClampLevel(currentLevel - 20) * 3) 
                             + (ClampLevel(currentLevel - 40) * 3);
        }

        private int ClampLevel(int value)
        {
            return Math.Clamp(value, 0, 100);
        }
    }
}
