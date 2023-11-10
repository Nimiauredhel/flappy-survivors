using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class ObjectMover : MonoBehaviour
    {
        public List<ScrolledObject> activeObjects = new List<ScrolledObject>(50);
        public List<ScrolledObject> pooledObjects = new List<ScrolledObject>(100);
        public List<ScrolledObject> toRemove = new List<ScrolledObject>(50);

        [SerializeField] private int poolSize;
        [SerializeField] private float startX, endX, minY, maxY;
        [SerializeField] private float maxSpawnGap, minSpawnGap;
        [SerializeField] private ScrolledObject objectPrefab;

        private float spawnCooldown = 0.0f;
    
        public void Initialize()
        {
            for (int i = 0; i < poolSize; i++)
            {
                ScrolledObject SO = Instantiate<ScrolledObject>(objectPrefab, new Vector3(startX, 0.0f, 0.0f), Quaternion.identity, transform);
                pooledObjects.Add(SO);
            }
        }

        public void DoUpdate()
        {
            if (spawnCooldown <= 0.0f)
            {
                SpawnObject();
            }
            else
            {
                spawnCooldown -= Time.deltaTime;
            }

            for (int i = 0; i < activeObjects.Count; i++)
            {
                if (activeObjects[i].transform.position.x <= endX)
                {
                    toRemove.Add(activeObjects[i]);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                toRemove[i].Deactivate();
                activeObjects.Remove(toRemove[i]);
            }
        
            toRemove.Clear();
        
            for (int i = 0; i < activeObjects.Count; i++)
            {
                activeObjects[i].transform.Translate((new Vector2(-3.0f, 0.0f) * Time.deltaTime));
            }
        }

        public void DoFixedUpdate()
        {
        
        }

        private void SpawnObject()
        {
            if (pooledObjects.Count > 0)
            {
                ScrolledObject SO = pooledObjects[pooledObjects.Count - 1];
                pooledObjects.RemoveAt(pooledObjects.Count-1);
                SO.transform.position = new Vector3(startX, Random.Range(minY, maxY));
                SO.Activate();
                activeObjects.Add(SO);
                spawnCooldown = maxSpawnGap;
            }
        }
    }
}
