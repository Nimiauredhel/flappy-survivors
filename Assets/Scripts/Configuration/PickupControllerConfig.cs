using Gameplay.ScrolledObjects;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Pickup Controller Config", menuName = "Config/Pickup Controller Config", order = 0)]
    public class PickupControllerConfig : ScriptableObject
    {
        public int PoolSize => poolSize;
        public int MinPickupValue => minPickupValue;
        public int MaxPickupValue => maxPickupValue;
        
        public float MinPickupScale => minPickupScale;
        public float MaxPickupScale => maxPickupScale;
        public float SpawnGap => spawnGap;
        public float EndX => endX;

        public Vector3 SpawnInitialScale => spawnInitialScale;
        public Vector3 SpawnPositionOffset => spawnPositionOffset;
        
        public ScrolledObjectView XPPickupPrefab => xpPickupPrefab;
        public ScrolledObjectView HealthPickupPrefab => healthPickupPrefab;
        public ScrolledObjectView UpgradePickupPrefab => upgradePickupPrefab;

        [SerializeField] private int poolSize = 100;
        [SerializeField] private int minPickupValue = 1;
        [SerializeField] private int maxPickupValue = 100;
        
        [SerializeField] private float minPickupScale = 1.0f;
        [SerializeField] private float maxPickupScale = 1.5f;
        [SerializeField] private float spawnGap = 0.25f;
        [SerializeField] private float endX = -25.0f;
        
        [SerializeField] private Vector3 spawnInitialScale = new Vector3(0.0f, 0.0f, 1.0f);
        [SerializeField] private Vector3 spawnPositionOffset = new Vector3(10.0f, 0.0f, 0.0f);
        
        [SerializeField] private ScrolledObjectView xpPickupPrefab;
        [SerializeField] private ScrolledObjectView healthPickupPrefab;
        [SerializeField] private ScrolledObjectView upgradePickupPrefab;
    }
}