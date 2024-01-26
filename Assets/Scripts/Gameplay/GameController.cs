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

        private bool transitioning = false;
        private UpgradeTree upgradeTree;
        private BurstDefinition bossBurstDefinition;
        
        private readonly Stack<PickupDropOrder> comboBalloon = new Stack<PickupDropOrder>(16);
        
        #region Life Cycle

        public async void Start()
        {
            Application.targetFrameRate = 60;
            uiView.SetFadeAlpha(1.0f, 0.0f);
            
            upgradeTree = ConfigSelectionMediator.GetUpgradeTree();
            upgradeTree.ResetUpgradeTree();
            ConfigSelectionMediator.GetStartingLoadout().Taken = true;
            
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
            
            playerController.SetHasControl(true);
            
            uiView.SetFadeAlpha(0.0f, 2.0f);
            uiView.PauseButtonClicked.AddListener(gameModel.TogglePause);
            uiView.RestartButtonClicked.AddListener(Restart);
            uiView.QuitButtonClicked.AddListener(Quit);
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
            gameModel.Dispose();
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
            gameModel.OnDealtDamage(damage);
            
            if (killed)
            {
                List<Vector3> vectorPositions = new List<Vector3>(positions.Length);

                foreach (SpriteRenderer t in positions)
                {
                    vectorPositions.Add(t.transform.position);
                }
                
                _ = vfxService.RequestExplosionsAt(vectorPositions, true, 0.05f, 0.01f);
                AudioService.Instance.PlayEnemyDestroyed();
                gameModel.OnDestroyedEnemy(1);
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
            gameModel.OnTookDamage(damage);
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
            vfxService.ChangeOutlineThickness(0.0f);
            
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
            
            if (GameModel.Won)
            {
                _ = vfxService.RequestExplosionsAt(enemiesController.PurgeAllEnemies());
                uiView.SetGamePhaseText("Well done!");
            }
            else
            {
                uiView.SetGamePhaseText("Too bad...");
            }
            
            gameModel.SetGamePhase(GameModel.Won ? GamePhase.YouWin : GamePhase.GameOver);
            playerController.SetHasControl(false);
            
            _ = GameOverRoutine();
        }
        
        private async Awaitable GameOverRoutine()
        {
            GameOverUIView viewPanel = uiView.GameOverUIUIView;
            
            viewPanel.CanvasGroup.alpha = 0.0f;
            viewPanel.gameObject.SetActive(true);
            
            if (GameModel.Won)
            {
                viewPanel.SetUp(true,
                    Restart,
                    Quit);
            }
            else
            {
                viewPanel.SetUp(false,
                    Quit,
                    Restart);
            }
            
            await Awaitable.WaitForSecondsAsync(1.0f);
            
            _ = vfxService.RequestExplosionsAt(pickupsController.PurgeAllPickups());
            await Awaitable.NextFrameAsync();
            
            viewPanel.CanvasGroup.DOFade(1.0f, 1.0f);
            await Awaitable.WaitForSecondsAsync(1.0f);
            
            vfxService.ChangeBaselineEmission(0.5f);

            float lerpValue = 0.0f;
            int totalScore = 1000;
            int displayScore = 0;
            int scoreOrigin = displayScore;
            int modifierOrigin = 0;
            
            int modifierNumber = 0;
            int modifierDisplayNumber = 0;
            
            viewPanel.TotalScoreNumber.text = "0";

            DOTween.To(()=> lerpValue, x=> lerpValue = x, 1.0f, 1.0f);

            while (lerpValue < 1.0f)
            {
                displayScore = (int)Mathf.Lerp(scoreOrigin, totalScore, lerpValue);
                viewPanel.TotalScoreNumber.text = displayScore.ToString();
                await Awaitable.NextFrameAsync();
            }

            displayScore = totalScore;
            viewPanel.TotalScoreNumber.text = displayScore.ToString();

            viewPanel.SmallButton.interactable = true;
            viewPanel.BigButton.interactable = true;
            
            await Awaitable.WaitForSecondsAsync(0.25f);

            // "Enemies Destroyed"
            
            modifierNumber = GameModel.TotalEnemiesDestroyed;
            modifierDisplayNumber = modifierNumber;
            totalScore += modifierNumber * 50;
            
            scoreOrigin = displayScore;
            modifierOrigin = modifierDisplayNumber;
            
            viewPanel.ModifierPrefix.text = "Enemies Destroyed:";
            viewPanel.ModifierNumber.text = modifierOrigin.ToString();

            lerpValue = 0.0f;
            DOTween.To(()=> lerpValue, x=> lerpValue = x, 1.0f, 1.0f);
            
            while (lerpValue < 1.0f)
            {
                displayScore = (int)Mathf.Lerp(scoreOrigin, totalScore, lerpValue);
                modifierDisplayNumber = (int)Mathf.Lerp(modifierOrigin, 0, lerpValue);
                viewPanel.TotalScoreNumber.text = displayScore.ToString();
                viewPanel.ModifierNumber.text = modifierDisplayNumber.ToString();
                await Awaitable.NextFrameAsync();
            }
            
            displayScore = totalScore;
            viewPanel.TotalScoreNumber.text = displayScore.ToString();
            viewPanel.ModifierNumber.text = "0";
            
            await Awaitable.WaitForSecondsAsync(0.25f);
            
            // "Damage Dealt"
            
            modifierNumber = GameModel.TotalDamageDealt;
            modifierDisplayNumber = modifierNumber;
            totalScore += modifierNumber * 10;
            
            scoreOrigin = displayScore;
            modifierOrigin = modifierDisplayNumber;
            
            viewPanel.ModifierPrefix.text = "Damage Dealt:";
            viewPanel.ModifierNumber.text = modifierOrigin.ToString();

            lerpValue = 0.0f;
            DOTween.To(()=> lerpValue, x=> lerpValue = x, 1.0f, 1.0f);
            
            while (lerpValue < 1.0f)
            {
                displayScore = (int)Mathf.Lerp(scoreOrigin, totalScore, lerpValue);
                modifierDisplayNumber = (int)Mathf.Lerp(modifierOrigin, 0, lerpValue);
                viewPanel.TotalScoreNumber.text = displayScore.ToString();
                viewPanel.ModifierNumber.text = modifierDisplayNumber.ToString();
                await Awaitable.NextFrameAsync();
            }
            
            displayScore = totalScore;
            viewPanel.TotalScoreNumber.text = displayScore.ToString();
            viewPanel.ModifierNumber.text = "0";
            
            await Awaitable.WaitForSecondsAsync(0.25f);
            
            // "Damage Taken"
            // Note: the damage taken value is already negative, so we flip it back
            
            modifierNumber = GameModel.TotalDamageTaken * -1;
            modifierDisplayNumber = modifierNumber;
            totalScore -= modifierNumber * 10;
            
            scoreOrigin = displayScore;
            modifierOrigin = modifierDisplayNumber;
            
            viewPanel.ModifierPrefix.text = "Damage Taken:";
            viewPanel.ModifierNumber.text = modifierOrigin.ToString();

            lerpValue = 0.0f;
            DOTween.To(()=> lerpValue, x=> lerpValue = x, 1.0f, 1.0f);
            
            while (lerpValue < 1.0f)
            {
                displayScore = (int)Mathf.Lerp(scoreOrigin, totalScore, lerpValue);
                modifierDisplayNumber = (int)Mathf.Lerp(modifierOrigin, 0, lerpValue);
                viewPanel.TotalScoreNumber.text = displayScore.ToString();
                viewPanel.ModifierNumber.text = modifierDisplayNumber.ToString();
                await Awaitable.NextFrameAsync();
            }
            
            displayScore = totalScore;
            viewPanel.TotalScoreNumber.text = displayScore.ToString();
            viewPanel.ModifierNumber.text = "0";
            
            await Awaitable.WaitForSecondsAsync(0.25f);

            viewPanel.ModifierPrefix.text = "";
            viewPanel.ModifierNumber.text = "";
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
            if (GameModel.CurrentGamePhase == GamePhase.IntroPhase)
            {
                gameModel.SetGamePhase(GamePhase.HordePhase);  
            }
        }

        private void PhaseChangedHandler(GamePhase newPhase)
        {
            AudioService.Instance.HandlePhaseChange(newPhase);
            
            switch (newPhase)
            {
                case GamePhase.IntroPhase:
                    uiView.SetCanvasAlpha(0.0f, 0.0f);
                    uiView.ShowTapText();
                    break;
                case GamePhase.UpgradePhase:
                    uiView.SetGamePhaseText("Ding!");
                    uiView.SetCanvasAlpha(0.0f, 0.5f);
                    vfxService.ChangeBaselineContrastRange(1.12f);
                    vfxService.ChangeBaselineEmission(3.0f);
                    vfxService.ChangeOutlineThickness(1.0f);
                    levelDirector.Pause();
                    break;
                case GamePhase.HordePhase:
                    uiView.SetCanvasAlpha(1.0f, 2.0f);
                    uiView.HideTapText();
                    levelDirector.Play();
                    break;
                case GamePhase.BossPhase:
                    if (levelDirector != null)
                    {
                        levelDirector.Stop();
                    }

                    _ = BossRoutine();
                    break;
                case GamePhase.YouWin:
                    gameModel.SetCanPause(false);
                    uiView.SetCanvasAlpha(0.0f, 5.0f);
                    levelDirector.Stop();
                    break;
                case GamePhase.GameOver:
                    gameModel.SetCanPause(false);
                    uiView.SetCanvasAlpha(0.0f, 1.5f);
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
                
                GameModel.ElapseTime(1.0f);
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
            _ = SurviveBlinkRoutine();
            AudioService.Instance.PlayLevelUp();
            enemiesController.CancelAllOngoingBursts();
            
            await Awaitable.NextFrameAsync();
            await vfxService.RequestExplosionsAt(enemiesController.PurgeAllEnemies(), true, 0.05f, 0.01f);
            await Awaitable.NextFrameAsync();
            await vfxService.RequestExplosionsAt(pickupsController.PurgeAllPickups(PickupType.XP));
            
            gameModel.SetCanPause(false);
            await Awaitable.NextFrameAsync();
            
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

        private async Awaitable SurviveBlinkRoutine()
        {
            string empty = string.Empty;
            
            string[] texts = new[]
            {
                "SURVIVE",
                "SURVIVE",
                "<i>Survive!</i>",
                "</b><i>survivre</i>",
                "sUrvIve",
                "suRviVe",
                "<size=60>s_</size>R<size=60>v</size>IV<size=50>e~",
                "sU<i>rrv</i><size=60>VivE",
                "SURVIVE!",
                "SUR<size=90>V</size>IVE!",
                "SURViVE",
                "!evivruS"
            };
            
            for (int i = 0; i < 120; i++)
            {
                uiView.SetGamePhaseText(texts[Random.Range(0, texts.Length)]);
                await Awaitable.NextFrameAsync();
            }
            
            uiView.SetGamePhaseText(texts[0]);
            await Awaitable.WaitForSecondsAsync(0.25f);
            uiView.SetGamePhaseText(empty);
            await Awaitable.WaitForSecondsAsync(0.75f);
            
            while (GameModel.CurrentGamePhase == GamePhase.BossPhase)
            {
                uiView.SetGamePhaseText(texts[Random.Range(0, texts.Length)]);
                await Awaitable.WaitForSecondsAsync(0.75f);
                if (GameModel.CurrentGamePhase != GamePhase.BossPhase) break;
                uiView.SetGamePhaseText(empty);
                await Awaitable.WaitForSecondsAsync(0.25f);
            }
        }

        private void Restart()
        {
            if (GameModel.Won)
            {
                AudioService.Instance.PlayLevelUp();
            }
            else
            {
                AudioService.Instance.PlayEnemyDestroyed();
            }

            gameModel.SetPaused(false);
            _ = TransitionToScene("Gameplay");
        }

        private void Quit()
        {
            if (GameModel.Won)
            {
                AudioService.Instance.PlayLevelUp();
            }
            else
            {
                AudioService.Instance.PlayEnemyDestroyed();
            }
            
            gameModel.SetPaused(false);
            _ = TransitionToScene("Menu");
        }

        private async Awaitable TransitionToScene(string scene)
        {
            if (transitioning) return;
            transitioning = true;
            
            AsyncOperation loading = SceneManager.LoadSceneAsync(scene);
            loading.allowSceneActivation = false;
            uiView.SetFadeAlpha(1.0f, 1.0f);
            await Awaitable.WaitForSecondsAsync(1.0f);
            DOTween.KillAll();
            await Awaitable.NextFrameAsync();
            transitioning = false;
            loading.allowSceneActivation = true;
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
