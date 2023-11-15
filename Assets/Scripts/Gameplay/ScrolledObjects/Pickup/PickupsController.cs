using System;
using System.Collections.Generic;
using Configuration;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay.ScrolledObjects.Pickup
{
    public class PickupsController : MonoBehaviour
    {
        [SerializeField] private int poolSize = 100;
        [SerializeField] private PickupConfiguration pickupConfig;
        [SerializeField] private ScrolledObjectView pickupPrefab;

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
        
        public void SpawnPickup(Vector3 position, int value, PickupType type)
        {
            ScrolledObjectView spawnedPickup;
            pooledPickups.Get(out spawnedPickup);

            if (spawnedPickup != null)
            {
                spawnedPickup.transform.position = position + (Vector3.right * 10.0f);
                spawnedPickup.Activate(value);
                activePickups.Add(spawnedPickup);
            }
        }

        private ScrolledObjectView CreatePickup()
        {
            ScrolledObjectView createdPickup = Instantiate(pickupPrefab, Vector3.up * 500.0f, quaternion.identity);
            createdPickup.Initialize(new PickupLogic(pickupConfig.Type, 0, 5.0f));
            createdPickup.Deactivate();
            return createdPickup;
        }
    }
}
