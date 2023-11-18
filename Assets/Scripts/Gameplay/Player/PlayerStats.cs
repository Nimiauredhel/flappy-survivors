using System;
using UnityEngine;

namespace Gameplay.Player
{
    [Serializable]
    public class PlayerStats
    {
        public int MaxHealth => maxHealth;
        
        [SerializeField] private int maxHealth = 100;

        public PlayerStats(PlayerStats original)
        {
            maxHealth = original.maxHealth;
        }

        public void UpgradeStats(PlayerStats upgradeStats)
        {
            maxHealth += upgradeStats.maxHealth;
        }
    }
}
