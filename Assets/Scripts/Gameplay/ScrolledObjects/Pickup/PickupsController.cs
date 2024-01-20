using System;
using System.Collections.Generic;
using Configuration;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Gameplay.ScrolledObjects.Pickup
{
    public class PickupsController
    {
        [Inject] private readonly Transform pickupsParent;
        [Inject] private readonly PickupControllerConfig config;

        private List<KeyValuePair<PickupType, ScrolledObjectView>> activePickups = new List<KeyValuePair<PickupType, ScrolledObjectView>>(16);
        private ObjectPool<ScrolledObjectView> pooledXPPickups;
        private ObjectPool<ScrolledObjectView> pooledHealthPickups;

        public void Initialize()
        {
            pooledXPPickups = new ObjectPool<ScrolledObjectView>(CreateXPPickup, null, null, null, true, config.PoolSize);
            pooledHealthPickups = new ObjectPool<ScrolledObjectView>(CreateHealthPickup, null, null, null, true, config.PoolSize);
        }
        
        public void DoUpdate()
        {
            for (int i = activePickups.Count - 1; i >= 0; i--)
            {
                ScrolledObjectView activePickup = activePickups[i].Value;
                
                if (activePickup.transform.position.x <= config.EndX)
                {
                    _ = activePickup.Deactivate();
                }
                
                if (activePickup.Active)
                {
                    if (GameModel.CurrentGamePhase == GamePhase.UpgradePhase && activePickups[i].Key != PickupType.Upgrade)
                    {
                        _ = activePickup.Deactivate();
                    }
                    else
                    {
                        activePickup.ScrolledObjectUpdate();
                    }
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
                            UnityEngine.Object.Destroy(toRemove.Value.gameObject);
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
            SpawnPickupsRoutine(pickupOrders, comboModifier, config.SpawnGap);
        }

        public ScrolledObjectView[] SpawnAndReturnPickups(Stack<PickupDropOrder> pickupOrders, float comboModifier)
        {
            ScrolledObjectView[] spawnedPickups = new ScrolledObjectView[pickupOrders.Count];
            PickupDropOrder currentOrder;
            int index = 0;

            while (pickupOrders.Count > 0)
            {
                currentOrder = pickupOrders.Pop();
                
                if (currentOrder.Value is int valueInt)
                {
                    object newValue = Mathf.FloorToInt(valueInt * comboModifier);
                    currentOrder.SetNewValue(newValue);
                }

                spawnedPickups[index] = SpawnPickup(currentOrder);
                index++;
            }

            return spawnedPickups;
        }

        public ScrolledObjectView SpawnPickup(PickupDropOrder order)
        {
            return SpawnPickup(order.Position, order.Value, order.Type);
        }

        public ScrolledObjectView SpawnPickup(Vector3 position, object value, PickupType type)
        {
            ScrolledObjectView spawnedPickup = null;

            if (type == PickupType.XP && GameModel.CurrentGamePhase > GamePhase.HordePhase)
            {
                type = PickupType.Health;
            }

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
                    spawnedPickup = CreatePickup(PickupType.Upgrade);
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
                int sizeValue = 1;

                if (value is int valueInt)
                {
                    sizeValue = valueInt;
                }

                Vector3 targetPosition = position + config.SpawnPositionOffset;
                Vector3 targetScale =
                    Vector3.one * Constants.MapFloat(config.MinPickupValue, config.MaxPickupValue, config.MinPickupScale, config.MaxPickupScale, sizeValue);

                Transform spawnedPickupTransform = spawnedPickup.transform;
                spawnedPickupTransform.localScale = config.SpawnInitialScale;
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
                return spawnedPickup;
            }
            else
            {
                Debug.LogWarning("Null pickup.");
                return null;
            }
        }

        public List<Vector3> PurgeAllPickups(PickupType specificType = PickupType.None)
        {
            List<Vector3> purgePositions = new List<Vector3>();
            
            foreach (KeyValuePair<PickupType, ScrolledObjectView> pickup in activePickups)
            {
                if (specificType == PickupType.None || pickup.Key == specificType)
                {
                    purgePositions.Add(pickup.Value.transform.position);
                    _ = pickup.Value.Deactivate();
                }
            }

            return purgePositions;
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
                    selectedPrefab = config.XPPickupPrefab;
                    break;
                case PickupType.Health:
                    selectedPrefab = config.HealthPickupPrefab;
                    break;
                case PickupType.Upgrade:
                    selectedPrefab = config.UpgradePickupPrefab;
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
                ScrolledObjectView createdPickup = UnityEngine.Object.Instantiate(selectedPrefab, Vector3.up * 500.0f, quaternion.identity);
                createdPickup.Initialize(new PickupLogic(type, 1));
                _ = createdPickup.Deactivate();
                createdPickup.transform.SetParent(pickupsParent);
                return createdPickup;
            }
            else
            {
                Debug.LogWarning("Pickup instantiation failed!");
                return null;
            }
        }

        private async void SpawnPickupsRoutine(Stack<PickupDropOrder> pickupOrders, float comboModifier, float spawnGap)
        {
            PickupDropOrder currentOrder;
            object currentValue;
            await Awaitable.NextFrameAsync();

            while (pickupOrders.Count > 0)
            {
                currentOrder = pickupOrders.Pop();
                currentValue = currentOrder.Value;

                if (currentValue is int valueInt)
                {
                    currentValue = Mathf.FloorToInt(valueInt * comboModifier);
                }
                
                SpawnPickup(currentOrder.Position, currentValue, currentOrder.Type);
                await Awaitable.WaitForSecondsAsync(spawnGap);
            }
        }
    }
}
