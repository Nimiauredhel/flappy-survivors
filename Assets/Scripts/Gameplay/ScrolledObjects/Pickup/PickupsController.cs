using System;
using System.Collections;
using System.Collections.Generic;
using Configuration;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Gameplay.ScrolledObjects.Pickup
{
    public class PickupsController : MonoBehaviour
    {
        private const int MIN_PICKUP_VALUE = 1;
        private const int MAX_PICKUP_VALUE = 100;
        private const float MIN_PICKUP_SCALE = 1.0f;
        private const float MAX_PICKUP_SCALE = 1.5f;
        private static readonly Vector3 SPAWN_INITIAL_SCALE = new Vector3(0.0f, 0.0f, 1.0f);
        private static readonly Vector3 SPAWN_POSITION_OFFSET = new Vector3(10.0f, 0.0f, 0.0f);
        private static readonly WaitForSeconds WaitForSpawnGap = new WaitForSeconds(0.25f);
        
        [SerializeField] private int poolSize = 100;
        [SerializeField] private ScrolledObjectView xpPickupPrefab;
        [SerializeField] private ScrolledObjectView healthPickupPrefab;

        private List<KeyValuePair<PickupType, ScrolledObjectView>> activePickups = new List<KeyValuePair<PickupType, ScrolledObjectView>>(16);
        private ObjectPool<ScrolledObjectView> pooledXPPickups;
        private ObjectPool<ScrolledObjectView> pooledHealthPickups;

        public void Initialize()
        {
            pooledXPPickups = new ObjectPool<ScrolledObjectView>(CreateXPPickup, null, null, null, true, poolSize);
            pooledHealthPickups = new ObjectPool<ScrolledObjectView>(CreateHealthPickup, null, null, null, true, poolSize);
        }
        
        public void DoUpdate()
        {
            for (int i = activePickups.Count - 1; i >= 0; i--)
            {
                ScrolledObjectView activePickup = activePickups[i].Value;
                
                if (activePickup.transform.position.x <= -24.0f)
                {
                    activePickup.Deactivate();
                }
                
                if (activePickup.Active)
                {
                    activePickup.ScrolledObjectUpdate();
                }
                else
                {
                    KeyValuePair<PickupType, ScrolledObjectView> toRemove = activePickups[i];

                    activePickups.Remove(toRemove);
                    
                    switch (toRemove.Key)
                    {
                        case PickupType.None:
                            break;
                        case PickupType.XP:
                            pooledXPPickups.Release(toRemove.Value);
                            break;
                        case PickupType.Health:
                            pooledHealthPickups.Release(toRemove.Value);
                            break;
                        case PickupType.Upgrade:
                            break;
                        case PickupType.Gold:
                            break;
                        case PickupType.Chest:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public void DoFixedUpdate()
        {
            for (int i = 0; i < activePickups.Count; i++)
            {
                activePickups[i].Value.ScrolledObjectFixedUpdate();
            }
        }

        public void SpawnPickups(Stack<PickupDropOrder> pickupOrders, float comboModifier)
        {
            StartCoroutine(SpawnPickupsRoutine(pickupOrders, comboModifier));
        }

        public void SpawnPickup(PickupDropOrder order, PickupType type)
        {
            SpawnPickup(order.Position, order.Value, type);
        }

        public void SpawnPickup(Vector3 position, int value, PickupType type)
        {
            ScrolledObjectView spawnedPickup = null;

            switch (type)
            {
                case PickupType.None:
                    break;
                case PickupType.XP:
                    pooledXPPickups.Get(out spawnedPickup);
                    break;
                case PickupType.Health:
                    pooledHealthPickups.Get(out spawnedPickup);
                    break;
                case PickupType.Upgrade:
                    break;
                case PickupType.Gold:
                    break;
                case PickupType.Chest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (spawnedPickup != null)
            {
                Vector3 targetPosition = position + SPAWN_POSITION_OFFSET;
                Vector3 targetScale =
                    Vector3.one * Constants.Map(MIN_PICKUP_VALUE, MAX_PICKUP_VALUE, MIN_PICKUP_SCALE, MAX_PICKUP_SCALE, value);

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
                        activePickups.Add(new KeyValuePair<PickupType, ScrolledObjectView>(type, spawnedPickup));
                    }
                });

                spawnSequence.Play();
            }
            else
            {
                Debug.LogWarning("Null pickup.");
            }
        }

        private ScrolledObjectView CreateXPPickup()
        {
            return CreatePickup(PickupType.XP);
        }
        
        private ScrolledObjectView CreateHealthPickup()
        {
            return CreatePickup(PickupType.Health);
        }

        private ScrolledObjectView CreatePickup(PickupType type)
        {
            ScrolledObjectView selectedPrefab = null;

            switch (type)
            {
                case PickupType.None:
                    break;
                case PickupType.XP:
                    selectedPrefab = xpPickupPrefab;
                    break;
                case PickupType.Health:
                    selectedPrefab = healthPickupPrefab;
                    break;
                case PickupType.Upgrade:
                    break;
                case PickupType.Gold:
                    break;
                case PickupType.Chest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (selectedPrefab != null)
            {
                ScrolledObjectView createdPickup = Instantiate(selectedPrefab, Vector3.up * 500.0f, quaternion.identity);
                createdPickup.Initialize(new PickupLogic(type, 1, 5.0f));
                createdPickup.Deactivate();
                return createdPickup;
            }
            else
            {
                Debug.LogWarning("Pickup instantiation failed!");
                return null;
            }
        }

        private IEnumerator SpawnPickupsRoutine(Stack<PickupDropOrder> pickupOrders, float comboModifier)
        {
            PickupDropOrder currentOrder;
            yield return null;

            while (pickupOrders.Count > 0)
            {
                currentOrder = pickupOrders.Pop();
                SpawnPickup(currentOrder.Position, Mathf.FloorToInt(currentOrder.Value * comboModifier), currentOrder.Type);
                yield return WaitForSpawnGap;
            }
        }
    }
}
