using System;
using System.Collections.Generic;
using Configuration;
using Gameplay.Level;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Gameplay.ScrolledObjects.Enemy
{
    public class EnemiesController
    {
        public event Action<bool, int, int, Vector3> EnemyHit;

        [Inject] private readonly EnemyControllerConfig config;
        [Inject] private readonly BurstSignalReceiver signalReceiver;
    
        private ObjectPool<ScrolledObjectView>[] enemyPools;
        private List<ScrolledObjectView>[] activeEnemyLists;

        public void Initialize()
        {
            int numOfTypes = config.EnemyRegistry.EnemyTypes.Length;
            
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
                
                enemyPools[i] = new ObjectPool<ScrolledObjectView>
                (
                    createEnemyOfType, onGetEnemyOfType, onReturnEnemyOfType,
                    null, true, config.PoolSize);
            }

            signalReceiver.signalAssetEventPairs = new BurstSignalReceiver.SignalAssetEventPair[1]
            {
                new BurstSignalReceiver.SignalAssetEventPair()
            };
            signalReceiver.signalAssetEventPairs[0].signalAsset = config.EnemyBurstSignal;
            signalReceiver.signalAssetEventPairs[0].events =
                new BurstSignalReceiver.SignalAssetEventPair.ParameterizedEvent();
            signalReceiver.signalAssetEventPairs[0].events.AddListener(RequestEnemyBurst);
        }

        public void DoUpdate()
        {
            for (int i = 0; i < activeEnemyLists.Length; i++)
            {
                for (int j = activeEnemyLists[i].Count - 1; j >= 0; j--)
                {
                    ScrolledObjectView currentObject = activeEnemyLists[i][j];
                    
                    if (currentObject.transform.position.x <= config.EndX)
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
            EnemyBurstRoutine(burstDefinition);
        }

        public List<Vector3> PurgeAllEnemies()
        {
            List<Vector3> purgePositions = new List<Vector3>();
            
            for (int i = 0; i < activeEnemyLists.Length; i++)
            {
                for (int j = activeEnemyLists[i].Count - 1; j >= 0; j--)
                {
                    ScrolledObjectView currentObject = activeEnemyLists[i][j];
                    purgePositions.Add(currentObject.transform.position);
                    currentObject.Deactivate();
                }
            }

            return purgePositions;
        }

        private async Awaitable EnemyBurstRoutine(BurstDefinition burstDefinition)
        {
            for (int i = 0; i < burstDefinition.enemyAmount; i++)
            {
                if (GameModel.CurrentGamePhase == GamePhase.BossPhase)
                {
                    break;
                }

                while (GameModel.CurrentGamePhase != GamePhase.HordePhase)
                {
                    await Awaitable.NextFrameAsync();
                }

                ScrolledObjectView enemy = enemyPools[burstDefinition.enemyId].Get();
                enemy.SetPath(config.Paths.Splines[burstDefinition.pathId]);
                await Awaitable.WaitForSecondsAsync(burstDefinition.enemySpawnGap);
            }
        }

        private void EnemyHitForwarder(bool killed, int damage, int value, Vector3 position)
        {
            EnemyHit?.Invoke(killed, damage, value, position);
        }

        private ScrolledObjectView CreateEnemy(int enemyId)
        {
            EnemyConfiguration enemyConfig = config.EnemyRegistry.EnemyTypes[enemyId];
            
            EnemyLogic createdLogic = new EnemyLogic(enemyConfig.Stats, EnemyHitForwarder);
            ScrolledObjectView createdView = UnityEngine.Object.Instantiate<ScrolledObjectView>(enemyConfig.ViewPrefab, new Vector3(config.StartX, 0.0f, 0.0f), Quaternion.identity);
            createdView.Initialize(createdLogic);
            return createdView;
        }

        private void OnGetEnemy(int enemyId, ScrolledObjectView spawnedEnemy)
        {
            spawnedEnemy.transform.position = new Vector3(config.StartX, 0.0f);
            spawnedEnemy.Activate(null);
            activeEnemyLists[enemyId].Add(spawnedEnemy);
        }

        private void OnReturnEnemy(int enemyId, ScrolledObjectView returnedEnemy)
        {
            activeEnemyLists[enemyId].Remove(returnedEnemy);
        }
    }
}
