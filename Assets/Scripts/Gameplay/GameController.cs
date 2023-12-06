using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Configuration;
using DG.Tweening;
using DG.Tweening.Core;
using Gameplay.Level;
using Gameplay.Player;
using Gameplay.ScrolledObjects;
using Gameplay.ScrolledObjects.Enemy;
using Gameplay.ScrolledObjects.Pickup;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class GameController : IStartable, ITickable, IFixedTickable, IDisposable
    {
        [Inject] private readonly EnemiesController enemiesController;
        [Inject] private readonly PickupsController pickupsController;
        [Inject] private readonly PlayerController playerController;
        [Inject] private readonly VFXService vfxService;
        [Inject] private readonly PlayableDirector levelDirector;
        
        [Inject] private readonly GameModel gameModel;
        
        private UpgradeTree upgradeTree;
        private Camera gameplayCamera;
        private Tween cameraShake = null;
        
        private Stack<PickupDropOrder> comboBalloon = new Stack<PickupDropOrder>(32);

        #region Life Cycle

        public void Start()
        {
            Application.targetFrameRate = 60;
            gameplayCamera = Camera.main;
            cameraShake = DOTween.Sequence();
            
            upgradeTree = ConfigSelectionMediator.GetUpgradeTree();
            
            InitializeLevel();
            
            gameModel.Initialize((float)levelDirector.duration);
            gameModel.GamePhaseChanged += PhaseChangedHandler;
            
            InitPlayerController();
            
            enemiesController.Initialize();
            enemiesController.EnemyHit += EnemyHitHandler;
            
            vfxService.Initialize();
            pickupsController.Initialize();
            
            AudioService.Instance.PlayGameplayMusic();
            
            gameModel.SetGamePhase(GamePhase.IntroPhase);
        }

        public void Tick()
        {
            playerController.DoUpdate();
            enemiesController.DoUpdate();
            pickupsController.DoUpdate();

            Physics2D.Simulate(Time.deltaTime);
        }

        public void FixedTick()
        {
            playerController.DoFixedUpdate();
            enemiesController.DoFixedUpdate();
            pickupsController.DoFixedUpdate();
        }

        public void Dispose()
        {
            AudioService.Instance.ReleaseGameplayMusic();
            
            playerController.OnDispose();
            playerController.ComboBreak -= ComboBrokenHandler;
            playerController.LevelUp -= LevelUpHandler;
            playerController.PlayerDamaged -= PlayerDamagedHandler;
            playerController.PlayerDied -= GameOver;
            playerController.PlayerStartedMoving -= PlayerStartedMovingHandler;
            
            enemiesController.EnemyHit -= EnemyHitHandler;
        }

        #endregion
        
        #region Initialization

        private void InitializeLevel()
        {
            LevelConfiguration levelConfig = ConfigSelectionMediator.GetLevelConfig();
            
            TimelineAsset timeline = levelConfig.Timeline;
            levelDirector.playableAsset = timeline;
            PlayableBinding[] bindings = timeline.outputs.ToArray();
            BurstSignalReceiver receiver = enemiesController.GetComponent<BurstSignalReceiver>();

            for (int i = 0; i < bindings.Length; i++)
            {
                levelDirector.SetGenericBinding(bindings[i].sourceObject, receiver);
            }

            Object.Instantiate(levelConfig.BackgroundAsset);
        }

        private void InitPlayerController()
        {
            playerController.Initialize();
            playerController.ComboBreak += ComboBrokenHandler;
            playerController.LevelUp += LevelUpHandler;
            playerController.PlayerDamaged += PlayerDamagedHandler;
            playerController.PlayerDied += GameOver;
            playerController.PlayerStartedMoving += PlayerStartedMovingHandler;
        }

        #endregion

        private void EnemyHitHandler(bool killed, int damage, int value, Vector3 position)
        {
            vfxService.RequestDamageTextAt(damage, position);
            
            if (killed)
            {
                vfxService.RequestExplosionAt(position);
                AudioService.Instance.PlayEnemyDestroyed();
                playerController.HandleEnemyKilled();

                if (value != 0)
                {
                    PickupType type;

                    if (value < 0)
                    {
                        type = PickupType.Health;
                        value *= -1;
                    }
                    else
                    {
                        type = PickupType.XP;
                    }

                    int pickupValue = Random.Range(Mathf.CeilToInt(value * 0.55f), value);
                    comboBalloon.Push(new PickupDropOrder(pickupValue, type, position));
                }
            }
            else
            {
                AudioService.Instance.PlayEnemyHit();
            }
        }
        
        private void PlayerDamagedHandler(int damage)
        {
            if (cameraShake != null) cameraShake.Kill(true);
            
            Vector3 strength = new Vector3(1, 1, 0) * (damage / 10.0f);
            
            cameraShake = gameplayCamera.DOShakePosition(0.25f, strength);
            cameraShake.onComplete += () => cameraShake.Rewind();
        }

        private void ComboBrokenHandler(int brokenCombo)
        {
            Stack<PickupDropOrder> pickupsToDrop = new Stack<PickupDropOrder>(comboBalloon);
            comboBalloon.Clear();

            float comboModifier = Constants.MapFloat(0, 100, 1.0f, 2.0f, brokenCombo);
            
            pickupsController.SpawnPickups(pickupsToDrop, comboModifier);
        }

        private void LevelUpHandler(int newLevel)
        {
            Vector3[] positions = new[] { new Vector3(25.0f, 5.5f), new Vector3(25.0f, -0.75f), new Vector3(25.0f, -7.0f) };
            List<UpgradeOption> allCurrentOptions = upgradeTree.GetAllCurrentOptions(newLevel);
            Stack<PickupDropOrder> shortList = new Stack<PickupDropOrder>(4);
            
            for (int i = 0; i < 3; i++)
            {
                if (allCurrentOptions.Count <= 0) break;
                
                UpgradeOption option = allCurrentOptions[Random.Range(0, allCurrentOptions.Count)];
                shortList.Push(new PickupDropOrder(option, PickupType.Upgrade, positions[i]));
                allCurrentOptions.Remove(option);
            }

            if (shortList.Count > 0)
            {
                gameModel.SetGamePhase(GamePhase.UpgradePhase);
            
                pickupsController.PurgeAllPickups(true);
                
                ScrolledObjectView[] upgradePickups = pickupsController.SpawnAndReturnPickups(shortList, 0.0f);

                Action finishUpgradeAction = delegate { FinishUpgradePhase(upgradePickups); };
            
                for (int i = 0; i < upgradePickups.Length; i++)
                {
                    upgradePickups[i].Deactivated += finishUpgradeAction;
                }
            }
        }

        private void FinishUpgradePhase(ScrolledObjectView[] upgradePickups)
        {
            if (GameModel.CurrentGamePhase != GamePhase.UpgradePhase) return;

            gameModel.SetGamePhase(GamePhase.HordePhase);

            for (int i = 0; i < upgradePickups.Length; i++)
            {
                if (upgradePickups[i].Active)
                {
                    upgradePickups[i].Deactivate();
                }
            }
        }

        private void GameOver()
        {
            gameModel.SetGamePhase(GameModel.Won ? GamePhase.YouWin : GamePhase.GameOver);
            EmptyMono tempMB = new GameObject().AddComponent<EmptyMono>();
            tempMB.StartCoroutine(GameOverRoutine());
        }
        
        private IEnumerator GameOverRoutine()
        {
            float delay = GameModel.Won ? 15.0f : 5.0f;
            yield return new WaitForSeconds(delay);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }

        private void PlayerStartedMovingHandler()
        {
            gameModel.SetGamePhase(GamePhase.HordePhase);
        }

        private void PhaseChangedHandler(GamePhase newPhase)
        {
            AudioService.Instance.HandlePhaseChange(newPhase);
            
            switch (newPhase)
            {
                case GamePhase.IntroPhase:
                    break;
                case GamePhase.UpgradePhase:
                    levelDirector.Pause();
                    break;
                case GamePhase.HordePhase:
                    levelDirector.Play();
                    break;
                case GamePhase.BossPhase:
                    levelDirector.Stop();
                    break;
                case GamePhase.YouWin:
                    levelDirector.Stop();
                    break;
                case GamePhase.GameOver:
                    levelDirector.Stop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newPhase), newPhase, null);
            }
        }
    }
}
