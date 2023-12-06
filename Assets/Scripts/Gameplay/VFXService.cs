using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay
{
    public class VFXService : MonoBehaviour
    {
        private static readonly WaitForSeconds WaitForExplosion = new WaitForSeconds(1.0f);
        private static readonly WaitForSeconds WaitForDamageTextIn = new WaitForSeconds(0.3f);
        private static readonly WaitForSeconds WaitForDamageTextOut = new WaitForSeconds(0.7f);
        
        private ObjectPool<GameObject> pooledExplosions;
        private ObjectPool<TextMeshPro> pooledDamageText;
        
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private TextMeshPro damageTextPrefab;
        
        private Camera gameplayCamera;
        private Tween cameraShake = null;

        public void Initialize()
        {
            gameplayCamera = Camera.main;
            cameraShake = DOTween.Sequence();
            pooledExplosions = new ObjectPool<GameObject>(CreateExplosion);
            pooledDamageText = new ObjectPool<TextMeshPro>(CreateDamageText);
        }

        public void DoCameraShake(float strength)
        {
            if (cameraShake != null) cameraShake.Kill(true);

            Vector3 strengthVector = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0) * strength;
            cameraShake = gameplayCamera.DOShakePosition(0.25f, strengthVector);
            cameraShake.onComplete += () => cameraShake.Rewind();
        }

        public void RequestExplosionAt(Vector2 position)
        {
            StartCoroutine(ServeExplosion(position));
        }

        public void RequestDamageTextAt(int damage, Vector2 position)
        {
            StartCoroutine(ServeDamageText(damage, position));
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
        
        private IEnumerator ServeDamageText(int damage, Vector2 position)
        {
            TextMeshPro damageText;
            pooledDamageText.Get(out damageText);
            Vector2 targetPosition = position + new Vector2(Random.Range(-0.75f, 0.75f), Random.Range(-0.75f, 0.75f));

            if (damageText != null)
            {
                float targetScale = 1 + ((damage - 1) * 0.055f);
                string message = damage.ToString();
                damageText.text = message;
                damageText.transform.position = position;
                damageText.transform.localScale = Vector3.zero;
                damageText.alpha = 0.01f;
                damageText.gameObject.SetActive(true);
                damageText.DOFade(0.9f, 0.2f);
                damageText.transform.DOScale(Vector3.one * targetScale, 0.7f);
                damageText.transform.DOMove(targetPosition, 0.7f);
                yield return WaitForDamageTextIn;
                damageText.DOFade(0.0f, 0.5f);
                yield return WaitForDamageTextOut;
                damageText.gameObject.SetActive(false);
                pooledDamageText.Release(damageText);
            }

            yield return null;
        }

        private GameObject CreateExplosion()
        {
            GameObject explosion = Instantiate(explosionPrefab);
            explosion.SetActive(false);
            return explosion;
        }
        
        private TextMeshPro CreateDamageText()
        {
            TextMeshPro damageTest = Instantiate(damageTextPrefab);
            damageTest.gameObject.SetActive(false);
            return damageTest;
        }

    }
}
