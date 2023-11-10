using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerView : MonoBehaviour
    {
        public event EventHandler<Collider2D> TriggerEntered; 
        public Rigidbody2D Body => _playerBody;
        public Transform Graphic => _playerGraphic;
        public Animator Animator => _playerAnimator;
        
        [SerializeField] internal Rigidbody2D _playerBody;
        [SerializeField] internal Transform _playerGraphic;
        [SerializeField] internal Animator _playerAnimator;

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEntered?.Invoke(this, other);
        }
    }
}
