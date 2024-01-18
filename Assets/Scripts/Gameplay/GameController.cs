using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using CommandTerminal;
using Configuration;
using DG.Tweening;
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
        private static readonly Vector3[] upgradePickupPositions = new[] { new Vector3(25.0f, 7.7f), new Vector3(25.0f, 1.2f), new Vector3(25.0f, -5.3f) };
        
        [Inject] private readonly EnemiesController enemiesController;
        [Inject] private readonly PickupsController pickupsController;
        [Inject] private readonly PlayerController playerController;
        [Inject] private readonly VFXService vfxService;
        [Inject] private readonly PlayableDirector levelDirector;
        [Inject] private readonly GameplayUIView uiView;
        [Inject] private readonly BurstSignalReceiver burstSignalReceiver;
        
        [Inject] private readonly GameModel gameModel;
        
        private UpgradeTree upgradeTree;
        private BurstDefinition bossBurstDefinition;
        
        private readonly Stack<PickupDropOrder> comboBalloon = new Stack<PickupDropOrder>(16);
        
        #region Life Cycle

        public async void Start()
        {
            uiView.SetFadeAlpha(1.0f, 0.0f);
            
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
            
            #if UNITY_EDITOR
            InitializeTerminalCommands();
            #endif

            await Awaitable.NextFrameAsync();
            
            gameModel.SetGamePhase(GamePhase.IntroPhase);
            
            await Awaitable.WaitForSecondsAsync(0.25f);
            
            uiView.SetFadeAlpha(0.0f, 2.0f);
            uiView.PauseButtonClicked.AddListener(gameModel.TogglePause);
            gameModel.GameSetPaused += GameSetPausedHandler;
        }

        public void Tick()
        {
            if (GameModel.Paused) return;
            
            playerController.DoUpdate();
            enemiesController.DoUpdate();
            pickupsController.DoUpdate();

            Physics2D.Simulate(Time.deltaTime);
        }

        public void FixedTick()
        {
            if (GameModel.Paused) return;
            
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

            bossBurstDefinition = levelConfig.BossEnemy;
            TimelineAsset timeline = levelConfig.Timeline;
            levelDirector.playableAsset = timeline;
            PlayableBinding[] bindings = timeline.outputs.ToArray();

            foreach (PlayableBinding t in bindings)
            {
                levelDirector.SetGenericBinding(t.sourceObject, burstSignalReceiver);
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

        private void EnemyHitHandler(bool killed, int damage, int value, SpriteRenderer[] positions)
        {
            Vector3 samplePosition = positions[Random.Range(0, positions.Length)].transform.position;
            vfxService.RequestDamageTextAt(damage, samplePosition);
            
            if (killed)
            {
                List<Vector3> vectorPositions = new List<Vector3>(positions.Length);

                foreach (SpriteRenderer t in positions)
                {
                    vectorPositions.Add(t.transform.position);
                }
                
                _ = vfxService.RequestExplosionsAt(vectorPositions, true, 0.05f, 0.01f);
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
                    comboBalloon.Push(new PickupDropOrder(pickupValue, type, samplePosition));
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
        
        private async void LevelUpHandler(int newLevel)
        {
            List<UpgradeOption> allCurrentOptions = upgradeTree.GetAllCurrentOptions(newLevel);
            Stack<PickupDropOrder> shortList = new Stack<PickupDropOrder>(4);
            
            for (int i = 0; i < 3; i++)
            {
                if (allCurrentOptions.Count <= 0) break;
                
                UpgradeOption option = allCurrentOptions[Random.Range(0, allCurrentOptions.Count)];
                shortList.Push(new PickupDropOrder(option, PickupType.Upgrade, upgradePickupPositions[i]));
                allCurrentOptions.Remove(option);
            }

            if (shortList.Count <= 0) return;
            
            gameModel.SetGamePhase(GamePhase.UpgradePhase);
            vfxService.ChangeBaselineContrastRange(1.12f);
            vfxService.ChangeBaselineEmission(3.0f);
            
            AudioService.Instance.PlayLevelUp();
            uiView.SetCanvasAlpha(0.0f, 0.5f);
                
            List<Vector3> purgePositions = pickupsController.PurgeAllPickups(PickupType.XP);
            purgePositions.AddRange(enemiesController.PurgeAllEnemies());
                
            _ = vfxService.RequestExplosionsAt(purgePositions);
                
            vfxService.DoCameraShake(purgePositions.Count * 0.1f);

            gameModel.SetCanPause(false);
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.3f, 0.3f);
            await Awaitable.WaitForSecondsAsync(0.3f);
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1.0f, 0.3f);
            gameModel.SetCanPause(true);
                
            ScrolledObjectView[] upgradePickups = pickupsController.SpawnAndReturnPickups(shortList, 0.0f);

            Action finishUpgradeAction = delegate { FinishUpgradePhase(upgradePickups, purgePositions); };
            
            foreach (ScrolledObjectView t in upgradePickups)
            {
                t.Deactivated += finishUpgradeAction;
            }
        }

        private void FinishUpgradePhase(ScrolledObjectView[] upgradePickups, List<Vector3> purgePositions)
        {
            if (GameModel.CurrentGamePhase != GamePhase.UpgradePhase) return;

            gameModel.SetGamePhase(GamePhase.HordePhase);
            vfxService.ChangeBaselineEmission(1.0f);
            vfxService.ChangeBaselineContrastRange(1.0f);
            
            Stack<PickupDropOrder> healthDrops = new Stack<PickupDropOrder>();
            
            foreach (Vector3 position in purgePositions)
            {
                healthDrops.Push(new PickupDropOrder(2, PickupType.Health, position));
            }
            
            pickupsController.SpawnPickups(healthDrops, 1.0f);
            
            foreach (ScrolledObjectView t in upgradePickups)
            {
                if (t.Active)
                {
                    _ = t.Deactivate();
                }
            }
        }

        private void GameOver()
        {
            uiView.SetCanvasAlpha(0.0f, 2.5f);
            
            List<Vector3> positions = pickupsController.PurgeAllPickups();
            
            if (GameModel.Won)
            {
                positions.AddRange(enemiesController.PurgeAllEnemies());
            }
            
            gameModel.SetGamePhase(GameModel.Won ? GamePhase.YouWin : GamePhase.GameOver);
            
            _ = vfxService.RequestExplosionsAt(positions);
            
            GameOverRoutine();
        }
        
        private async void GameOverRoutine()
        {
            AsyncOperation loading = SceneManager.LoadSceneAsync("Menu");
            loading.allowSceneActivation = false;
            
            float delay = GameModel.Won ? 20.0f : 5.0f;
            
            if (!GameModel.Won)
            {
                await Awaitable.WaitForSecondsAsync(1);
                vfxService.ChangeBaselineEmission(100.0f);
            }
            
            await Awaitable.WaitForSecondsAsync(delay);
            loading.allowSceneActivation = true;
        }

        private void GameSetPausedHandler(bool value)
        {
            vfxService.ChangeBaselineTint(value ? new Color(0.25f, 0.0f, 0.0f) : Color.white);
            Time.timeScale = value ? 0.0f : 1.0f;
            uiView.SetShowPausePanel(value);
            AudioService.Instance.HandleSetPaused(value);
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
                    _ = BossRoutine();
                    break;
                case GamePhase.YouWin:
                    gameModel.SetCanPause(false);
                    uiView.SetCanvasAlpha(0.0f, 5.0f);
                    uiView.SetFadeAlpha(1.0f, 10.0f);
                    uiView.ShowGameOverMessage("You Won", 10.0f);
                    levelDirector.Stop();
                    break;
                case GamePhase.GameOver:
                    gameModel.SetCanPause(false);
                    uiView.SetCanvasAlpha(0.0f, 1.5f);
                    uiView.SetFadeAlpha(1.0f, 5.5f);
                    uiView.ShowGameOverMessage("You Died", 5.0f);
                    levelDirector.Stop();
                    break;
                case GamePhase.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(newPhase), newPhase, null);
            }
        }

        private async void TimerRoutine()
        {
            await Awaitable.WaitForSecondsAsync(1);
            
            while (GameModel.TimeLeft > 0)
            {
                while (GameModel.Paused || GameModel.CurrentGamePhase != GamePhase.HordePhase)
                {
                    await Awaitable.NextFrameAsync();
                }
                
                GameModel.ChangeTimeLeft(-1.0f);
                uiView.UpdateTimerText((int)GameModel.TimeLeft);

                await Awaitable.WaitForSecondsAsync(1);
            }
            
            while (GameModel.Paused || GameModel.CurrentGamePhase != GamePhase.HordePhase)
            {
                await Awaitable.NextFrameAsync();
            }
            
            if (!playerController.PlayerIsDead) gameModel.SetGamePhase(GamePhase.BossPhase);
        }

        private async Awaitable BossRoutine()
        {
            AudioService.Instance.PlayLevelUp();
            enemiesController.CancelAllOngoingBursts();

            await Awaitable.NextFrameAsync();
            
            await vfxService.RequestExplosionsAt(enemiesController.PurgeAllEnemies(), true, 0.05f, 0.01f);

            gameModel.SetCanPause(false);
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.3f, 0.3f);
            await Awaitable.WaitForSecondsAsync(0.3f);
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1.0f, 0.3f);
            gameModel.SetCanPause(true);
            
            bool bossDefeated = false;
            List<ScrolledObjectView> bossEnemies = await enemiesController.RequestEnemyBurstAndList(bossBurstDefinition);
            
            while (!bossDefeated && !playerController.PlayerIsDead)
            {
                await Awaitable.WaitForSecondsAsync(0.5f);
                
                bossDefeated = true;

                foreach (ScrolledObjectView bossEnemy in bossEnemies)
                {
                    if (bossEnemy.Active)
                    {
                        bossDefeated = false;
                        break;
                    }
                }
            }
            
            if (bossDefeated && !playerController.PlayerIsDead)
            {
                gameModel.SetWonGame();
                GameOver();
            }
        }
        
        #if UNITY_EDITOR
        private void InitializeTerminalCommands()
        {
            Terminal.Shell.AddCommand("phase", delegate(CommandArg[] args)
            {
                Enum.TryParse(args[0].String, out GamePhase phase);
                gameModel.SetGamePhase(phase);
            });
            Terminal.Shell.AddCommand("gameover", delegate { GameOver(); });
            Terminal.Shell.AddCommand("win",
                delegate
                {
                    gameModel.SetWonGame(); 
                    GameOver(); 
                    
                });
        }
        #endif
    }
}
