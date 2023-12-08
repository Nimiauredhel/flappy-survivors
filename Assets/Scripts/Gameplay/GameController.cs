using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Configuration;
using Gameplay.Level;
using Gameplay.Player;
using Gameplay.ScrolledObjects;
using Gameplay.ScrolledObjects.Enemy;
using Gameplay.ScrolledObjects.Pickup;
using Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
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
        [Inject] private readonly GameplayUIView uiView;
        [Inject] private readonly BurstSignalReceiver burstSignalReceiver;
        
        [Inject] private readonly GameModel gameModel;
        
        private UpgradeTree upgradeTree;
        
        private Stack<PickupDropOrder> comboBalloon = new Stack<PickupDropOrder>(32);

        #region Life Cycle

        public void Start()
        {
            Application.targetFrameRate = 60;
            
            upgradeTree = ConfigSelectionMediator.GetUpgradeTree();
            
            enemiesController.Initialize();
            enemiesController.EnemyHit += EnemyHitHandler;
            
            InitializeLevel();
            
            gameModel.Initialize((float)levelDirector.duration);
            gameModel.GamePhaseChanged += PhaseChangedHandler;
            
            InitPlayerController();
            
            vfxService.Initialize();
            pickupsController.Initialize();
            
            AudioService.Instance.PlayGameplayMusic();
            TimerRoutine();
            gameModel.SetGamePhase(GamePhase.IntroPhase);
            
            uiView.SetFadeAlpha(0.0f, 2.0f, 1.0f);
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
            
            vfxService.Dispose();
        }

        #endregion
        
        #region Initialization

        private void InitializeLevel()
        {
            LevelConfiguration levelConfig = ConfigSelectionMediator.GetLevelConfig();
            
            TimelineAsset timeline = levelConfig.Timeline;
            levelDirector.playableAsset = timeline;
            PlayableBinding[] bindings = timeline.outputs.ToArray();

            for (int i = 0; i < bindings.Length; i++)
            {
                levelDirector.SetGenericBinding(bindings[i].sourceObject, burstSignalReceiver);
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
            vfxService.DoCameraShake(damage * 0.1f);
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
            float refY = Camera.main.transform.position.y;
            Vector3[] positions = new[] { new Vector3(25.0f, refY + 6.7f), new Vector3(25.0f, refY + 0.2f), new Vector3(25.0f, refY - 6.3f) };
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
                vfxService.ChangeBaselineEmission(3.0f);
            
                AudioService.Instance.PlayLevelUp();
                uiView.SetCanvasAlpha(0.0f, 0.5f);
                
                List<Vector3> purgePositions = pickupsController.PurgeAllPickups(PickupType.XP);
                purgePositions.AddRange(enemiesController.PurgeAllEnemies());
                
                vfxService.RequestExplosionsAt(purgePositions);
                
                vfxService.DoCameraShake(purgePositions.Count * 0.1f);
                
                ScrolledObjectView[] upgradePickups = pickupsController.SpawnAndReturnPickups(shortList, 0.0f);

                Action finishUpgradeAction = delegate { FinishUpgradePhase(upgradePickups, purgePositions); };
            
                for (int i = 0; i < upgradePickups.Length; i++)
                {
                    upgradePickups[i].Deactivated += finishUpgradeAction;
                }
            }
        }

        private void FinishUpgradePhase(ScrolledObjectView[] upgradePickups, List<Vector3> purgePositions)
        {
            if (GameModel.CurrentGamePhase != GamePhase.UpgradePhase) return;

            gameModel.SetGamePhase(GamePhase.HordePhase);
            vfxService.ChangeBaselineEmission(1.0f);
            
            Stack<PickupDropOrder> healthDrops = new Stack<PickupDropOrder>();
            
            foreach (Vector3 position in purgePositions)
            {
                healthDrops.Push(new PickupDropOrder(2, PickupType.Health, position));
            }
            
            pickupsController.SpawnPickups(healthDrops, 1.0f);
            
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
            uiView.SetCanvasAlpha(0.0f, 2.5f);
            
            List<Vector3> positions = new List<Vector3>();
            positions = pickupsController.PurgeAllPickups();
            
            if (GameModel.Won)
            {
                positions.AddRange(enemiesController.PurgeAllEnemies());
            }
            
            vfxService.RequestExplosionsAt(positions);

            gameModel.SetGamePhase(GameModel.Won ? GamePhase.YouWin : GamePhase.GameOver);
            GameOverRoutine();
        }
        
        private async void GameOverRoutine()
        {
            AsyncOperation loading = SceneManager.LoadSceneAsync("Menu");
            loading.allowSceneActivation = false;
            
            float delay = GameModel.Won ? 20.0f : 6.0f;
            
            if (!GameModel.Won)
            {
                await Awaitable.WaitForSecondsAsync(1);
                vfxService.ChangeBaselineEmission(100.0f);
            }
            
            await Awaitable.WaitForSecondsAsync(delay);
            loading.allowSceneActivation = true;
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
                    uiView.SetCanvasAlpha(0.0f, 0.0f);
                    break;
                case GamePhase.UpgradePhase:
                    uiView.SetCanvasAlpha(0.0f, 0.5f);
                    levelDirector.Pause();
                    break;
                case GamePhase.HordePhase:
                    uiView.SetCanvasAlpha(1.0f, 2.0f);
                    levelDirector.Play();
                    break;
                case GamePhase.BossPhase:
                    levelDirector.Stop();
                    break;
                case GamePhase.YouWin:
                    uiView.SetCanvasAlpha(0.0f, 0.5f);
                    uiView.SetFadeAlpha(1.0f, 15.0f);
                    uiView.ShowGameOverMessage("You Won", 3.0f);
                    levelDirector.Stop();
                    break;
                case GamePhase.GameOver:
                    uiView.SetCanvasAlpha(0.0f, 0.5f);
                    uiView.SetFadeAlpha(1.0f, 5.0f);
                    uiView.ShowGameOverMessage("You Died", 3.0f);
                    levelDirector.Stop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newPhase), newPhase, null);
            }
        }
        
        private async void TimerRoutine()
        {
            await Awaitable.WaitForSecondsAsync(1);
            
            while (GameModel.TimeLeft > 0)
            {
                while (GameModel.CurrentGamePhase != GamePhase.HordePhase)
                {
                    await Awaitable.NextFrameAsync();
                }
                
                GameModel.ChangeTimeLeft(-1.0f);
                uiView.UpdateTimerText((int)GameModel.TimeLeft);

                await Awaitable.WaitForSecondsAsync(1);
            }
            
            gameModel.SetGamePhase(GamePhase.BossPhase);

            await Awaitable.WaitForSecondsAsync(2);
            gameModel.SetWonGame();
            GameOver();
        }
    }
}
