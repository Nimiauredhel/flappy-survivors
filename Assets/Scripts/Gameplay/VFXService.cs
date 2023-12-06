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
        
        private const float EXPLOSION_DELAY = 1.0f;
        private const float WaitForDamageTextIn = 0.3f;
        private const float WaitForDamageTextOut = 0.7f;
        
        private ObjectPool<GameObject> pooledExplosions;
        private ObjectPool<TextMeshPro> pooledDamageText;
        
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private TextMeshPro damageTextPrefab;
        [SerializeField] private Material sharedSpriteMaterial;

        private float baselineEmission = 1.0f;
        
        private Camera gameplayCamera;
        private Tween cameraShake = null;
        private Tween materialEmission = null;

        public void Initialize()
        {
            baselineEmission = 1.0f;
            sharedSpriteMaterial.SetFloat(EMISSION_HASH, baselineEmission);
            gameplayCamera = Camera.main;
            cameraShake = DOTween.Sequence();
            pooledExplosions = new ObjectPool<GameObject>(CreateExplosion);
            pooledDamageText = new ObjectPool<TextMeshPro>(CreateDamageText);
        }

        public void ChangeBaselineEmission(float newValue)
        {
            baselineEmission = newValue;
            sharedSpriteMaterial.DOFloat(baselineEmission, EMISSION_HASH, 3.0f);
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
            ServeExplosionAsync(position);
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
            ServeDamageTextAsync(damage, position);
        }

        private async void ServeExplosionAsync(Vector2 position)
        {
            GameObject explosion;
            pooledExplosions.Get(out explosion);

            if (explosion != null)
            {
                explosion.transform.position = position;
                explosion.SetActive(true);
                await Awaitable.WaitForSecondsAsync(EXPLOSION_DELAY);
                explosion.SetActive(false);
                pooledExplosions.Release(explosion);
            }

            await Awaitable.NextFrameAsync();
        }

        public void LightForSeconds(float duration)
        {
            if (materialEmission != null)
            {
                materialEmission.Kill();
            }

            sharedSpriteMaterial.SetFloat(EMISSION_HASH, baselineEmission + 0.25f);
            materialEmission = sharedSpriteMaterial.DOFloat(baselineEmission, EMISSION_HASH, duration);
        }

        private async void ServeDamageTextAsync(int damage, Vector2 position)
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
                await Awaitable.WaitForSecondsAsync(WaitForDamageTextIn);
                damageText.DOFade(0.0f, 0.5f);
                await Awaitable.WaitForSecondsAsync(WaitForDamageTextOut);
                damageText.gameObject.SetActive(false);
                pooledDamageText.Release(damageText);
            }

            await Awaitable.NextFrameAsync();
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
            sharedSpriteMaterial.SetFloat(EMISSION_HASH, 1.0f);
        }
    }
}
