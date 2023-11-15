using System;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerModel
    {
        public int CurrentLevel => currentLevel;
        public int TotalXP => totalXp;
        public int NextLevelXPReq => nextLevelXpReq;
        public int PreviousLevelXPReq => previousLevelXpReq;
        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public float CurrentXSpeed => currentXSpeed;
        public float CurrentYSpeed => currentYSpeed;

        public event Action<float> HealthPercentChanged;
        public event Action<float> XPPercentChanged;
        public event Action<int> LeveledUp;
        
        private int currentLevel = 1;
        private int totalXp = 0;
        private int nextLevelXpReq = 5;
        private int previousLevelXpReq = 0;
        private int maxHealth = 100;
        private int currentHealth = 100;
        
        private float currentXSpeed;
        private float currentYSpeed;
        
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
            currentHealth = Mathf.Clamp(currentHealth + value, 0, maxHealth);

            float healthPercent = Mathf.InverseLerp(0, maxHealth, currentHealth);
            HealthPercentChanged?.Invoke(healthPercent);
        }

        public void ChangeXP(int value)
        {
            totalXp += value;

            if (totalXp >= nextLevelXpReq)
            {
                LevelUp();
            }

            float xpPercent = Mathf.InverseLerp(previousLevelXpReq, nextLevelXpReq, totalXp);
            XPPercentChanged?.Invoke(xpPercent);
        }

        private void LevelUp()
        {
            currentLevel++;

            // Calculate new next level requirement
            previousLevelXpReq = nextLevelXpReq;
            nextLevelXpReq = 5
                             + (ClampLevel(currentLevel - 1) * 10)
                             + (ClampLevel(currentLevel - 20) * 3) 
                             + (ClampLevel(currentLevel - 40) * 3);
            
            LeveledUp?.Invoke(currentLevel);
        }

        private int ClampLevel(int value)
        {
            return Math.Clamp(value, 0, 100);
        }
    }
}
