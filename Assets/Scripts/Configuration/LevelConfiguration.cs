﻿using Gameplay.Background;
using Gameplay.Level;
using UnityEngine;
using UnityEngine.Timeline;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Level Config", menuName = "Config/Level Config", order = 0)]
    public class LevelConfiguration : ScriptableObject
    {
        public string Name => name;
        public Sprite Thumbnail => thumbnail;
        public float RunTime => (float)timeline.duration;
        public TimelineAsset Timeline => timeline;
        public BackgroundView BackgroundAsset => backgroundAsset;
        public BurstDefinition BossEnemy => bossEnemy;

        [SerializeField] private new string name;
        [SerializeField] private Sprite thumbnail;
        [SerializeField] private TimelineAsset timeline;
        [SerializeField] private BackgroundView backgroundAsset;
        [SerializeField] private BurstDefinition bossEnemy;
    }
}