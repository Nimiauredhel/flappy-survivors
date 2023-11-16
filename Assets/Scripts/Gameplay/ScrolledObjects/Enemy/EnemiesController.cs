using System;
using System.Collections.Generic;
using Configuration;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Gameplay.ScrolledObjects.Enemy
{
    public class EnemiesController : MonoBehaviour
    {
        public event Action<int, Vector3> EnemyKilled;

        [SerializeField] private int poolSize = 100;
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
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i].transform.position.x <= endX)
                {
                    activeEnemies[i].Deactivate();
                }
                
                if (activeEnemies[i].Active)
                {
                    activeEnemies[i].ScrolledObjectUpdate();
                }
                else
                {
                    ScrolledObjectView toRemove = activeEnemies[i];
                    activeEnemies.Remove(toRemove);
                    pooledEnemies.Release(toRemove);
                }
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
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                activeEnemies[i].ScrolledObjectFixedUpdate();
            }
        }

        private void EnemyKilledForwarder(int value, Vector3 position)
        {
            EnemyKilled?.Invoke(value, position);
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
            spawnCooldown = Random.Range(minSpawnGap, maxSpawnGap);
            spawnedEnemy.transform.position = new Vector3(startX, Random.Range(minY, maxY));
            spawnedEnemy.Activate(0);
            activeEnemies.Add(spawnedEnemy);
        }

        private void OnReturnEnemy(ScrolledObjectView returnedEnemy)
        {
            activeEnemies.Remove(returnedEnemy);
        }
    }
}
