using System;
using System.Collections;
using System.Collections.Generic;
using Configuration;
using Gameplay.Level;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Gameplay.ScrolledObjects.Enemy
{
    public class EnemiesController : MonoBehaviour
    {
        public event Action<bool, int, int, Vector3> EnemyHit;
        
        [SerializeField] private int poolSize = 100;
        [SerializeField] private float startX, endX, minY, maxY;
        [SerializeField] private EnemyRegistry enemyRegistry;
        [SerializeField] private SplineContainer paths;
    
        private ObjectPool<ScrolledObjectView>[] enemyPools;
        private List<ScrolledObjectView>[] activeEnemyLists;

        public void Initialize()
        {
            int numOfTypes = enemyRegistry.EnemyTypes.Length;
            
            activeEnemyLists = new System.Collections.Generic.List<ScrolledObjectView>[numOfTypes];

            for (int i = 0; i < activeEnemyLists.Length; i++)
            {
                activeEnemyLists[i] = new List<ScrolledObjectView>(8);
            }

            enemyPools = new ObjectPool<ScrolledObjectView>[numOfTypes];
            Func<ScrolledObjectView> createEnemyOfType;
            Action<ScrolledObjectView> onGetEnemyOfType;
            Action<ScrolledObjectView> onReturnEnemyOfType;

            for (int i = 0; i < enemyPools.Length; i++)
            {
                int index = i;
                createEnemyOfType = delegate { return CreateEnemy(index); };
                onGetEnemyOfType = delegate(ScrolledObjectView view) { OnGetEnemy(index, view); };
                onReturnEnemyOfType = delegate(ScrolledObjectView view) { OnReturnEnemy(index, view); };
                
                enemyPools[i] = new ObjectPool<ScrolledObjectView>(createEnemyOfType, onGetEnemyOfType, onReturnEnemyOfType, null, true, poolSize);
            }
        }

        public void DoUpdate()
        {
            for (int i = 0; i < activeEnemyLists.Length; i++)
            {
                for (int j = activeEnemyLists[i].Count - 1; j >= 0; j--)
                {
                    ScrolledObjectView currentObject = activeEnemyLists[i][j];
                    
                    if (currentObject.transform.position.x <= endX)
                    {
                        currentObject.Deactivate();
                    }

                    if (currentObject.Active)
                    {
                        currentObject.ScrolledObjectUpdate();
                    }
                    else
                    {
                        activeEnemyLists[i].Remove(currentObject);
                        enemyPools[i].Release(currentObject);
                    }
                }
            }
        }

        public void DoFixedUpdate()
        {
            for (int i = 0; i < activeEnemyLists.Length; i++)
            {
                for (int j = 0; j < activeEnemyLists[i].Count; j++)
                {
                    activeEnemyLists[i][j].ScrolledObjectFixedUpdate();
                }
            }

        }

        public void RequestEnemyBurst(BurstDefinition burstDefinition)
        {
            StartCoroutine(EnemyBurstRoutine(burstDefinition));
        }

        private IEnumerator EnemyBurstRoutine(BurstDefinition burstDefinition)
        {
            WaitForSeconds spawnGap = new WaitForSeconds(burstDefinition.enemySpawnGap);

            for (int i = 0; i < burstDefinition.enemyAmount; i++)
            {
                ScrolledObjectView enemy = enemyPools[burstDefinition.enemyId].Get();
                enemy.SetPath(paths.Splines[burstDefinition.pathId]);
                yield return spawnGap;
            }
        }

        private void EnemyHitForwarder(bool killed, int damage, int value, Vector3 position)
        {
            EnemyHit?.Invoke(killed, damage, value, position);
        }

        private ScrolledObjectView CreateEnemy(int enemyId)
        {
            EnemyConfiguration enemyConfig = enemyRegistry.EnemyTypes[enemyId];
            
            EnemyLogic createdLogic = new EnemyLogic(enemyConfig.Stats, EnemyHitForwarder);
            ScrolledObjectView createdView = Instantiate<ScrolledObjectView>(enemyConfig.ViewPrefab, new Vector3(startX, 0.0f, 0.0f), Quaternion.identity, transform);
            createdView.Initialize(createdLogic);
            return createdView;
        }

        private void OnGetEnemy(int enemyId, ScrolledObjectView spawnedEnemy)
        {
            spawnedEnemy.transform.position = new Vector3(startX, Random.Range(minY, maxY));
            spawnedEnemy.Activate(null);
            activeEnemyLists[enemyId].Add(spawnedEnemy);
        }

        private void OnReturnEnemy(int enemyId, ScrolledObjectView returnedEnemy)
        {
            activeEnemyLists[enemyId].Remove(returnedEnemy);
        }
    }
}
