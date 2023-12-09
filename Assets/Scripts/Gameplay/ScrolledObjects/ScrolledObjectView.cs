using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;
using Random = System.Random;

namespace Gameplay.ScrolledObjects
{
    public class ScrolledObjectView : MonoBehaviour
    {
        private static readonly int FLASH_AMOUNT_ID = Shader.PropertyToID("_FlashAmount");
        
        public event Action Activated;
        public event Action Deactivated;
        
        public bool Active => active;

        public SpriteRenderer[] Graphics => graphics;
        public SpriteRenderer SecondaryGraphic => secondaryGraphic;
        public Rigidbody2D Body => body;
        public TextMeshPro Text => text;
        
        [SerializeField] private HitTrigger[] hurtBoxes;
        [SerializeField] private HitTrigger[] hitBoxes;
        [SerializeField] private SpriteRenderer[] graphics;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private SpriteRenderer secondaryGraphic;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshPro text;

        private bool active = false;
        private bool deactivating = false;
        private IScrolledObjectLogic logic;
        private MaterialPropertyBlock materialPropertyBlock;
        private Coroutine flashRoutine = null;

        public void Initialize(IScrolledObjectLogic injectedLogic)
        {
            logic = injectedLogic;
            
            if (graphics.Length > 0)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
                graphics[0].GetPropertyBlock(materialPropertyBlock);
            }
        }

        public void SetPath(Spline path)
        {
            logic.SetPath(path);
        }

        public void ScrolledObjectUpdate()
        {
            logic.ScrolledObjectUpdate(this);
        }
        
        public void ScrolledObjectFixedUpdate()
        {
            logic.ScrolledObjectFixedUpdate(this);
        }

        public void Activate(object value, bool forbidPathRotation = false, float speedOverride = 0.0f)
        {
            gameObject.SetActive(true);
            logic.OnActivate(this, value, forbidPathRotation, speedOverride);

            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].gameObject.SetActive(true);
            }
            
            SetHurtboxEnabled(true);
            SetHitboxEnabled(true);
            
            active = true;

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0.0f);
            }
            
            Activated?.Invoke();
            
            SetWhiteAmount(0);
        }
        
        public async Awaitable Deactivate()
        {
            if (deactivating) return;
            deactivating = true;
            
            logic.OnDeactivate(this);
            SetHurtboxEnabled(false);
            SetHitboxEnabled(false);

            if (graphics.Length > 1)
            {
                System.Random r = new System.Random();
            
                foreach (int i in Enumerable.Range(0, graphics.Length).OrderBy(x => r.Next()))
                {
                    graphics[i].gameObject.SetActive(false);
                    await Awaitable.WaitForSecondsAsync(UnityEngine.Random.Range(0.05f, 0.1f));
                }
            }
            else if (graphics.Length == 1)
            {
                graphics[0].gameObject.SetActive(false);
            }
            
            active = false;
            deactivating = false;
            gameObject.SetActive(false);
            
            Deactivated?.Invoke();
        }
        
        public void HitByWeapon(int damage)
        {
            ShowDamage(damage);
            logic.OnHitByWeapon(this, damage);
        }

        public void HitPlayer(Action<int> hpAction, Action<int> xpAction, Action<UpgradeOption> upgradeAction)
        {
            logic.OnHitPlayer(this, hpAction, xpAction, upgradeAction);
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

            //in
            while (time < halfDuration)
            {
                percent = time / halfDuration;
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
                whiteValue = Mathf.Lerp(0.85f, 0.0f, percent);
                SetWhiteAmount(whiteValue);
                yield return null;
                time += Time.deltaTime;
            }

            SetWhiteAmount(0.0f);
        }

        private void SetWhiteAmount(float amount)
        {
            materialPropertyBlock.SetFloat(FLASH_AMOUNT_ID, amount);
            
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].SetPropertyBlock(materialPropertyBlock);
            }
        }

        private void SetHurtboxEnabled(bool value)
        {
            for (int i = 0; i < hurtBoxes.Length; i++)
            {
                hurtBoxes[i].gameObject.SetActive(value);
            }
        }
        
        private void SetHitboxEnabled(bool value)
        {
            for (int i = 0; i < hitBoxes.Length; i++)
            {
                hitBoxes[i].gameObject.SetActive(value);
            }
        }
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            if (hitBoxes.Length == 0 || hurtBoxes.Length == 0)
            {
                HitTrigger[] hitTriggers = GetComponentsInChildren<HitTrigger>();

                List<HitTrigger> hits = new List<HitTrigger>();
                List<HitTrigger> hurts = new List<HitTrigger>();

                foreach (HitTrigger trigger in hitTriggers)
                {
                    if (trigger.gameObject.name.Contains("Hitbox"))
                    {
                        hits.Add(trigger);
                    }
                    else if (trigger.gameObject.name.Contains("Hurtbox"))
                    {
                        hurts.Add(trigger);
                    }
                }

                hitBoxes = hits.ToArray();
                hurtBoxes = hurts.ToArray();
            }
        }

        #endif
    }
}
