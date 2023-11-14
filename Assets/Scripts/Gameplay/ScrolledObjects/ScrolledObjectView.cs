using System;
using UnityEngine;

namespace Gameplay.ScrolledObjects
{
    public class ScrolledObjectView : MonoBehaviour
    {
        public bool Active => active;

        [SerializeField] private Collider2D hurtBox;
        [SerializeField] private SpriteRenderer graphic;
        [SerializeField] private GameObject deathEffect;

        private bool active = false;
        private IScrolledObjectLogic logic;

        public void Initialize(IScrolledObjectLogic injectedLogic)
        {
            logic = injectedLogic;
        }

        public void ScrolledObjectUpdate()
        {
            logic.ScrolledObjectUpdate(this);
        }
        
        public void ScrolledObjectFixedUpdate()
        {
            logic.ScrolledObjectFixedUpdate(this);
        }

        public void HitByWeapon(float damage)
        {
            logic.OnHitByWeapon(this, damage);
        }

        public void HitByPlayer(Action<float> hpAction, Action<int> xpAction)
        {
            logic.OnHitByPlayer(this, hpAction, xpAction);
        }

        public void Activate(int value)
        {
            logic.OnActivate(value);
            graphic.gameObject.SetActive(true);
            hurtBox.enabled = true;
            deathEffect.SetActive(false);
            active = true;
        }
        
        public void Deactivate(bool dieEffect = false)
        {
            if (dieEffect)
            {
                deathEffect.SetActive(true);
            }

            logic.OnDeactivate();
            graphic.gameObject.SetActive(false);
            hurtBox.enabled = false;
            active = false;
        }
    }
}
