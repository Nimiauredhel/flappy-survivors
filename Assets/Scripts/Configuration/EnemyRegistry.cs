using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Enemy Registry", menuName = "Config/Enemy Registry", order = 0)]
    public class EnemyRegistry : ScriptableObject
    {
        public EnemyConfiguration[] EnemyTypes => enemyTypes;
        
        [SerializeField] private EnemyConfiguration[] enemyTypes;
    }
}