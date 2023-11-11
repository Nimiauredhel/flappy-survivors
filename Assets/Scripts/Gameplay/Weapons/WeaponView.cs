using System;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class WeaponView : MonoBehaviour
    {
        public event EventHandler<Collider2D> TriggerEnter;

        public SpriteRenderer Graphic => graphic;
        public Collider2D Hitbox => hitbox;

        [SerializeField] private SpriteRenderer graphic;
        [SerializeField] private Collider2D hitbox;
        
        public void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter?.Invoke(this, other);
        }
    }
}