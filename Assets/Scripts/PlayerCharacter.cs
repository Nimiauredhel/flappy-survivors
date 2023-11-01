using System;
using Data;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCharacter : MonoBehaviour
{
    private static readonly int CLIMBING_HASH = Animator.StringToHash("Climbing");
    private static readonly int DIVING_HASH = Animator.StringToHash("Diving");
    
    [SerializeField] internal Rigidbody2D _playerBody;
    [SerializeField] internal Transform _playerGraphic;
    [SerializeField] internal Animator _playerAnimator;
    [FormerlySerializedAs("_playerMovementData")] [SerializeField] private PlayerMovementData movementData;

    private float _currentXSpeed;
    private float _currentYSpeed;
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
        
        public virtual void ClimbCommand(PlayerCharacter character)
        {
            character.SetNewState(character._climbState);
        }

        public virtual void DiveCommand(PlayerCharacter character)
        {
            character.SetNewState(character._diveState);
        }

        public virtual void EnterState(PlayerCharacter character)
        {
            
        }

        public virtual void UpdateState(PlayerCharacter character)
        {
            
        }

        public virtual void FixedUpdateState(PlayerCharacter character)
        {
            float xSpeed = 0.0f;
            float ySpeed = 0.0f;
            
            if ((character._currentXSpeed > 0.0f && character._playerBody.position.x > character.movementData.MaxX)
                || (character._currentXSpeed < 0.0f && character._playerBody.position.x < character.movementData.MinX))
            {
                xSpeed = 0.0f;
            }
            else
            {
                xSpeed = character._currentXSpeed;
            }
            
            if ((character._currentYSpeed > 0.0f && character._playerBody.position.y > character.movementData.MaxY)
                || (character._currentYSpeed < 0.0f && character._playerBody.position.y < character.movementData.MinY))
            {
                ySpeed = 0.0f;
            }
            else
            {
                ySpeed = character._currentYSpeed;
            }

            movementVector.Set(xSpeed, ySpeed);
            character._playerBody.MovePosition(character._playerBody.position + (movementVector * Time.fixedDeltaTime));
        }

        public virtual void ExitState(PlayerCharacter character)
        {
            
        }
    }

    private class ClimbState : PlayerState
    {
        public override void ClimbCommand(PlayerCharacter character)
        {
            
        }

        public override void EnterState(PlayerCharacter character)
        {
            character.SetGoUp();
        }
    }
    
    private class DiveState : PlayerState
    {
        public override void DiveCommand(PlayerCharacter character)
        {
            
        }

        public override void EnterState(PlayerCharacter character)
        {
            character.SetGoDown();
        }
    }
    
    private class NeutralState : PlayerState
    {
        public override void EnterState(PlayerCharacter character)
        {
            character.SetNeutral();
        }
    }
    
    #endregion

    public void DoUpdate()
    {
        _currentState.UpdateState(this);
    }

    public void DoFixedUpdate()
    {
        _currentState.FixedUpdateState(this);
    }
    
    public void ClimbCommand()
    {
        _currentState.ClimbCommand(this);
    }

    public void DiveCommand()
    {
        _currentState.DiveCommand(this);
    }

    private void Awake()
    {
        SetNewState(_neutralState);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ScrolledObject SO = other.gameObject.GetComponentInParent<ScrolledObject>();

        if (SO != null && SO.Active)
        {
            Debug.Log("Collided with SO!");
            
            SO.Deactivate();
            
            /*switch (SO.ObjectType)
            {
                case ScrolledObject.ScrolledObjectType.None:
                    break;
                case ScrolledObject.ScrolledObjectType.Enemy:
                    break;
                case ScrolledObject.ScrolledObjectType.Experience:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }*/
        }
    }

    private void SetNewState(PlayerState newState)
    {
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    private void SetGoUp()
    {
        ySpeedTweener?.Kill();
        ySpeedTweener = DOTween.To(() => _currentYSpeed, y => _currentYSpeed = y, movementData.ClimbSpeed, movementData.ClimbAccelTime);
        
        rotationTweener?.Kill();
        rotationTweener = _playerGraphic.DORotate(new Vector3(0.0f, 0.0f, 35.0f), movementData.ClimbAccelTime);
        
        xSpeedTweener?.Kill();
        xSpeedTweener = DOTween.To(() => _currentXSpeed, x => _currentXSpeed = x, -movementData.ReverseSpeed, movementData.ClimbAccelTime);
        
        _playerAnimator.Play(CLIMBING_HASH);
    }

    private void SetGoDown()
    {
        ySpeedTweener?.Kill();
        ySpeedTweener = DOTween.To(() => _currentYSpeed, y => _currentYSpeed = y, -movementData.DiveSpeed, movementData.DiveAccelTime);
        
        rotationTweener?.Kill();
        rotationTweener = _playerGraphic.DORotate(new Vector3(0.0f, 0.0f, -35.0f), movementData.DiveAccelTime);
        
        xSpeedTweener?.Kill();
        xSpeedTweener = DOTween.To(() => _currentXSpeed, x => _currentXSpeed = x, movementData.ForwardSpeed, movementData.DiveAccelTime);
        
        _playerAnimator.Play(DIVING_HASH);
    }

    private void SetNeutral()
    {
        ySpeedTweener?.Kill();
        ySpeedTweener = DOTween.To(() => _currentYSpeed, y => _currentYSpeed = y, 0.0f, 0.5f);
        _playerBody.SetRotation(Quaternion.Euler(0.0f, 0.0f, 0.0f));
    }

}
