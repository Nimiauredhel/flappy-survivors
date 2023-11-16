using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay
{
    public class VFXService : MonoBehaviour
    {
        private static readonly WaitForSeconds WaitForExplosion = new WaitForSeconds(1.0f);
        
        private ObjectPool<GameObject> pooledExplosions;
        [SerializeField] private GameObject explosionPrefab;

        public void Initialize()
        {
            pooledExplosions = new ObjectPool<GameObject>(CreateExplosion);
        }

        public void RequestExplosionAt(Vector2 position)
        {
            StartCoroutine(ServeExplosion(position));
        }

        private IEnumerator ServeExplosion(Vector2 position)
        {
            GameObject explosion;
            pooledExplosions.Get(out explosion);

            if (explosion != null)
            {
                explosion.transform.position = position;
                explosion.SetActive(true);
                yield return WaitForExplosion;
                explosion.SetActive(false);
                pooledExplosions.Release(explosion);
            }

            yield return null;
        }

        private GameObject CreateExplosion()
        {
            GameObject explosion = GameObject.Instantiate(explosionPrefab);
            explosion.SetActive(false);
            return explosion;
        }

    }
}
