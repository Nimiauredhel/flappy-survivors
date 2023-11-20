using System;
using UnityEngine;

namespace Gameplay.Player
{
    [Serializable]
    public class PlayerStats
    {
        public int MaxHealth => maxHealth;
        public float MagnetStrength => magnetStrength;
        
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float magnetStrength = 3.0f;

        public PlayerStats(PlayerStats original)
        {
            maxHealth = original.maxHealth;
            magnetStrength = original.magnetStrength;
        }

        public void UpgradeStats(PlayerStats upgradeStats)
        {
            maxHealth += upgradeStats.maxHealth;
            magnetStrength += upgradeStats.magnetStrength;
        }
    }
}
