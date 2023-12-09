using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Gameplay.Level
{
    [Serializable]
    public class BurstDefinition
    {
        public bool noRotation;
        public int enemyId;
        public int enemyAmount;
        public int pathId;
        public float enemySpawnGap;
        public float speedOverride;

    }
}