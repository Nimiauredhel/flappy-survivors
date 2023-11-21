using System;
using UnityEngine;

namespace Gameplay.Level
{
    [Serializable]
    public class BurstDefinition
    {
        public int enemyId;
        public int enemyAmount;
        public float enemySpawnGap;
    }
}