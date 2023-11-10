using System;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;

namespace Gameplay.Player
{
    public class PlayerController : IStartable, ITickable, IFixedTickable, IDisposable
    {
        private static readonly int CLIMBING_HASH = Animator.StringToHash("Climbing");
        private static readonly int DIVING_HASH = Animator.StringToHash("Diving");

        [Inject] readonly TouchReceiver _touchReceiver;
        [Inject] readonly PlayerView view;
        [Inject] readonly PlayerModel model;
        [Inject] readonly PlayerWeaponsComponent weapons;
        [Inject] readonly PlayerMovementData movementData;

        private PlayerState _currentState;
        private ClimbState _climbState = new ClimbState();
        private DiveState _diveState = new DiveState();
        private NeutralState _neutralState = new NeutralState();
        private Tweener ySpeedTweener;
        private Tweener xSpeedTweener;
        private Tweener rotationTweener;

        #region Player State Machine

        private class PlayerState
        {
            private Vector2 movementVector = new Vector2();
        
            public virtual void ClimbCommand(PlayerController player)
            {
                player.SetNewState(player._climbState);
            }

            public virtual void DiveCommand(PlayerController player)
            {
                player.SetNewState(player._neutralState);
            }

            public virtual void EnterState(PlayerController player)
            {
            
            }

            public virtual void UpdateState(PlayerController player)
            {
            
            }

            public virtual void FixedUpdateState(PlayerController player)
            {
                float xSpeed = 0.0f;
                float ySpeed = 0.0f;
            
                if ((player.model.CurrentXSpeed > 0.0f && player.view.Body.position.x > player.movementData.MaxX)
                    || (player.model.CurrentXSpeed < 0.0f && player.view.Body.position.x < player.movementData.MinX))
                {
                    xSpeed = 0.0f;
                }
                else
                {
                    xSpeed = player.model.CurrentXSpeed;
                }
            
                if ((player.model.CurrentYSpeed > 0.0f && player.view.Body.position.y > player.movementData.MaxY)
                    || (player.model.CurrentYSpeed < 0.0f && player.view.Body.position.y < player.movementData.MinY))
                {
                    ySpeed = 0.0f;
                }
                else
                {
                    ySpeed = player.model.CurrentYSpeed;
                }

                movementVector.Set(xSpeed, ySpeed);
                player.view.Body.MovePosition(player.view.Body.position + (movementVector * Time.fixedDeltaTime));
            }

            public virtual void ExitState(PlayerController player)
            {
            
            }
        }

        private class InitialState : PlayerState
        {
            public override void DiveCommand(PlayerController player)
            {
                
            }
            
            public override void EnterState(PlayerController player)
            {
                player.SetNeutral();
            }
        }
        
        private class ClimbState : PlayerState
        {
            public override void ClimbCommand(PlayerController player)
            {
            
            }

            public override void EnterState(PlayerController player)
            {
                player.SetGoUp();
            }

            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(WeaponData.WeaponType.Climbing);
            }
        }

        private class DiveState : PlayerState
        {
            public override void DiveCommand(PlayerController player)
            {
            
            }

            public override void EnterState(PlayerController player)
            {
                player.SetGoDown();
            }
        
            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(WeaponData.WeaponType.Diving);
            }
        }

        private class NeutralState : PlayerState
        {
            private float timeToDive = 0.0f;
            
            public override void EnterState(PlayerController player)
            {
                timeToDive = player.movementData.NeutralDuration;
                player.SetNeutral();
            }

            public override void UpdateState(PlayerController player)
            {
                if (timeToDive <= 0.0f)
                {
                    player.SetNewState(player._diveState);
                }
                else
                {
                    timeToDive -= Time.deltaTime;
                }
            }
        }
        
        private void SetNewState(PlayerState newState)
        {
            _currentState?.ExitState(this);
            _currentState = newState;
            _currentState.EnterState(this);
        }

        #endregion

        #region Lifetime Scope Events
        
        public void Tick()
        {
            _currentState.UpdateState(this);
        }

        public void FixedTick()
        {
            _currentState.FixedUpdateState(this);
        }

        public void Start()
        {
            SetNewState(new InitialState());
            weapons.InitializeWeapons();
            _touchReceiver.PointerDown += PointerDownHandler;
            _touchReceiver.PointerUp += PointerUpHandler;
            view.TriggerEntered += TriggerEnterHandler;
        }

        public void Dispose()
        {
            _touchReceiver.PointerDown -= PointerDownHandler;
            _touchReceiver.PointerUp -= PointerUpHandler;
            view.TriggerEntered -= TriggerEnterHandler;
            rotationTweener.Kill();
            xSpeedTweener.Kill();
            ySpeedTweener.Kill();
        }
        
        #endregion

        private void PointerDownHandler(object sender, PointerEventData eventData)
        {
            _currentState.ClimbCommand(this);
        }

        private void PointerUpHandler(object sender, PointerEventData eventData)
        {
            _currentState.DiveCommand(this);
        }
        
        private void TriggerEnterHandler(object sender, Collider2D other)
        {
            ScrolledObject SO = other.gameObject.GetComponentInParent<ScrolledObject>();

            if (SO != null && SO.Active)
            {
                Debug.Log("Collided with SO!");

                TakeDamage(SO.MeleeDamage);
            }
        }

        private void TakeDamage(float damage)
        {
            model.ChangeHealth(-damage);

            if (model.Health <= 0.0f)
            {
                Die();
            }
        }

        private void Die()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        private void SetGoUp()
        {
            ySpeedTweener?.Kill();
            ySpeedTweener = DOTween.To(() => model.CurrentYSpeed, y => model.SetYSpeed(y), movementData.ClimbSpeed, movementData.ClimbAccelTime);
        
            rotationTweener?.Kill();
            rotationTweener = view.Graphic.DORotate(new Vector3(0.0f, 0.0f, 35.0f), movementData.ClimbAccelTime);
        
            xSpeedTweener?.Kill();
            xSpeedTweener = DOTween.To(() => model.CurrentXSpeed, x => model.SetXSpeed(x), -movementData.ReverseSpeed, movementData.ClimbAccelTime);
        
            view.Animator.Play(CLIMBING_HASH);
        }

        private void SetGoDown()
        {
            ySpeedTweener?.Kill();
            ySpeedTweener = DOTween.To(() => model.CurrentYSpeed, y => model.SetYSpeed(y), -movementData.DiveSpeed, movementData.DiveAccelTime);
        
            rotationTweener?.Kill();
            rotationTweener = view.Graphic.DORotate(new Vector3(0.0f, 0.0f, -35.0f), movementData.DiveAccelTime);
        
            xSpeedTweener?.Kill();
            xSpeedTweener = DOTween.To(() => model.CurrentXSpeed, x => model.SetXSpeed(x), movementData.ForwardSpeed, movementData.DiveAccelTime);
        
            view.Animator.Play(DIVING_HASH);
        }

        private void SetNeutral()
        {
            ySpeedTweener?.Kill();
            ySpeedTweener = DOTween.To(() => model.CurrentYSpeed, y => model.SetYSpeed(y), 0.0f, 0.5f);
            xSpeedTweener?.Kill();
            xSpeedTweener = DOTween.To(() => model.CurrentXSpeed, x => model.SetXSpeed(x), 0.05f, 0.5f);
            rotationTweener?.Kill();
            rotationTweener = view.Graphic.DORotate(Vector3.zero, 0.5f);
        }
    }
}
