using System;
using CommandTerminal;
using Configuration;
using DG.Tweening;
using Gameplay.ScrolledObjects;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Gameplay.Player
{
    public class PlayerController
    {
        private static readonly int CLIMBING_HASH = Animator.StringToHash("Climbing");
        private static readonly int DIVING_HASH = Animator.StringToHash("Diving");
        private static readonly int HOVERING_HASH = Animator.StringToHash("Hovering");
        private static readonly int HURT_HASH = Animator.StringToHash("Hurt");
        private static readonly int DYING_HASH = Animator.StringToHash("Dying");
        
        public event Action<int> ComboBreak;
        public event Action<int> LevelUp;
        public event Action PlayerStartedMoving;
        public event Action<float, float> PlayerHeightChanged;
        public event Action<int> PlayerDamaged;
        public event Action PlayerDied;

        public bool PlayerIsDead => model.Dead;
        
        [Inject] private readonly TouchReceiver _touchReceiver;
        [Inject] private readonly PlayerView view;
        [Inject] private readonly PlayerUIView uiView;
        [Inject] private readonly PlayerModel model;
        [Inject] private readonly PlayerMovementConfiguration movementConfig;

        private readonly PlayerWeaponsComponent weapons = new PlayerWeaponsComponent();
        private readonly PlayerMagnetComponent magnet = new PlayerMagnetComponent();
        private readonly ComboService comboService = new ComboService();

        private bool hasControl = false;
        private bool inputPressed = false;
        private float lastHeight = 0.0f;
        
        private PlayerCharacterConfiguration characterConfig;
        
        private PlayerState _currentState;
        private ClimbState _climbState = new ClimbState();
        private DiveState _diveState = new DiveState();
        private NeutralState _neutralState = new NeutralState();
        private HurtState _hurtState = new HurtState();
        private DyingState _dyingState = new DyingState();
        
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
                if (GameModel.CurrentGamePhase >= GamePhase.HordePhase)
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
            private protected static Weapons.PlayerState currentPlayerState = Weapons.PlayerState.None;
            
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
                currentPlayerState = Weapons.PlayerState.None;
            }

            public virtual void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(Weapons.PlayerState.Both);
                
                if (!player.hasControl)
                {
                    player.SetNewState(new InitialState());
                }
            }

            public virtual void FixedUpdateState(PlayerController player)
            {
                if (player.inputPressed)
                {
                    ClimbCommand(player);
                }
                else
                {
                    DiveCommand(player);
                }

                player.HandleMovement();
                player.weapons.WeaponsFixedUpdate(currentPlayerState);
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
            
            public override void ClimbCommand(PlayerController player)
            {
                if (player.hasControl)
                {
                    player.SetNewState(player._climbState);
                }
            }
            
            public override void EnterState(PlayerController player)
            {
                currentPlayerState = Weapons.PlayerState.None;
                player.SetNeutral();
                player.model.SetVulnerable(false);
            }

            public override void UpdateState(PlayerController player)
            {
                
            }

            public override void ExitState(PlayerController player)
            {
                player.PlayerStartedMoving?.Invoke();
                player.model.SetVulnerable(true);
            }
        }
        
        private class ClimbState : PlayerState
        {
            public override void ClimbCommand(PlayerController player)
            {
            
            }

            public override void EnterState(PlayerController player)
            {
                currentPlayerState = Weapons.PlayerState.Climbing;
                player.SetGoUp();
            }

            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(currentPlayerState);
                
                if (!player.hasControl)
                {
                    player.SetNewState(new InitialState());
                }
            }
        }

        private class DiveState : PlayerState
        {
            public override void DiveCommand(PlayerController player)
            {
            
            }

            public override void EnterState(PlayerController player)
            {
                currentPlayerState = Weapons.PlayerState.Diving;
                player.SetGoDown();
            }
        
            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(currentPlayerState);
                
                if (!player.hasControl)
                {
                    player.SetNewState(new InitialState());
                }
            }
        }

        private class NeutralState : PlayerState
        {
            private float timeToDive = 0.0f;
            
            public override void DiveCommand(PlayerController player)
            {
            
            }
            
            public override void EnterState(PlayerController player)
            {
                currentPlayerState = Weapons.PlayerState.Both;
                timeToDive = player.movementConfig.NeutralDuration;
                player.SetNeutral();
            }

            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(currentPlayerState);
                
                if (!player.hasControl)
                {
                    player.SetNewState(new InitialState());
                    return;
                }
                
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
        
        private class HurtState : PlayerState
        {
            private float timeToRecover = 0.0f;

            public override void ClimbCommand(PlayerController player)
            {
                if (timeToRecover <= 0.01f)
                {
                    player.SetNewState(player._climbState);
                }
            }

            public override void DiveCommand(PlayerController player)
            {
                if (timeToRecover <= 0.01f)
                {
                    player.SetNewState(player._diveState);
                }
            }

            public override void EnterState(PlayerController player)
            {
                player.model.SetVulnerable(false);
                currentPlayerState = Weapons.PlayerState.None;
                timeToRecover = player.movementConfig.FlinchDuration;
                player.SetHurt();
            }

            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(currentPlayerState);
                
                if (timeToRecover < 0.0f)
                {
                    player.SetNewState(player._neutralState);
                }
                else
                {
                    timeToRecover -= Time.deltaTime;
                }
            }

            public override void ExitState(PlayerController player)
            {
                player.model.SetVulnerable(true);
            }
        }
        
        private class DyingState : PlayerState
        {
            public override void ClimbCommand(PlayerController player)
            {
                
            }

            public override void DiveCommand(PlayerController player)
            {
                
            }
            
            public override void EnterState(PlayerController player)
            {
                currentPlayerState = Weapons.PlayerState.None;
                player.SetDying();
                player.Die();
            }

            public override void UpdateState(PlayerController player)
            {
                player.weapons.WeaponsUpdate(currentPlayerState);
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

            characterConfig = ConfigSelectionMediator.GetCharacterConfiguration();
            
            model.InitializeModel(characterConfig);
            view.Initialize();
            weapons.Initialize(view.Graphic.transform, characterConfig.StartingWeapons, uiView);
            magnet.Initialize(view.Graphic.transform);
            
            _touchReceiver.PointerDown += PointerDownHandler;
            _touchReceiver.PointerUp += PointerUpHandler;
            view.TriggerEntered += TriggerEnterHandler;
            
            uiView.Initialize();
            uiView.UpdatePlayerHealthView((float)model.CurrentHealth/model.MaxHealth);
            uiView.UpdatePlayerXPView(0.0f);
            
            model.HealthPercentChanged += uiView.UpdatePlayerHealthView;
            model.XPPercentChanged += uiView.UpdatePlayerXPView;
            comboService.ComboChanged += HandleComboChanged;
            
            #if COMMAND_TERMINAL
            Terminal.Shell.AddCommand("die", delegate { ChangePlayerHealth(-model.CurrentHealth-1); });
            #endif
            
            float height = view.Body.position.y;
            PlayerHeightChanged?.Invoke(height, Mathf.InverseLerp(movementConfig.MinY, movementConfig.MaxY, height));
        }

        public void SetHasControl(bool value)
        {
            hasControl = value;
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
            
            if (!model.Dead && ((model.CurrentXSpeed > 0.0f && view.Body.position.x > movementConfig.MaxX)
                || (model.CurrentXSpeed < 0.0f && view.Body.position.x < movementConfig.MinX)))
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
            movementVector *= Time.fixedDeltaTime;
            view.Body.MovePosition(view.Body.position + movementVector);
            float height = view.Body.position.y;
            
            if (height != lastHeight)
            {
                lastHeight = height;
                PlayerHeightChanged?.Invoke(height, Mathf.InverseLerp(movementConfig.MinY, movementConfig.MaxY, height));
            }
        }

        private void PointerDownHandler(object sender, PointerEventData eventData)
        {
            inputPressed = true;
        }

        private void PointerUpHandler(object sender, PointerEventData eventData)
        {
            inputPressed = false;
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
            if (GameModel.CurrentGamePhase > GamePhase.HordePhase
                || GameModel.CurrentGamePhase == GamePhase.UpgradePhase) return;
            
            bool levelUp = false;
            model.ChangeXP(value, out levelUp);

            if (levelUp)
            {
                _ = LevelUpHandler();
            }
        }
        
        private void TriggerEnterHandler(object sender, Collider2D other)
        {
            HitTrigger hitTrigger = other.gameObject.GetComponent<HitTrigger>();
            
            if (hitTrigger != null && hitTrigger.enabled)
            {
                hitTrigger.HitReceiver.HitPlayer(ChangePlayerHealth, ChangePlayerXP, SelectedUpgradeHandler);
            }
        }

        private async Awaitable LevelUpHandler()
        {
            uiView.UpdatePlayerCurrentLevelText(model.CurrentLevel);
            LevelUp?.Invoke(model.CurrentLevel);

            while (GameModel.CurrentGamePhase != GamePhase.HordePhase)
            {
                await Awaitable.NextFrameAsync();
            }
            
            uiView.XPReverse();
        }

        private void SelectedUpgradeHandler(UpgradeOption selectedOption)
        {
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
            if (amount <= 0)
            {
                if (!model.Vulnerable) return;
                
                model.ChangeHealth(amount);
                OnPlayerWasDamaged(amount);
            }
            else
            {
                model.ChangeHealth(amount);
            }
        }
        
        private void OnPlayerWasDamaged(int damage)
        {
            if (model.CurrentHealth <= 0.0f)
            {
                SetNewState(_dyingState);
            }
            else
            {
                view.Flash();
                SetNewState(_hurtState);
                PlayerDamaged?.Invoke(damage);
            }
        }

        private void Die()
        {
            if (model.Dead) return;
            
            model.SetVulnerable(false);
            model.SetDead(true);
            PlayerDied?.Invoke();
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
            
            view.Animator.Play(HOVERING_HASH);
        }

        private void SetHurt()
        {
            ySpeedTweener?.Kill();
            ySpeedTweener = DOTween.To(() => model.CurrentYSpeed, y => model.SetYSpeed(y), -movementConfig.ClimbSpeed/2, 0.25f);
            xSpeedTweener?.Kill();
            xSpeedTweener = DOTween.To(() => model.CurrentXSpeed, x => model.SetXSpeed(x), -movementConfig.ForwardSpeed, 0.25f);
            rotationTweener?.Kill();
            rotationTweener = view.Graphic.transform.DORotate(Vector3.zero, 0.25f);
            
            view.Animator.Play(HURT_HASH);
        }
        
        private void SetDying()
        {
            ySpeedTweener?.Kill();
            ySpeedTweener = DOTween.To(() => model.CurrentYSpeed, y => model.SetYSpeed(y), -movementConfig.ClimbSpeed, 0.25f);
            xSpeedTweener?.Kill();
            xSpeedTweener = DOTween.To(() => model.CurrentXSpeed, x => model.SetXSpeed(x), -movementConfig.ForwardSpeed, 0.25f);
            rotationTweener?.Kill();
            rotationTweener = view.Graphic.transform.DORotate(Vector3.zero, 0.25f);
            
            view.Animator.Play(DYING_HASH);
        }

        
    }
}
