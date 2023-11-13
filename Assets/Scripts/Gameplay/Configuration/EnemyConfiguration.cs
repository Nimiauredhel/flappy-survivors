using Gameplay.ScrolledObjects;
using Gameplay.ScrolledObjects.Enemy;
using UnityEngine;

namespace Gameplay.Configuration
{
    [CreateAssetMenu(fileName = "Enemy Config", menuName = "Config/Enemy Config", order = 0)]
    public class EnemyConfiguration : ScriptableObject
    {
        public EnemyStats Stats => stats;
        public ScrolledObjectView ViewPrefab => viewPrefab;
        
        [SerializeField] private EnemyStats stats;
        [SerializeField] private ScrolledObjectView viewPrefab;
    }
}