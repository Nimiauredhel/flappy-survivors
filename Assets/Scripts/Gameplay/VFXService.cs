using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class VFXService : MonoBehaviour
    {
        private static readonly int EMISSION_HASH = Shader.PropertyToID("_Emission");
        
        private static readonly WaitForSeconds WaitForExplosion = new WaitForSeconds(1.0f);
        private static readonly WaitForSeconds WaitForDamageTextIn = new WaitForSeconds(0.3f);
        private static readonly WaitForSeconds WaitForDamageTextOut = new WaitForSeconds(0.7f);
        
        private ObjectPool<GameObject> pooledExplosions;
        private ObjectPool<TextMeshPro> pooledDamageText;
        
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private TextMeshPro damageTextPrefab;
        [SerializeField] private Material sharedSpriteMaterial;
        
        private Camera gameplayCamera;
        private Tween cameraShake = null;
        private Tween materialEmission = null;

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

        public void RequestExplosionAt(Vector2 position, bool doLight = true)
        {
            StartCoroutine(ServeExplosion(position));
            if (doLight) LightForSeconds(1.5f);
        }

        public void RequestExplosionsAt(List<Vector3> positions, bool doLight = true)
        {
            foreach (Vector3 position in positions)
            {
                RequestExplosionAt(position, false);
            }
            
            if (doLight) LightForSeconds(1.0f);
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

        public void LightForSeconds(float duration)
        {
            if (materialEmission != null)
            {
                materialEmission.Kill();
            }

            sharedSpriteMaterial.SetFloat(EMISSION_HASH, 1.25f);
            materialEmission = sharedSpriteMaterial.DOFloat(1.0f, EMISSION_HASH, duration);
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

        private void OnDestroy()
        {
            sharedSpriteMaterial.color = Color.white;
            sharedSpriteMaterial.SetFloat(EMISSION_HASH, 1.0f);
        }
    }
}
