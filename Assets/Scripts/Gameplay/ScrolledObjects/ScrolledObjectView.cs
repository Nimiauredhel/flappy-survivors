using System;
using System.Collections;
using Gameplay.Upgrades;
using TMPro;
using UnityEngine;

namespace Gameplay.ScrolledObjects
{
    public class ScrolledObjectView : MonoBehaviour
    {
        public event Action Activated;
        public event Action Deactivated;
        
        public bool Active => active;

        public SpriteRenderer SecondaryGraphic => secondaryGraphic;
        public TextMeshPro Text => text;
        
        [SerializeField] private Collider2D hurtBox;
        [SerializeField] private SpriteRenderer graphic;
        [SerializeField] private SpriteRenderer secondaryGraphic;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshPro text;

        private bool active = false;
        private IScrolledObjectLogic logic;
        private MaterialPropertyBlock materialPropertyBlock;
        private Coroutine flashRoutine = null;

        public void Initialize(IScrolledObjectLogic injectedLogic)
        {
            logic = injectedLogic;
            materialPropertyBlock = new MaterialPropertyBlock();
            graphic.GetPropertyBlock(materialPropertyBlock);
        }

        public void ScrolledObjectUpdate()
        {
            logic.ScrolledObjectUpdate(this);
        }
        
        public void ScrolledObjectFixedUpdate()
        {
            logic.ScrolledObjectFixedUpdate(this);
        }

        public void HitByWeapon(int damage)
        {
            Flash();
            logic.OnHitByWeapon(this, damage);
        }

        public void HitByPlayer(Action<int> hpAction, Action<int> xpAction, Action<UpgradeOption> upgradeAction)
        {
            logic.OnHitByPlayer(this, hpAction, xpAction, upgradeAction);
        }

        public void Activate(object value)
        {
            logic.OnActivate(this, value);
            graphic.gameObject.SetActive(true);
            hurtBox.enabled = true;
            active = true;

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0.0f);
            }
            
            Activated?.Invoke();
            
            SetWhiteAmount(0);
        }
        
        public void Deactivate(bool dieEffect = false)
        {
            logic.OnDeactivate(this);
            graphic.gameObject.SetActive(false);
            hurtBox.enabled = false;
            active = false;
            
            Deactivated?.Invoke();
        }
        
        public void Flash()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
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
        }

        private void SetWhiteAmount(float amount)
        {
            string propertyString = "_FlashAmount";
            materialPropertyBlock.SetFloat(propertyString, amount);
            graphic.SetPropertyBlock(materialPropertyBlock);
            //graphic.sharedMaterial.SetFloat(propertyString, amount);
        }
    }
}
