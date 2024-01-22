using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerView : MonoBehaviour
    {
        private static readonly int FLASH_AMOUNT_ID = Shader.PropertyToID("_FlashAmount");
        
        public event EventHandler<Collider2D> TriggerEntered; 
        public Rigidbody2D Body => _playerBody;
        public SpriteRenderer Graphic => _playerGraphic;
        public Animator Animator => _playerAnimator;
        
        [SerializeField] internal Rigidbody2D _playerBody;
        [SerializeField] internal SpriteRenderer _playerGraphic;
        [SerializeField] internal Animator _playerAnimator;

        private MaterialPropertyBlock materialPropertyBlock;
        private Coroutine flashRoutine = null;

        public void Initialize()
        {
            materialPropertyBlock = new MaterialPropertyBlock();
            _playerGraphic.GetPropertyBlock(materialPropertyBlock);
        }

        public void Flash()
        {
            if (!Preferences.FlashVFX) return;
            
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Physics.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer)) return;
            
            TriggerEntered?.Invoke(this, other);
        }

        private IEnumerator FlashRoutine()
        {
            
            float halfDuration = 0.15f;
            float time = 0.0f;
            
            //in
            while (time < halfDuration)
            {
                float value = Mathf.Lerp(0.0f, 0.85f, time / halfDuration);
                SetWhiteAmount(value);
                yield return null;
                time += Time.deltaTime;
            }
            
            //out
            time = 0.0f;
            while (time < halfDuration)
            {
                float value = Mathf.Lerp(0.85f, 0.0f, time / halfDuration);
                SetWhiteAmount(value);
                yield return null;
                time += Time.deltaTime;
            }
            
            SetWhiteAmount(0.0f);
        }

        private void SetWhiteAmount(float amount)
        {
            materialPropertyBlock.SetFloat(FLASH_AMOUNT_ID, amount);
            _playerGraphic.SetPropertyBlock(materialPropertyBlock);
            //_playerGraphic.sharedMaterial.SetFloat(propertyString, amount);
        }
    }
}
