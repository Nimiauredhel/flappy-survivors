using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Timeline;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Enemy Controller Config", menuName = "Config/Enemy Controller Config", order = 0)]
    public class EnemyControllerConfig : ScriptableObject
    {
        public int PoolSize => poolSize;
        public float StartX => startX;
        public float EndX => endX;

        public EnemyRegistry EnemyRegistry => enemyRegistry;
        public SplineContainer Paths => paths;
        public SignalAsset EnemyBurstSignal => enemyBurstSignal;
        
        [SerializeField] private int poolSize = 100;
        [SerializeField] private float startX, endX;
        [SerializeField] private EnemyRegistry enemyRegistry;
        [SerializeField] private SplineContainer paths;
        [SerializeField] private SignalAsset enemyBurstSignal;
    }
}