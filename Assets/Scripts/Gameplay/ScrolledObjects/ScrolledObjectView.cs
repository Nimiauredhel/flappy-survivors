using System;
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

        public void HitByWeapon(int damage)
        {
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
        }
        
        public void Deactivate(bool dieEffect = false)
        {
            logic.OnDeactivate(this);
            graphic.gameObject.SetActive(false);
            hurtBox.enabled = false;
            active = false;
            
            Deactivated?.Invoke();
        }
    }
}
