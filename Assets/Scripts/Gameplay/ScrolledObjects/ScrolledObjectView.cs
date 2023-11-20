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
            ShowDamage(damage);
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
        
        public void ShowDamage(int damage)
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(ShowDamageRoutine(damage));
        }
        
        private IEnumerator ShowDamageRoutine(int damage)
        {
            float halfDuration = 0.15f;
            float time = 0.0f;
            float percent;
            float whiteValue;
            
            text.text = damage.ToString();
            text.alpha = 0.0f;
            text.gameObject.SetActive(true);

            //in
            while (time < halfDuration)
            {
                percent = time / halfDuration;
                text.alpha = percent;
                whiteValue = Mathf.Lerp(0.0f, 0.85f, percent);
                SetWhiteAmount(whiteValue);
                yield return null;
                time += Time.deltaTime;
            }
            
            //out
            time = 0.0f;
            while (time < halfDuration)
            {
                percent = time / halfDuration;
                text.alpha = 1.0f - percent;
                whiteValue = Mathf.Lerp(0.85f, 0.0f, percent);
                SetWhiteAmount(whiteValue);
                yield return null;
                time += Time.deltaTime;
            }

            text.gameObject.SetActive(false);
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
