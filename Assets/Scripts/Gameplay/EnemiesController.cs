using System;
using System.Collections.Generic;
using Gameplay.Configuration;
using Gameplay.ScrolledObjects;
using Gameplay.ScrolledObjects.Enemy;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class EnemiesController : MonoBehaviour
    {
        public event EventHandler<int> EnemyKilled;

        [SerializeField] private int poolSize;
        [SerializeField] private float startX, endX, minY, maxY;
        [SerializeField] private float maxSpawnGap, minSpawnGap;
        [SerializeField] private EnemyConfiguration enemyConfig;

        private float spawnCooldown = 0.0f;
    
        private ObjectPool<ScrolledObjectView> pooledEnemies;
        private List<ScrolledObjectView> activeEnemies = new List<ScrolledObjectView>(50);
        
        public void Initialize()
        {
            pooledEnemies = new ObjectPool<ScrolledObjectView>(CreateEnemy, OnGetEnemy, OnReturnEnemy, null, true, poolSize);
        }

        public void DoUpdate()
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i].transform.position.x <= endX)
                {
                    activeEnemies[i].Deactivate();
                }
                
                if (!activeEnemies[i].Active)
                {
                    pooledEnemies.Release(activeEnemies[i]);
                }
            }
        
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                activeEnemies[i].ScrolledObjectUpdate();
            }
            
            if (spawnCooldown <= 0.0f)
            {
                pooledEnemies.Get();
            }
            else
            {
                spawnCooldown -= Time.deltaTime;
            }
        }

        public void DoFixedUpdate()
        {
        
        }

        private void EnemyKilledForwarder(object sender, int value)
        {
            EnemyKilled?.Invoke(sender, value);
        }

        private ScrolledObjectView CreateEnemy()
        {
            EnemyLogic createdLogic = new EnemyLogic(enemyConfig.Stats, EnemyKilledForwarder);
            ScrolledObjectView createdView = Instantiate<ScrolledObjectView>(enemyConfig.ViewPrefab, new Vector3(startX, 0.0f, 0.0f), Quaternion.identity, transform);
            createdView.Initialize(createdLogic);
            return createdView;
        }

        private void OnGetEnemy(ScrolledObjectView spawnedEnemy)
        {
            spawnCooldown = maxSpawnGap;
            spawnedEnemy.transform.position = new Vector3(startX, Random.Range(minY, maxY));
            spawnedEnemy.Activate();
            activeEnemies.Add(spawnedEnemy);
        }

        private void OnReturnEnemy(ScrolledObjectView returnedEnemy)
        {
            activeEnemies.Remove(returnedEnemy);
        }
    }
}
