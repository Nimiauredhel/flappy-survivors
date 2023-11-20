using System;
using System.Collections;
using System.Collections.Generic;
using Configuration;
using DG.Tweening;
using Gameplay.ScrolledObjects;
using Gameplay.Upgrades;
using Gameplay.Weapons;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Gameplay.Player
{
    public class PlayerController
    {
        private static readonly int CLIMBING_HASH = Animator.StringToHash("Climbing");
        private static readonly int DIVING_HASH = Animator.StringToHash("Diving");

        public event Action<int> ComboBreak;
        public event Action<int> LevelUp;
        public event Action<int> PlayerDamaged; 
        
        [Inject] private readonly TouchReceiver _touchReceiver;
        [Inject] private readonly PlayerView view;
        [Inject] private readonly PlayerUIView uiView;
        [Inject] private readonly UpgradesUIView upgradesUIView;
        [Inject] private readonly PlayerModel model;
        [Inject] private readonly PlayerWeaponsComponent weapons;
        [Inject] private readonly PlayerCharacterConfiguration characterConfig;
        [Inject] private readonly PlayerMovementConfiguration movementConfig;

        private readonly PlayerMagnetComponent magnet = new PlayerMagnetComponent();
        private readonly ComboService comboService = new ComboService();
        
        private PlayerState _currentState;
        private ClimbState _climbState = new ClimbState();
        private DiveState _diveState = new DiveState();
        private NeutralState _neutralState = new NeutralState();
        private Tweener ySpeedTweener;
        private Tweener xSpeedTweener;
        private Tweener rotationTweener;
        private Vector2 movementVector = new Vector2();
        
        private class ComboService
        {
            private const float COMBO_GAP = 0.5f;
            
            public event Action<int, int> ComboChanged; 
            
            private int currentCombo = 0;
            private int previousCombo = 0;
            private float timeSinceLastKill = 0.0f;

            public void DoUpdate()
            {
                if (GameModel.CurrentGamePhase == GamePhase.HordePhase)
                {
                    if (currentCombo > 0)
                    {
                        timeSinceLastKill += Time.deltaTime;

                        if (timeSinceLastKill > COMBO_GAP)
                        {
                            ComboBreak();
                        }
                    }
                }
            }

            public void ReportKill()
            {
                previousCombo = currentCombo;
                
                if (timeSinceLastKill <= COMBO_GAP + (currentCombo * 0.005f))
                {
                    currentCombo++;
                }
                else
                {
                    currentCombo = 1;
                }

                timeSinceLastKill = 0.0f;

                if (currentCombo != previousCombo)
                {
                    ComboChanged?.Invoke(currentCombo, previousCombo);
                }
            }

            private void ComboBreak()
            {
                currentCombo = 0;
                ComboChanged?.Invoke(currentCombo, previousCombo);
            }
        }

        #region Player State Machine

        private class PlayerState
        {
            private protected static WeaponType validWeaponType = WeaponType.None;
            
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
                validWeaponType = WeaponType.None;
            }

            public virtual void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(WeaponType.Both);
            }

            public virtual void FixedUpdateState(PlayerController player)
            {
                player.HandleMovement();
                player.weapons.WeaponsFixedUpdate(validWeaponType);
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
                validWeaponType = WeaponType.None;
                player.SetNeutral();
            }

            public override void UpdateState(PlayerController player)
            {
                
            }
        }
        
        private class ClimbState : PlayerState
        {
            public override void ClimbCommand(PlayerController player)
            {
            
            }

            public override void EnterState(PlayerController player)
            {
                validWeaponType = WeaponType.Climbing;
                player.SetGoUp();
            }

            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(validWeaponType);
            }
        }

        private class DiveState : PlayerState
        {
            public override void DiveCommand(PlayerController player)
            {
            
            }

            public override void EnterState(PlayerController player)
            {
                validWeaponType = WeaponType.Diving;
                player.SetGoDown();
            }
        
            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(validWeaponType);
            }
        }

        private class NeutralState : PlayerState
        {
            private float timeToDive = 0.0f;
            
            public override void EnterState(PlayerController player)
            {
                validWeaponType = WeaponType.Both;
                timeToDive = player.movementConfig.NeutralDuration;
                player.SetNeutral();
            }

            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(validWeaponType);
                
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
        
        public void DoUpdate()
        {
            _currentState.UpdateState(this);
            comboService.DoUpdate();
        }

        public void DoFixedUpdate()
        {
            _currentState.FixedUpdateState(this);
            magnet.DoFixedUpdate(model.MagnetStrength);
        }

        public void Initialize()
        {
            SetNewState(new InitialState());

            model.InitializeModel(characterConfig);
            view.Initialize();
            weapons.Initialize(view.Graphic.transform, characterConfig.StartingWeapons, uiView);
            magnet.Initialize(view.Graphic.transform);
            
            _touchReceiver.PointerDown += PointerDownHandler;
            _touchReceiver.PointerUp += PointerUpHandler;
            view.TriggerEntered += TriggerEnterHandler;
            
            uiView.UpdatePlayerHealthView((float)model.CurrentHealth/model.MaxHealth);
            uiView.UpdatePlayerXPView(0.0f);
            
            model.HealthPercentChanged += uiView.UpdatePlayerHealthView;
            model.XPPercentChanged += uiView.UpdatePlayerXPView;
            comboService.ComboChanged += HandleComboChanged;

            uiView.StartCoroutine(TimerRoutine());
        }

        public void OnDispose()
        {
            _touchReceiver.PointerDown -= PointerDownHandler;
            _touchReceiver.PointerUp -= PointerUpHandler;
            view.TriggerEntered -= TriggerEnterHandler;
            rotationTweener.Kill();
            xSpeedTweener.Kill();
            ySpeedTweener.Kill();

            model.HealthPercentChanged -= uiView.UpdatePlayerHealthView;
            model.XPPercentChanged -= uiView.UpdatePlayerXPView;
            comboService.ComboChanged -= HandleComboChanged;
        }

        #region Movement
        
        private void HandleMovement()
        {
            float xSpeed = 0.0f;
            float ySpeed = 0.0f;
            
            if ((model.CurrentXSpeed > 0.0f && view.Body.position.x > movementConfig.MaxX)
                || (model.CurrentXSpeed < 0.0f && view.Body.position.x < movementConfig.MinX))
            {
                xSpeed = 0.0f;
            }
            else
            {
                xSpeed = model.CurrentXSpeed;
            }
            
            if ((model.CurrentYSpeed > 0.0f && view.Body.position.y > movementConfig.MaxY)
                || (model.CurrentYSpeed < 0.0f && view.Body.position.y < movementConfig.MinY))
            {
                ySpeed = 0.0f;
            }
            else
            {
                ySpeed = model.CurrentYSpeed;
            }

            movementVector.Set(xSpeed, ySpeed);
            view.transform.position += (Vector3)movementVector * Time.fixedDeltaTime;
            //view.Body.MovePosition(view.Body.position + (movementVector * Time.fixedDeltaTime));
        }

        private void PointerDownHandler(object sender, PointerEventData eventData)
        {
            _currentState.ClimbCommand(this);
        }

        private void PointerUpHandler(object sender, PointerEventData eventData)
        {
            _currentState.DiveCommand(this);
        }
        
        #endregion
        
        public void HandleEnemyKilled()
        {
            comboService.ReportKill();
        }

        private void HandleComboChanged(int currentCombo, int previousCombo)
        {
            uiView.UpdatePlayerCurrentComboText(currentCombo);

            if (currentCombo == 0)
            {
                ComboBreak?.Invoke(previousCombo);
            }
        }

        private void ChangePlayerXP(int value)
        {
            bool levelUp = false;
            model.ChangeXP(value, out levelUp);

            if (levelUp)
            {
                LevelUpHandler();
            }
        }
        
        private void TriggerEnterHandler(object sender, Collider2D other)
        {
            ScrolledObjectView SO = other.gameObject.GetComponentInParent<ScrolledObjectView>();
            
            if (SO != null && SO.Active)
            {
                SO.HitByPlayer(ChangePlayerHealth, ChangePlayerXP, SelectedUpgradeHandler);
            }
        }

        private void LevelUpHandler()
        {
            uiView.UpdatePlayerCurrentLevelText(model.CurrentLevel);
            LevelUp?.Invoke(model.CurrentLevel);
        }

        private void SelectedUpgradeHandler(UpgradeOption selectedOption)
        {
            Time.timeScale = 1.0f;
            selectedOption.Taken = true;

            switch (selectedOption.UpgradeConfig.Type())
            {
                case UpgradeType.None:
                    break;
                
                case UpgradeType.Stats:
                    StatUpgradeConfiguration statConfig = selectedOption.UpgradeConfig as StatUpgradeConfiguration;
                    
                    if (statConfig != null)
                    {
                        model.UpgradeStats(statConfig.Stats);
                    }
                    break;
                
                case UpgradeType.Weapon:
                    WeaponConfiguration weaponConfig = selectedOption.UpgradeConfig as WeaponConfiguration;
                    
                    if (weaponConfig != null)
                    {
                        weapons.AddOrUpgradeWeapon(view.Graphic.transform, weaponConfig, uiView);
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
        }

        private void ChangePlayerHealth(int amount)
        {
            model.ChangeHealth(amount);
            
            if (amount < 0)
            {
                OnPlayerWasDamaged(amount);
            }
        }
        
        private void OnPlayerWasDamaged(int damage)
        {
            if (model.CurrentHealth <= 0.0f)
            {
                Die();
            }
            else
            {
                view.Flash();
                PlayerDamaged?.Invoke(damage);
            }
        }

        private void Die()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        private void SetGoUp()
        {
            ySpeedTweener?.Kill();
            ySpeedTweener = DOTween.To(() => model.CurrentYSpeed, y => model.SetYSpeed(y), movementConfig.ClimbSpeed, movementConfig.ClimbAccelTime);
        
            rotationTweener?.Kill();
            rotationTweener = view.Graphic.transform.DORotate(new Vector3(0.0f, 0.0f, 35.0f), movementConfig.ClimbAccelTime);
        
            xSpeedTweener?.Kill();
            xSpeedTweener = DOTween.To(() => model.CurrentXSpeed, x => model.SetXSpeed(x), -movementConfig.ReverseSpeed, movementConfig.ClimbAccelTime);
        
            view.Animator.Play(CLIMBING_HASH);
        }

        private void SetGoDown()
        {
            ySpeedTweener?.Kill();
            ySpeedTweener = DOTween.To(() => model.CurrentYSpeed, y => model.SetYSpeed(y), -movementConfig.DiveSpeed, movementConfig.DiveAccelTime);
        
            rotationTweener?.Kill();
            rotationTweener = view.Graphic.transform.DORotate(new Vector3(0.0f, 0.0f, -35.0f), movementConfig.DiveAccelTime);
        
            xSpeedTweener?.Kill();
            xSpeedTweener = DOTween.To(() => model.CurrentXSpeed, x => model.SetXSpeed(x), movementConfig.ForwardSpeed, movementConfig.DiveAccelTime);
        
            view.Animator.Play(DIVING_HASH);
        }

        private void SetNeutral()
        {
            ySpeedTweener?.Kill();
            ySpeedTweener = DOTween.To(() => model.CurrentYSpeed, y => model.SetYSpeed(y), 0.0f, 0.5f);
            xSpeedTweener?.Kill();
            xSpeedTweener = DOTween.To(() => model.CurrentXSpeed, x => model.SetXSpeed(x), 0.05f, 0.5f);
            rotationTweener?.Kill();
            rotationTweener = view.Graphic.transform.DORotate(Vector3.zero, 0.5f);
        }

        private IEnumerator TimerRoutine()
        {
            WaitForSeconds second = new WaitForSeconds(1);
            int secondsLeft = 5 * 60 ;

            while (secondsLeft > 0)
            {
                while (GameModel.CurrentGamePhase != GamePhase.HordePhase)
                {
                    yield return null;
                }

                uiView.UpdateTimerText(secondsLeft);
                secondsLeft--;
                yield return second;
            }
            
            Die();
        }
    }
}
