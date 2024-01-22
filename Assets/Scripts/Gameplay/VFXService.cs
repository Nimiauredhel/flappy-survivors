using System.Collections.Generic;
using System.Linq;
using Configuration;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class VFXService
    {
        [Inject] private readonly Transform vfxParent;
        [Inject] private readonly VFXConfiguration config;
        
        private ObjectPool<GameObject> pooledExplosions;
        private ObjectPool<TextMeshPro> pooledDamageText;

        private Color baselineTint = Color.white;
        private float baselineEmission = 1.0f;
        private float baselineContrastRange = 1.0f;
        
        private Camera gameplayCamera;
        private Tween cameraShake = null;
        private Tween materialEmissionTween = null;
        private Tween materialContrastTween = null;

        public void Initialize()
        {
            baselineTint = config.InitialBaselineTint;
            config.SharedSpriteMaterial.SetColor(config.TintHash, baselineTint);
            baselineEmission = config.InitialBaselineEmission;
            config.SharedSpriteMaterial.SetFloat(config.EmissionHash, baselineEmission);
            baselineContrastRange = config.InitialBaselineContrastRange;
            config.SharedSpriteMaterial.SetFloat(config.ContrastRangeHash, baselineContrastRange);
            
            gameplayCamera = Camera.main;
            cameraShake = DOTween.Sequence();
            pooledExplosions = new ObjectPool<GameObject>(CreateExplosion);
            pooledDamageText = new ObjectPool<TextMeshPro>(CreateDamageText);
        }

        public void Dispose()
        {
            config.Cleanup();
        }

        public void ChangeBaselineTint(Color newValue)
        {
            baselineTint = newValue;
            config.SharedSpriteMaterial.DOColor(baselineTint, config.TintHash, config.TintChangeDelay).SetUpdate(true);
        }
        
        public void ChangeBaselineEmission(float newValue)
        {
            if (!Preferences.FlashVFX) return;
            baselineEmission = newValue;
            config.SharedSpriteMaterial.DOFloat(baselineEmission, config.EmissionHash, config.EmissionChangeDelay);
        }
        
        public void ChangeBaselineContrastRange(float newValue)
        {
            if (!Preferences.ContrastVFX) return;
            baselineContrastRange = newValue;
            config.SharedSpriteMaterial.DOFloat(baselineContrastRange, config.ContrastRangeHash, config.ContrastRangeChangeDelay);
        }

        public void DoCameraShake(float strength)
        {
            cameraShake?.Kill(true);

            Vector3 strengthVector = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0) * strength;
            cameraShake = gameplayCamera.DOShakePosition(0.25f, strengthVector);
            cameraShake.onComplete += () => cameraShake.Rewind();
        }

        public void RequestExplosionAt(Vector2 position, bool doLight = true)
        {
            if (!Preferences.ExplosionVFX) return;
            _ = ServeExplosionAsync(position);
            if (doLight) LightForSeconds(1.5f);
        }

        public async Awaitable RequestExplosionsAt(List<Vector3> positions, bool doLight = true, float staggerMax = 0.0f, float staggerMin = 0.0f)
        {
            if (!Preferences.ExplosionVFX) return;
            bool doneLight = false;
            
            System.Random r = new System.Random();
            
            foreach (int i in Enumerable.Range(0, positions.Count).OrderBy(x => r.Next()))
            {
                RequestExplosionAt(positions[i], false);

                if (!doneLight && doLight)
                {
                    doneLight = true;
                    LightForSeconds(3.0f);
                }

                if (staggerMax != 0.0f)
                {
                    await Awaitable.WaitForSecondsAsync(Random.Range(staggerMin, staggerMax));
                }
                else
                {
                    await Awaitable.NextFrameAsync();
                }
            }
            
            if (doLight)
            {
                LightForSeconds(3.0f);
            }
        }

        public void RequestDamageTextAt(int damage, Vector2 position)
        {
            _ = ServeDamageTextAsync(damage, position);
        }

        private async Awaitable ServeExplosionAsync(Vector2 position)
        {
            if (!Preferences.ExplosionVFX) return;
            pooledExplosions.Get(out var explosion);

            if (explosion)
            {
                explosion.transform.position = position;
                explosion.transform.eulerAngles = new Vector3(0.0f, 0.0f, Random.Range(-15.0f, 15.0f));
                explosion.SetActive(true);
                await Awaitable.WaitForSecondsAsync(config.ExplosionDelay);
                explosion.SetActive(false);
                pooledExplosions.Release(explosion);
            }

            await Awaitable.NextFrameAsync();
        }

        public void LightForSeconds(float duration)
        {
            if (!Preferences.FlashVFX) return;
            materialEmissionTween?.Kill();

            config.SharedSpriteMaterial.SetFloat(config.EmissionHash, baselineEmission + 0.25f);
            materialEmissionTween = config.SharedSpriteMaterial.DOFloat(baselineEmission, config.EmissionHash, duration);
        }

        public void ContrastForSeconds(float duration)
        {
            if (!Preferences.ContrastVFX) return;
            materialContrastTween?.Kill();

            config.SharedSpriteMaterial.SetFloat(config.ContrastRangeHash, baselineContrastRange + 1.0f);
            materialContrastTween = config.SharedSpriteMaterial.DOFloat(baselineContrastRange, config.ContrastRangeHash, duration);
        }

        private async Awaitable ServeDamageTextAsync(int damage, Vector2 position)
        {
            pooledDamageText.Get(out TextMeshPro damageText);
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
                await Awaitable.WaitForSecondsAsync(config.DamageTextInDelay);
                damageText.DOFade(0.0f, 0.5f);
                await Awaitable.WaitForSecondsAsync(config.DamageTextOutDelay);
                damageText.gameObject.SetActive(false);
                pooledDamageText.Release(damageText);
            }

            await Awaitable.NextFrameAsync();
        }

        private GameObject CreateExplosion()
        {
            GameObject explosion = Object.Instantiate(config.ExplosionPrefab, vfxParent);
            explosion.SetActive(false);
            return explosion;
        }
        
        private TextMeshPro CreateDamageText()
        {
            TextMeshPro damageText = Object.Instantiate(config.DamageTextPrefab, vfxParent);
            damageText.gameObject.SetActive(false);
            return damageText;
        }
    }
}
