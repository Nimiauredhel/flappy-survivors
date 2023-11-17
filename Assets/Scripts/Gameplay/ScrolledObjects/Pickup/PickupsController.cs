using System;
using System.Collections;
using System.Collections.Generic;
using Configuration;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay.ScrolledObjects.Pickup
{
    public class PickupsController : MonoBehaviour
    {
        private static readonly Vector3 SPAWN_INITIAL_SCALE = new Vector3(0.0f, 0.0f, 1.0f);
        private static readonly Vector3 SPAWN_POSITION_OFFSET = new Vector3(10.0f, 0.0f, 0.0f);
        private static readonly WaitForSeconds WaitForSpawnGap = new WaitForSeconds(0.25f);
        
        [SerializeField] private int poolSize = 100;
        [SerializeField] private PickupConfiguration pickupConfig;
        [SerializeField] private ScrolledObjectView pickupPrefab;

        private int minPickupValue = 1;
        private int maxPickupValue = 100;
        private float minPickupScale = 1.0f;
        private float maxPickupScale = 1.5f;

        private List<ScrolledObjectView> activePickups = new List<ScrolledObjectView>(16);
        private ObjectPool<ScrolledObjectView> pooledPickups;

        public void Initialize()
        {
            pooledPickups = new ObjectPool<ScrolledObjectView>(CreatePickup, null, null, null, true, poolSize);
        }
        
        public void DoUpdate()
        {
            for (int i = activePickups.Count - 1; i >= 0; i--)
            {
                if (activePickups[i].transform.position.x <= -24.0f)
                {
                    activePickups[i].Deactivate();
                }
                
                if (activePickups[i].Active)
                {
                    activePickups[i].ScrolledObjectUpdate();
                }
                else
                {
                    ScrolledObjectView toRemove = activePickups[i];
                    activePickups.Remove(toRemove);
                    pooledPickups.Release(toRemove);
                }
            }
        }

        public void DoFixedUpdate()
        {
            for (int i = 0; i < activePickups.Count; i++)
            {
                activePickups[i].ScrolledObjectFixedUpdate();
            }
        }

        public void SpawnPickups(Stack<PickupDropOrder> pickupOrders, PickupType type)
        {
            StartCoroutine(SpawnPickupsRoutine(pickupOrders, type));
        }

        public void SpawnPickup(PickupDropOrder order, PickupType type)
        {
            SpawnPickup(order.Position, order.Value, type);
        }

        public void SpawnPickup(Vector3 position, int value, PickupType type)
        {
            ScrolledObjectView spawnedPickup;
            pooledPickups.Get(out spawnedPickup);

            if (spawnedPickup != null)
            {
                
                Vector3 targetPosition = position + SPAWN_POSITION_OFFSET;
                Vector3 targetScale =
                    Vector3.one * Constants.Map(minPickupValue, maxPickupValue, minPickupScale, maxPickupScale, value);

                Transform spawnedPickupTransform = spawnedPickup.transform;
                spawnedPickupTransform.localScale = SPAWN_INITIAL_SCALE;
                spawnedPickupTransform.position = position;
                
                Sequence spawnSequence = DOTween.Sequence();
                spawnSequence.Append(spawnedPickupTransform.DOScale(targetScale, 0.5f));
                spawnSequence.Join(spawnedPickupTransform.DOMove(targetPosition, 0.75f));
                
                spawnedPickup.Activate(value);
                
                spawnSequence.AppendCallback(delegate
                {
                    if (spawnedPickup.Active)
                    {
                        activePickups.Add(spawnedPickup);
                    }
                });

                spawnSequence.Play();
            }
        }

        private ScrolledObjectView CreatePickup()
        {
            ScrolledObjectView createdPickup = Instantiate(pickupPrefab, Vector3.up * 500.0f, quaternion.identity);
            createdPickup.Initialize(new PickupLogic(pickupConfig.Type, 0, 5.0f));
            createdPickup.Deactivate();
            return createdPickup;
        }

        private IEnumerator SpawnPickupsRoutine(Stack<PickupDropOrder> pickupOrders, PickupType type)
        {
            yield return null;

            while (pickupOrders.Count > 0)
            {
                SpawnPickup(pickupOrders.Pop(), type);
                yield return WaitForSpawnGap;
            }
        }
    }
}
