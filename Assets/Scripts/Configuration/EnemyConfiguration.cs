﻿using Gameplay.ScrolledObjects;
using Gameplay.ScrolledObjects.Enemy;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Enemy Config", menuName = "Config/Enemy Config", order = 0)]
    public class EnemyConfiguration : ScriptableObject
    {
        public EnemyStats Stats => stats;
        public ScrolledObjectView ViewPrefab => viewPrefab;
        public Sprite Icon => icon;
        public bool RotateWithPath => rotateWithPath;

        [SerializeField] private EnemyStats stats;
        [SerializeField] private ScrolledObjectView viewPrefab;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool rotateWithPath;
    }
}