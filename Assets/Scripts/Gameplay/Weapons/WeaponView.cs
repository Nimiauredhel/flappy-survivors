using System;
using FMODUnity;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponView : MonoBehaviour
    {
        public event EventHandler<Collider2D> TriggerEnter;

        public SpriteRenderer Graphic => graphic;
        public Collider2D Hitbox => hitbox;
        public Animator Animator => animator;

        [SerializeField] private SpriteRenderer graphic;
        [SerializeField] private Collider2D hitbox;
        [SerializeField] private Animator animator;
        [SerializeField] private EventReference drawEventReference;
        [SerializeField] private EventReference sheatheEventReference;

        public void PlayDrawSound()
        {
            if (!drawEventReference.IsNull)
            {
                RuntimeManager.PlayOneShot(drawEventReference);
            }
        }
        
        public void PlaySheatheSound()
        {
            if (!sheatheEventReference.IsNull)
            {
                RuntimeManager.PlayOneShot(sheatheEventReference);
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (Physics.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer)) return;
            
            TriggerEnter?.Invoke(this, other);
        }

        public void ResetEventSubscription()
        {
            TriggerEnter = null;
        }

        public void SetHitArea(float hitArea = 1.0f)
        {
            hitbox.transform.localScale = Vector3.one * hitArea;
        }
    }
}
